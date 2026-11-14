using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using HTM_PRO.Config;
using HTM_PRO.Core;
using HTM_PRO.Utils;

namespace HTM_PRO.UI
{
    /// <summary>
    /// Interactive 5-step Stage 1 Machine Configuration Wizard for HTM_PRO.
    /// Manages controller resource budgeting (axes, Output pins, Input pins),
    /// gantry Master/Slave topology, wiring strategies, soft limits, and Stage 2 ATC readiness.
    /// </summary>
    public class MachineSetupWizardForm : Form
    {
        private readonly ProfileService _profileService;
        private readonly Form _uccncMainForm;
        private readonly MachineConfig _workingConfig;

        private readonly Color _bgDark = Color.FromArgb(28, 31, 38);
        private readonly Color _bgCard = Color.FromArgb(39, 43, 52);
        private readonly Color _bgInput = Color.FromArgb(48, 53, 64);
        private readonly Color _borderCol = Color.FromArgb(66, 72, 86);
        private readonly Color _textPrimary = Color.FromArgb(240, 243, 248);
        private readonly Color _textMuted = Color.FromArgb(165, 175, 192);
        private readonly Color _accentBlue = Color.FromArgb(47, 128, 237);
        private readonly Color _accentGreen = Color.FromArgb(39, 174, 96);
        private readonly Color _accentAmber = Color.FromArgb(242, 153, 74);
        private readonly Color _accentRed = Color.FromArgb(235, 87, 87);

        private int _currentStep = 0; // 0..4
        private readonly string[] _stepTitles =
        {
            "1. Environment",
            "2. Axes & Gantry",
            "3. Motor Outputs",
            "4. Home & Limits",
            "5. I/O & Summary"
        };

        private Button[] _stepNavButtons;
        private Label _lblResourceBar;
        private Panel _contentHost;
        private Button _btnBack;
        private Button _btnNext;
        private Button _btnApplySave;
        private Button _btnExportSummary;
        private Label _lblStatusFooter;

        private bool _isBinding = false;

        // Reusable pin item wrapper for filtered dropdowns
        private sealed class PinChoice
        {
            public int Port { get; }
            public int Pin { get; }
            public string Label { get; }

            public PinChoice(int port, int pin, string label = null)
            {
                Port = port;
                Pin = pin;
                Label = label ?? (port == 0 && pin == 0 ? "Unassigned (0/0)" : $"Port {port} - Pin {pin}");
            }

            public override string ToString() => Label;
        }

        public event Action OnStage1Saved;

        public MachineSetupWizardForm(ProfileService profileService, Form uccncMainForm)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _uccncMainForm = uccncMainForm;
            _workingConfig = CloneConfig(_profileService.Config);
            _workingConfig.EnsureSixAxes();

            InitializeWindowChrome();
            RenderCurrentStep();
            RefreshResourceStatusBar();
        }

        private void InitializeWindowChrome()
        {
            Text = $"{PluginVersion.FullTitle} — Stage 1: Machine Configuration Wizard [{_profileService.ProfileName}]";
            Size = new Size(1100, 740);
            MinimumSize = new Size(1020, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = _bgDark;
            ForeColor = _textPrimary;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.Sizable;

            // Header container
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 104,
                BackColor = Color.FromArgb(22, 24, 30),
                Padding = new Padding(16, 10, 16, 8)
            };

            var lblTitle = new Label
            {
                Text = $"{PluginVersion.FullTitle}  |  Machine Setup Wizard (Stage 1 of 2)",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 8)
            };

            // Step indicator bar
            var stepBar = new FlowLayoutPanel
            {
                Location = new Point(14, 38),
                Size = new Size(1050, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            _stepNavButtons = new Button[_stepTitles.Length];
            for (int i = 0; i < _stepTitles.Length; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = _stepTitles[i],
                    Width = 196,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Margin = new Padding(0, 0, 8, 0),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => NavigateToStep(idx);
                _stepNavButtons[i] = btn;
                stepBar.Controls.Add(btn);
            }

            // Live resource capacity telemetry strip
            _lblResourceBar = new Label
            {
                Location = new Point(16, 76),
                Size = new Size(1050, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 210, 255)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(stepBar);
            headerPanel.Controls.Add(_lblResourceBar);

            // Footer container
            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 62,
                BackColor = Color.FromArgb(22, 24, 30),
                Padding = new Padding(16, 12, 16, 12)
            };

            _btnExportSummary = CreateButton("Export Pinout (.txt)", Color.FromArgb(58, 65, 78), 165, 36);
            _btnExportSummary.Location = new Point(16, 13);
            _btnExportSummary.Click += (s, e) => ShowExportSummaryModal();

            _lblStatusFooter = new Label
            {
                Location = new Point(195, 20),
                Size = new Size(420, 22),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = _textMuted,
                Text = "Stage 2 (ATC Setup) unlocks automatically after Stage 1 is applied."
            };

            _btnBack = CreateButton("< Back", Color.FromArgb(62, 68, 82), 100, 36);
            _btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnBack.Location = new Point(footerPanel.Width - 460, 13);
            _btnBack.Click += (s, e) => NavigateToStep(_currentStep - 1);

            _btnNext = CreateButton("Next >", _accentBlue, 110, 36);
            _btnNext.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnNext.Location = new Point(footerPanel.Width - 350, 13);
            _btnNext.Click += (s, e) => NavigateToStep(_currentStep + 1);

            _btnApplySave = CreateButton("Apply & Save Stage 1", _accentGreen, 210, 36);
            _btnApplySave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnApplySave.Location = new Point(footerPanel.Width - 230, 13);
            _btnApplySave.Click += BtnApplySave_Click;

            footerPanel.Controls.AddRange(new Control[] { _btnExportSummary, _lblStatusFooter, _btnBack, _btnNext, _btnApplySave });

            // Scrollable main content host
            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(18, 12, 18, 12),
                BackColor = _bgDark
            };

            Controls.Add(_contentHost);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
        }

        private ControllerHardwareSpec CurrentSpec =>
            HardwareCatalog.GetSpec(HardwareCatalog.ParseModel(_workingConfig.ControllerType));

        private void NavigateToStep(int stepIdx)
        {
            if (stepIdx < 0 || stepIdx >= _stepTitles.Length) return;
            _currentStep = stepIdx;
            RenderCurrentStep();
        }

        private void RenderCurrentStep()
        {
            UpdateStepNavVisuals();

            _contentHost.SuspendLayout();
            _contentHost.Controls.Clear();

            Panel stepPanel = _currentStep switch
            {
                0 => BuildStep1EnvironmentPanel(),
                1 => BuildStep2AxesGantryPanel(),
                2 => BuildStep3MotorOutputsPanel(),
                3 => BuildStep4HomeLimitsPanel(),
                _ => BuildStep5IoAndSummaryPanel()
            };

            stepPanel.Dock = DockStyle.Top;
            stepPanel.AutoSize = true;
            stepPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _contentHost.Controls.Add(stepPanel);
            _contentHost.ResumeLayout();

            _btnBack.Enabled = _currentStep > 0;
            _btnNext.Enabled = _currentStep < _stepTitles.Length - 1;
            RefreshResourceStatusBar();
        }

        private void UpdateStepNavVisuals()
        {
            for (int i = 0; i < _stepNavButtons.Length; i++)
            {
                bool active = i == _currentStep;
                _stepNavButtons[i].BackColor = active ? _accentBlue : (i < _currentStep ? Color.FromArgb(40, 82, 58) : _bgCard);
                _stepNavButtons[i].ForeColor = active || i < _currentStep ? Color.White : _textMuted;
                _stepNavButtons[i].FlatAppearance.BorderColor = active ? Color.FromArgb(110, 175, 255) : _borderCol;
            }
        }

        private void RefreshResourceStatusBar()
        {
            var spec = CurrentSpec;
            var report = PinAllocator.ValidateAndAudit(_workingConfig, spec);
            string unit = _workingConfig.IsImperial ? "Imperial (in)" : "Metric (mm)";
            string conflictsText = report.Conflicts.Count > 0 || report.InvalidPins.Count > 0
                ? $"  |  ISSUES: {report.Conflicts.Count + report.InvalidPins.Count}"
                : "  |  Pins OK";

            _lblResourceBar.Text =
                $"Controller: {spec.Model} ({spec.TotalOutputs} OUT / {spec.TotalInputs} IN)  |  Units: {unit}  |  " +
                $"Outputs: {report.UsedOutputCount} used / {report.FreeOutputCount} free  |  " +
                $"Inputs: {report.UsedInputCount} used / {report.FreeInputCount} free  |  " +
                $"Free Axis Channels: {(report.FreeAxisChannels.Count > 0 ? string.Join(",", report.FreeAxisChannels) : "None")}" +
                conflictsText;

            _lblResourceBar.ForeColor = (report.Conflicts.Count > 0 || report.InvalidPins.Count > 0)
                ? _accentRed
                : (report.FreeInputCount < 2 || report.FreeOutputCount < 2 ? _accentAmber : Color.FromArgb(130, 215, 255));
        }

        // =========================================================================
        // STEP 1: ENVIRONMENT, HARDWARE CONTROLLER & UNITS
        // =========================================================================
        private Panel BuildStep1EnvironmentPanel()
        {
            var root = CreateStepContainer("Step 1: Controller Hardware, Measurement Units & Setup Mode",
                "HTM_PRO detects your UCCNC profile units and hardware controller so all subsequent pins and limits match physical hardware capabilities.");

            _isBinding = true;
            try
            {
                var detected = HardwareCatalog.DetectActiveController(_uccncMainForm);
                var cardHw = CreateCardPanel("Motion Controller & Environment", 210);

                AddFormLabel(cardHw, "Active UCCNC Profile:", 18, 42);
                AddFormValue(cardHw, _profileService.ProfileName, 240, 42, Color.White);

                AddFormLabel(cardHw, "Detected Interface:", 18, 74);
                string devNote = detected.IsDemoMode
                    ? $"{detected.RawDeviceType} (Demo mode active — you may select your target controller below)"
                    : $"{detected.RawDeviceType} (Connected Hardware)";
                AddFormValue(cardHw, devNote, 240, 74, detected.IsDemoMode ? _accentAmber : _accentGreen);

                AddFormLabel(cardHw, "Target Controller Model:", 18, 108);
                var cmbController = new ComboBox
                {
                    Location = new Point(240, 104),
                    Width = 360,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = _bgInput,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                var specs = HardwareCatalog.GetAllSpecs().ToList();
                foreach (var s in specs)
                {
                    cmbController.Items.Add(s.DisplayName);
                }
                int selectedIdx = specs.FindIndex(s => s.Model.ToString() == _workingConfig.ControllerType);
                cmbController.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                var lblSpecNotes = new Label
                {
                    Location = new Point(240, 138),
                    Size = new Size(760, 42),
                    ForeColor = _textMuted,
                    Text = CurrentSpec.Notes
                };

                cmbController.SelectedIndexChanged += (s, e) =>
                {
                    if (_isBinding || cmbController.SelectedIndex < 0) return;
                    var chosen = specs[cmbController.SelectedIndex];
                    _workingConfig.ControllerType = chosen.Model.ToString();
                    lblSpecNotes.Text = chosen.Notes;
                    if (_workingConfig.UseFreshSetupDefaults)
                    {
                        PinAllocator.AutoAssignOutputs(_workingConfig, chosen);
                        PinAllocator.AutoAssignInputs(_workingConfig, chosen);
                    }
                    RefreshResourceStatusBar();
                };

                cardHw.Controls.Add(cmbController);
                cardHw.Controls.Add(lblSpecNotes);
                root.Controls.Add(cardHw);

                // Card 2: Measurement Units & Startup Mode
                var cardMode = CreateCardPanel("Measurement Units & Setup Initialization Mode", 225);

                AddFormLabel(cardMode, "Machine Measurement Units:", 18, 42);
                var rbMetric = CreateRadioButton("Metric (Millimeters - mm)", 240, 40, 220, !_workingConfig.IsImperial);
                var rbImperial = CreateRadioButton("Imperial (Decimal Inches - in)", 475, 40, 260, _workingConfig.IsImperial);

                rbMetric.CheckedChanged += (s, e) =>
                {
                    if (_isBinding || !rbMetric.Checked) return;
                    _workingConfig.ApplyUnitBaselineDefaults(false);
                    RefreshResourceStatusBar();
                };
                rbImperial.CheckedChanged += (s, e) =>
                {
                    if (_isBinding || !rbImperial.Checked) return;
                    _workingConfig.ApplyUnitBaselineDefaults(true);
                    RefreshResourceStatusBar();
                };

                var lblUnitWarn = new Label
                {
                    Location = new Point(240, 68),
                    Size = new Size(750, 22),
                    ForeColor = _textMuted,
                    Text = "Match the measurement system chosen when UCCNC was first installed/created."
                };

                AddFormLabel(cardMode, "Initialization Mode:", 18, 102);
                var rbFresh = CreateRadioButton("Fresh Guided Machine Setup (Auto-assign optimal defaults & pins — recommended for new install)",
                    240, 100, 740, _workingConfig.UseFreshSetupDefaults);
                var rbExisting = CreateRadioButton("Import & Review Existing UCCNC Profile Settings (Preserve calibrated Steps/unit & existing pins)",
                    240, 128, 740, !_workingConfig.UseFreshSetupDefaults);

                rbFresh.CheckedChanged += (s, e) =>
                {
                    if (_isBinding || !rbFresh.Checked) return;
                    _workingConfig.UseFreshSetupDefaults = true;
                    _workingConfig.ApplyUnitBaselineDefaults(_workingConfig.IsImperial);
                    PinAllocator.AutoAssignOutputs(_workingConfig, CurrentSpec);
                    PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                    RefreshResourceStatusBar();
                };
                rbExisting.CheckedChanged += (s, e) =>
                {
                    if (_isBinding || !rbExisting.Checked) return;
                    _workingConfig.UseFreshSetupDefaults = false;
                    _profileService.ImportExistingUccncProfileSettings(_workingConfig);
                    RefreshResourceStatusBar();
                };

                var chkReserveAtc = new CheckBox
                {
                    Location = new Point(240, 168),
                    Size = new Size(740, 26),
                    ForeColor = Color.FromArgb(145, 225, 175),
                    Checked = _workingConfig.ReserveResourcesForAtc,
                    Text = "Plan for Stage 2 ATC Module (keep Gantry Slave off A-axis if needed & monitor free ATC inputs/outputs)"
                };
                chkReserveAtc.CheckedChanged += (s, e) =>
                {
                    _workingConfig.ReserveResourcesForAtc = chkReserveAtc.Checked;
                    RefreshResourceStatusBar();
                };

                cardMode.Controls.AddRange(new Control[] { rbMetric, rbImperial, lblUnitWarn, rbFresh, rbExisting, chkReserveAtc });
                root.Controls.Add(cardMode);
            }
            finally
            {
                _isBinding = false;
            }

            return root;
        }

        // =========================================================================
        // STEP 2: AXES TOPOLOGY, MASTER/SLAVE GANTRY & TRAVEL ENVELOPE
        // =========================================================================
        private Panel BuildStep2AxesGantryPanel()
        {
            string unit = _workingConfig.IsImperial ? "in" : "mm";
            var root = CreateStepContainer("Step 2: Axis Topology, Master/Slave Gantry & Travel Limits",
                "Configure active linear/rotary axes, dual-motor gantry slaving, kinematics, and physical travel to compute Safe Soft Limits automatically.");

            _isBinding = true;
            try
            {
                // Top card: Quick Gantry & Rotary topology selector
                var cardTopo = CreateCardPanel("Gantry Master/Slave & Auxiliary Rotary Axes", 125);

                string ySlave = GetSlaveChannelForMaster("Y");
                string xSlave = GetSlaveChannelForMaster("X");

                AddFormLabel(cardTopo, "Y-Axis Gantry Slave:", 18, 40);
                var cmbYSlave = CreateCompactCombo(190, 36, 150, new[] { "None", "C (Recommended)", "B", "A" });
                cmbYSlave.SelectedIndex = ySlave == "C" ? 1 : (ySlave == "B" ? 2 : (ySlave == "A" ? 3 : 0));

                AddFormLabel(cardTopo, "X-Axis Gantry Slave:", 370, 40);
                var cmbXSlave = CreateCompactCombo(530, 36, 130, new[] { "None", "B", "C", "A" });
                cmbXSlave.SelectedIndex = xSlave == "B" ? 1 : (xSlave == "C" ? 2 : (xSlave == "A" ? 3 : 0));

                var lblTopoHint = new Label
                {
                    Location = new Point(18, 76),
                    Size = new Size(990, 38),
                    ForeColor = _textMuted,
                    Text = "Tip: Assigning Y-Slave to channel C keeps A free for a 4th rotary work axis and leaves B/A available if Stage 2 uses a Rotary Tool Magazine."
                };

                cardTopo.Controls.AddRange(new Control[] { cmbYSlave, cmbXSlave, lblTopoHint });
                root.Controls.Add(cardTopo);

                // Axes kinematics table card
                var cardAxes = CreateCardPanel($"Active Axis Channels & Travel ({unit})", 345);
                int y = 40;
                AddColHeader(cardAxes, "Axis", 16, y, 45);
                AddColHeader(cardAxes, "Enable", 65, y, 55);
                AddColHeader(cardAxes, "Channel Role", 125, y, 165);
                AddColHeader(cardAxes, $"Steps / {unit}", 300, y, 105);
                AddColHeader(cardAxes, $"Max Vel ({unit}/m)", 415, y, 115);
                AddColHeader(cardAxes, $"Accel ({unit}/s²)", 540, y, 105);
                AddColHeader(cardAxes, $"Travel ({unit})", 655, y, 100);
                AddColHeader(cardAxes, "Invert Dir", 765, y, 75);
                AddColHeader(cardAxes, "Rotary / Rollover", 848, y, 160);
                y += 28;

                // Helper to apply slave changes cleanly and rebuild Step 2
                void ApplyGantryChoice(string masterAxis, ComboBox srcCombo)
                {
                    if (_isBinding) return;
                    string raw = srcCombo.SelectedItem?.ToString() ?? "None";
                    string chosenChannel = raw.StartsWith("C") ? "C" : raw.StartsWith("B") ? "B" : raw.StartsWith("A") ? "A" : "None";

                    foreach (var ax in _workingConfig.Axes.Where(a => a.Role == AxisRole.GantrySlave && a.MasterAxis == masterAxis))
                    {
                        ax.Role = AxisRole.Unused;
                        ax.MasterAxis = "None";
                        ax.Enabled = false;
                    }

                    if (chosenChannel != "None")
                    {
                        var targetAx = _workingConfig.GetAxis(chosenChannel);
                        targetAx.Role = AxisRole.GantrySlave;
                        targetAx.MasterAxis = masterAxis;
                        targetAx.Enabled = true;
                        targetAx.IsRotary = false;
                        targetAx.RotaryRollover = false;
                    }

                    _workingConfig.SyncSlaveAxisParameters();
                    if (_workingConfig.UseFreshSetupDefaults)
                    {
                        PinAllocator.AutoAssignOutputs(_workingConfig, CurrentSpec);
                        PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                    }
                    RenderCurrentStep();
                }

                cmbYSlave.SelectedIndexChanged += (s, e) => ApplyGantryChoice("Y", cmbYSlave);
                cmbXSlave.SelectedIndexChanged += (s, e) => ApplyGantryChoice("X", cmbXSlave);

                foreach (var ax in _workingConfig.Axes)
                {
                    bool isSlave = ax.Role == AxisRole.GantrySlave;

                    var lblAx = new Label
                    {
                        Text = ax.AxisName,
                        Location = new Point(20, y + 6),
                        Size = new Size(40, 22),
                        Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                        ForeColor = ax.Enabled ? _accentBlue : _textMuted
                    };

                    var chkEn = new CheckBox
                    {
                        Location = new Point(78, y + 6),
                        Size = new Size(24, 22),
                        Checked = ax.Enabled,
                        Enabled = !isSlave && ax.AxisName != "X" && ax.AxisName != "Y" && ax.AxisName != "Z"
                    };

                    string roleDisplay = ax.Role switch
                    {
                        AxisRole.PrimaryLinear => "Primary Linear",
                        AxisRole.GantrySlave => $"Slave -> {ax.MasterAxis}",
                        AxisRole.RotaryWorkAxis => "Rotary Work Axis",
                        AxisRole.ReservedForAtcRotary => "Reserved (ATC)",
                        _ => ax.IsRotaryChannel ? "Unused (Free for ATC)" : "Unused"
                    };
                    var lblRole = new Label
                    {
                        Text = roleDisplay,
                        Location = new Point(125, y + 6),
                        Size = new Size(165, 22),
                        ForeColor = isSlave ? _accentAmber : (ax.Enabled ? Color.White : _textMuted)
                    };

                    var tbSteps = CreateNumericBox(300, y + 2, 100, ax.StepsPerUnit.ToString("0.####", CultureInfo.InvariantCulture), !isSlave && ax.Enabled);
                    var tbVel = CreateNumericBox(415, y + 2, 110, ax.MaxVelocity.ToString("0.##", CultureInfo.InvariantCulture), !isSlave && ax.Enabled);
                    var tbAcc = CreateNumericBox(540, y + 2, 100, ax.Acceleration.ToString("0.##", CultureInfo.InvariantCulture), !isSlave && ax.Enabled);
                    var tbTravel = CreateNumericBox(655, y + 2, 95, ax.MaxTravel.ToString("0.##", CultureInfo.InvariantCulture), !isSlave && ax.Enabled);

                    var chkInvDir = new CheckBox
                    {
                        Location = new Point(788, y + 6),
                        Size = new Size(24, 22),
                        Checked = ax.DirNegate,
                        Enabled = ax.Enabled
                    };

                    chkEn.CheckedChanged += (s, e) =>
                    {
                        if (_isBinding) return;
                        ax.Enabled = chkEn.Checked;
                        ax.Role = ax.Enabled
                            ? (ax.IsRotaryChannel ? AxisRole.RotaryWorkAxis : AxisRole.PrimaryLinear)
                            : AxisRole.Unused;
                        if (_workingConfig.UseFreshSetupDefaults)
                        {
                            PinAllocator.AutoAssignOutputs(_workingConfig, CurrentSpec);
                            PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                        }
                        RenderCurrentStep();
                    };

                    BindDoubleInput(tbSteps, val => { ax.StepsPerUnit = Math.Max(0.001, val); _workingConfig.SyncSlaveAxisParameters(); });
                    BindDoubleInput(tbVel, val => { ax.MaxVelocity = Math.Max(0.1, val); _workingConfig.SyncSlaveAxisParameters(); });
                    BindDoubleInput(tbAcc, val => { ax.Acceleration = Math.Max(0.1, val); _workingConfig.SyncSlaveAxisParameters(); });
                    BindDoubleInput(tbTravel, val =>
                    {
                        ax.MaxTravel = Math.Max(0.0, val);
                        ax.RecalculateSoftLimitsFromTravel();
                        _workingConfig.SyncSlaveAxisParameters();
                    });
                    chkInvDir.CheckedChanged += (s, e) => ax.DirNegate = chkInvDir.Checked;

                    cardAxes.Controls.AddRange(new Control[] { lblAx, chkEn, lblRole, tbSteps, tbVel, tbAcc, tbTravel, chkInvDir });

                    if (ax.IsRotaryChannel)
                    {
                        var chkRot = new CheckBox
                        {
                            Text = "Rotary",
                            Location = new Point(848, y + 5),
                            Size = new Size(72, 22),
                            ForeColor = _textMuted,
                            Checked = ax.IsRotary,
                            Enabled = ax.Enabled && !isSlave
                        };
                        var chkRoll = new CheckBox
                        {
                            Text = "360°",
                            Location = new Point(924, y + 5),
                            Size = new Size(65, 22),
                            ForeColor = _textMuted,
                            Checked = ax.RotaryRollover,
                            Enabled = ax.Enabled && !isSlave && ax.IsRotary
                        };
                        chkRot.CheckedChanged += (s, e) =>
                        {
                            ax.IsRotary = chkRot.Checked;
                            chkRoll.Enabled = ax.IsRotary;
                        };
                        chkRoll.CheckedChanged += (s, e) =>
                        {
                            ax.RotaryRollover = chkRoll.Checked;
                            ax.RecalculateSoftLimitsFromTravel();
                        };
                        cardAxes.Controls.Add(chkRot);
                        cardAxes.Controls.Add(chkRoll);
                    }

                    y += 40;
                }

                root.Controls.Add(cardAxes);
            }
            finally
            {
                _isBinding = false;
            }

            return root;
        }

        // =========================================================================
        // STEP 3: MOTOR OUTPUTS (STEP / DIR / ENABLE)
        // =========================================================================
        private Panel BuildStep3MotorOutputsPanel()
        {
            var spec = CurrentSpec;
            var root = CreateStepContainer("Step 3: Motor Output Allocation (Step / Dir / Enable)",
                "Dropdowns below ONLY list physical OUTPUT pins valid on the selected controller. Sharing one Enable pin saves valuable outputs for ATC solenoids.");

            _isBinding = true;
            try
            {
                var cardStrat = CreateCardPanel("Enable Pin Strategy & Auto-Assignment", 125);
                AddFormLabel(cardStrat, "Driver Enable Wiring:", 18, 40);

                var rbSharedEna = CreateRadioButton("Single Shared Enable Pin (Recommended — uses only 1 Output total)", 200, 38, 430,
                    _workingConfig.EnableStrategy == EnablePinStrategy.SharedSinglePin);
                var rbIndivEna = CreateRadioButton("Individual Enable Pin per Motor", 200, 64, 300,
                    _workingConfig.EnableStrategy == EnablePinStrategy.IndividualPerAxis);
                var rbNoEna = CreateRadioButton("Externally Enabled (0 Output Pins)", 200, 90, 300,
                    _workingConfig.EnableStrategy == EnablePinStrategy.ExternalNoPin);

                var btnAutoOut = CreateButton("Smart Auto-Assign Outputs", _accentBlue, 220, 34);
                btnAutoOut.Location = new Point(775, 42);
                btnAutoOut.Click += (s, e) =>
                {
                    PinAllocator.AutoAssignOutputs(_workingConfig, CurrentSpec);
                    RenderCurrentStep();
                };

                void SetEnableStrat(EnablePinStrategy st)
                {
                    if (_isBinding) return;
                    _workingConfig.EnableStrategy = st;
                    PinAllocator.AutoAssignOutputs(_workingConfig, CurrentSpec);
                    RenderCurrentStep();
                }
                rbSharedEna.CheckedChanged += (s, e) => { if (rbSharedEna.Checked) SetEnableStrat(EnablePinStrategy.SharedSinglePin); };
                rbIndivEna.CheckedChanged += (s, e) => { if (rbIndivEna.Checked) SetEnableStrat(EnablePinStrategy.IndividualPerAxis); };
                rbNoEna.CheckedChanged += (s, e) => { if (rbNoEna.Checked) SetEnableStrat(EnablePinStrategy.ExternalNoPin); };

                cardStrat.Controls.AddRange(new Control[] { rbSharedEna, rbIndivEna, rbNoEna, btnAutoOut });
                root.Controls.Add(cardStrat);

                // Motor table
                var activeAxes = _workingConfig.Axes.Where(a => a.Enabled).ToList();
                var cardTable = CreateCardPanel($"Motor Output Pin Mapping ({spec.DisplayName})", 85 + activeAxes.Count * 42);

                int y = 40;
                AddColHeader(cardTable, "Motor", 18, y, 120);
                AddColHeader(cardTable, "Step Output Pin", 150, y, 200);
                AddColHeader(cardTable, "Inv", 358, y, 40);
                AddColHeader(cardTable, "Dir Output Pin", 420, y, 200);
                AddColHeader(cardTable, "Inv", 628, y, 40);
                AddColHeader(cardTable, "Enable Output Pin", 690, y, 210);
                AddColHeader(cardTable, "Inv", 908, y, 40);
                y += 28;

                foreach (var ax in activeAxes)
                {
                    string lbl = ax.Role == AxisRole.GantrySlave ? $"{ax.AxisName} (Slave {ax.MasterAxis})" : $"{ax.AxisName} Axis";
                    cardTable.Controls.Add(new Label
                    {
                        Text = lbl,
                        Location = new Point(18, y + 6),
                        Size = new Size(125, 22),
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = ax.Role == AxisRole.GantrySlave ? _accentAmber : Color.White
                    });

                    var cbStep = CreatePinComboBox(spec, isInput: false, 150, y + 2, 200, ax.StepPort, ax.StepPin,
                        (pt, pn) => { ax.StepPort = pt; ax.StepPin = pn; RefreshResourceStatusBar(); });
                    var chkStepNeg = CreateCompactCheck(362, y + 6, ax.StepNegate, v => ax.StepNegate = v);

                    var cbDir = CreatePinComboBox(spec, isInput: false, 420, y + 2, 200, ax.DirPort, ax.DirPin,
                        (pt, pn) => { ax.DirPort = pt; ax.DirPin = pn; RefreshResourceStatusBar(); });
                    var chkDirNeg = CreateCompactCheck(632, y + 6, ax.DirNegate, v => ax.DirNegate = v);

                    bool enaEditable = _workingConfig.EnableStrategy == EnablePinStrategy.IndividualPerAxis ||
                        (_workingConfig.EnableStrategy == EnablePinStrategy.SharedSinglePin && ax == activeAxes[0]);

                    var cbEna = CreatePinComboBox(spec, isInput: false, 690, y + 2, 210, ax.EnaPort, ax.EnaPin,
                        (pt, pn) =>
                        {
                            if (_workingConfig.EnableStrategy == EnablePinStrategy.SharedSinglePin)
                            {
                                _workingConfig.SharedEnablePort = pt;
                                _workingConfig.SharedEnablePin = pn;
                                PinAllocator.SyncSharedEnablePin(_workingConfig);
                                RenderCurrentStep();
                            }
                            else
                            {
                                ax.EnaPort = pt;
                                ax.EnaPin = pn;
                                RefreshResourceStatusBar();
                            }
                        });
                    cbEna.Enabled = enaEditable;

                    var chkEnaNeg = CreateCompactCheck(912, y + 6, ax.EnaNegate, v =>
                    {
                        if (_workingConfig.EnableStrategy == EnablePinStrategy.SharedSinglePin)
                        {
                            _workingConfig.SharedEnableNegate = v;
                            PinAllocator.SyncSharedEnablePin(_workingConfig);
                            RenderCurrentStep();
                        }
                        else
                        {
                            ax.EnaNegate = v;
                        }
                    });
                    chkEnaNeg.Enabled = enaEditable;

                    cardTable.Controls.AddRange(new Control[] { cbStep, chkStepNeg, cbDir, chkDirNeg, cbEna, chkEnaNeg });
                    y += 40;
                }

                root.Controls.Add(cardTable);
            }
            finally
            {
                _isBinding = false;
            }

            return root;
        }

        // =========================================================================
        // STEP 4: HOMING, LIMIT SWITCH STRATEGY & SOFT LIMITS
        // =========================================================================
        private Panel BuildStep4HomeLimitsPanel()
        {
            var spec = CurrentSpec;
            string unit = _workingConfig.IsImperial ? "in" : "mm";
            var root = CreateStepContainer("Step 4: Homing, Limit Switch Wiring & Soft Limits",
                "Select how limit and home switches are wired to maximize accuracy and preserve Input pins for ATC sensors.");

            _isBinding = true;
            try
            {
                var cardStrat = CreateCardPanel("Limit & Home Wiring Strategy", 160);

                var rbHomeSoft = CreateRadioButton("1. Home Only + Soft Limits (1 Input/motor — Recommended, frees maximum pins for ATC)", 18, 36, 700,
                    _workingConfig.LimitStrategy == LimitHomeWiringStrategy.HomeOnlyWithSoftLimits);
                var rbSharedHL = CreateRadioButton("2. Shared Home + Limit Switch per Axis (1 Input/motor — dual-assigned in UCCNC)", 18, 62, 700,
                    _workingConfig.LimitStrategy == LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis);
                var rbSeriesLim = CreateRadioButton("3. Individual Home + 1 Series NC Limit Loop (1 Input/motor + 1 Common Limit pin)", 18, 88, 700,
                    _workingConfig.LimitStrategy == LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin);
                var rbSepAll = CreateRadioButton("4. Separate Home, Limit- and Limit+ per Axis (3 Inputs/motor — high pin consumption)", 18, 114, 700,
                    _workingConfig.LimitStrategy == LimitHomeWiringStrategy.SeparateHomeLimitMinusLimitPlus);

                var chkSlaveHome = new CheckBox
                {
                    Location = new Point(730, 38),
                    Size = new Size(275, 24),
                    ForeColor = Color.FromArgb(145, 225, 175),
                    Checked = _workingConfig.IndependentSlaveHome,
                    Text = "Slave Gantry Auto-Squaring Home"
                };

                var btnAutoIn = CreateButton("Smart Auto-Assign Inputs", _accentBlue, 220, 34);
                btnAutoIn.Location = new Point(770, 74);
                btnAutoIn.Click += (s, e) =>
                {
                    PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                    RenderCurrentStep();
                };

                void SetLimStrat(LimitHomeWiringStrategy st)
                {
                    if (_isBinding) return;
                    _workingConfig.LimitStrategy = st;
                    PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                    RenderCurrentStep();
                }
                rbHomeSoft.CheckedChanged += (s, e) => { if (rbHomeSoft.Checked) SetLimStrat(LimitHomeWiringStrategy.HomeOnlyWithSoftLimits); };
                rbSharedHL.CheckedChanged += (s, e) => { if (rbSharedHL.Checked) SetLimStrat(LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis); };
                rbSeriesLim.CheckedChanged += (s, e) => { if (rbSeriesLim.Checked) SetLimStrat(LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin); };
                rbSepAll.CheckedChanged += (s, e) => { if (rbSepAll.Checked) SetLimStrat(LimitHomeWiringStrategy.SeparateHomeLimitMinusLimitPlus); };
                chkSlaveHome.CheckedChanged += (s, e) =>
                {
                    if (_isBinding) return;
                    _workingConfig.IndependentSlaveHome = chkSlaveHome.Checked;
                    PinAllocator.AutoAssignInputs(_workingConfig, CurrentSpec);
                    RenderCurrentStep();
                };

                cardStrat.Controls.AddRange(new Control[] { rbHomeSoft, rbSharedHL, rbSeriesLim, rbSepAll, chkSlaveHome, btnAutoIn });
                root.Controls.Add(cardStrat);

                // Per-axis Home / Limit / Soft Limits table
                var activeAxes = _workingConfig.Axes.Where(a => a.Enabled).ToList();
                var cardAxes = CreateCardPanel($"Home / Limit Inputs & Soft Limits ({unit})", 82 + activeAxes.Count * 42);
                int y = 40;
                AddColHeader(cardAxes, "Axis", 16, y, 80);
                AddColHeader(cardAxes, "Home Input", 100, y, 165);
                AddColHeader(cardAxes, "Inv", 270, y, 36);
                AddColHeader(cardAxes, "Home Dir", 312, y, 75);
                AddColHeader(cardAxes, "Limit- Input", 395, y, 165);
                AddColHeader(cardAxes, "Limit+ Input", 570, y, 165);
                AddColHeader(cardAxes, $"Soft Min ({unit})", 745, y, 82);
                AddColHeader(cardAxes, $"Soft Max ({unit})", 835, y, 82);
                AddColHeader(cardAxes, "Backoff", 925, y, 70);
                y += 28;

                bool showSepLimits = _workingConfig.LimitStrategy == LimitHomeWiringStrategy.SeparateHomeLimitMinusLimitPlus;
                bool showSeriesLimits = _workingConfig.LimitStrategy == LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin;

                foreach (var ax in activeAxes)
                {
                    bool isSlave = ax.Role == AxisRole.GantrySlave;
                    string lbl = isSlave ? $"{ax.AxisName} (Sl)" : ax.AxisName;
                    cardAxes.Controls.Add(new Label
                    {
                        Text = lbl,
                        Location = new Point(16, y + 6),
                        Size = new Size(80, 22),
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = isSlave ? _accentAmber : Color.White
                    });

                    var cbHome = CreatePinComboBox(spec, isInput: true, 100, y + 2, 165, ax.HomePort, ax.HomePin,
                        (pt, pn) =>
                        {
                            ax.HomePort = pt; ax.HomePin = pn;
                            if (_workingConfig.LimitStrategy == LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis)
                            {
                                if (ax.HomeDirectionPositive) { ax.LimitPlusPort = pt; ax.LimitPlusPin = pn; }
                                else { ax.LimitMinusPort = pt; ax.LimitMinusPin = pn; }
                                RenderCurrentStep();
                            }
                            else RefreshResourceStatusBar();
                        });
                    cbHome.Enabled = !isSlave || _workingConfig.IndependentSlaveHome;

                    var chkHomeNeg = CreateCompactCheck(274, y + 6, ax.HomeNegate, v =>
                    {
                        ax.HomeNegate = v;
                        if (_workingConfig.LimitStrategy == LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis)
                        {
                            ax.LimitMinusNegate = v; ax.LimitPlusNegate = v;
                        }
                    });

                    var cmbDir = CreateCompactCombo(312, y + 2, 72, new[] { "- Neg", "+ Pos" });
                    cmbDir.SelectedIndex = ax.HomeDirectionPositive ? 1 : 0;
                    cmbDir.Enabled = !isSlave;
                    cmbDir.SelectedIndexChanged += (s, e) =>
                    {
                        if (_isBinding) return;
                        ax.HomeDirectionPositive = cmbDir.SelectedIndex == 1;
                        ax.RecalculateSoftLimitsFromTravel();
                        _workingConfig.SyncSlaveAxisParameters();
                        RenderCurrentStep();
                    };

                    var cbLimM = CreatePinComboBox(spec, isInput: true, 395, y + 2, 165, ax.LimitMinusPort, ax.LimitMinusPin,
                        (pt, pn) => { ax.LimitMinusPort = pt; ax.LimitMinusPin = pn; RefreshResourceStatusBar(); });
                    cbLimM.Enabled = !isSlave && (showSepLimits || showSeriesLimits);

                    var cbLimP = CreatePinComboBox(spec, isInput: true, 570, y + 2, 165, ax.LimitPlusPort, ax.LimitPlusPin,
                        (pt, pn) => { ax.LimitPlusPort = pt; ax.LimitPlusPin = pn; RefreshResourceStatusBar(); });
                    cbLimP.Enabled = !isSlave && showSepLimits;

                    var tbSoftMin = CreateNumericBox(745, y + 2, 78, ax.SoftLimitNegative.ToString("0.##", CultureInfo.InvariantCulture), !isSlave);
                    var tbSoftMax = CreateNumericBox(835, y + 2, 78, ax.SoftLimitPositive.ToString("0.##", CultureInfo.InvariantCulture), !isSlave);
                    var tbBackoff = CreateNumericBox(925, y + 2, 68, ax.HomeBackoff.ToString("0.##", CultureInfo.InvariantCulture), !isSlave);

                    BindDoubleInput(tbSoftMin, v => { ax.SoftLimitNegative = v; _workingConfig.SyncSlaveAxisParameters(); });
                    BindDoubleInput(tbSoftMax, v => { ax.SoftLimitPositive = v; _workingConfig.SyncSlaveAxisParameters(); });
                    BindDoubleInput(tbBackoff, v => { ax.HomeBackoff = Math.Max(0, v); _workingConfig.SyncSlaveAxisParameters(); });

                    cardAxes.Controls.AddRange(new Control[] { cbHome, chkHomeNeg, cmbDir, cbLimM, cbLimP, tbSoftMin, tbSoftMax, tbBackoff });
                    y += 40;
                }
                root.Controls.Add(cardAxes);

                // General Homing sequence & Safe Z
                var cardSeq = CreateCardPanel("Homing Sequence, Soft Limits Activation & Safe Z", 105);
                var chkSoft = new CheckBox
                {
                    Text = "Enable Soft Limits",
                    Location = new Point(18, 38),
                    Size = new Size(160, 24),
                    ForeColor = Color.White,
                    Checked = _workingConfig.EnableSoftLimits
                };
                chkSoft.CheckedChanged += (s, e) => _workingConfig.EnableSoftLimits = chkSoft.Checked;

                var chkHomeStart = new CheckBox
                {
                    Text = "Require Homing before Cycle Start",
                    Location = new Point(18, 66),
                    Size = new Size(255, 24),
                    ForeColor = Color.White,
                    Checked = _workingConfig.CheckHomeOnCycleStart
                };
                chkHomeStart.CheckedChanged += (s, e) => _workingConfig.CheckHomeOnCycleStart = chkHomeStart.Checked;

                AddFormLabel(cardSeq, $"Safe Z ({unit}):", 295, 40);
                var tbSafeZ = CreateNumericBox(410, 36, 80, _workingConfig.SafeZ.ToString("0.##", CultureInfo.InvariantCulture), true);
                BindDoubleInput(tbSafeZ, v => _workingConfig.SafeZ = v);

                AddFormLabel(cardSeq, "Home Order 1..4:", 525, 40);
                string[] axisChoices = { "None", "X", "Y", "Z", "A", "B", "C" };
                for (int sIdx = 0; sIdx < 4; sIdx++)
                {
                    int idx = sIdx;
                    var cbSeq = CreateCompactCombo(665 + sIdx * 82, 36, 74, axisChoices);
                    int match = Array.IndexOf(axisChoices, _workingConfig.HomeSequence[idx] ?? "None");
                    cbSeq.SelectedIndex = match >= 0 ? match : 0;
                    cbSeq.SelectedIndexChanged += (s, e) =>
                    {
                        if (_isBinding) return;
                        _workingConfig.HomeSequence[idx] = cbSeq.SelectedItem?.ToString() ?? "None";
                    };
                    cardSeq.Controls.Add(cbSeq);
                }

                cardSeq.Controls.AddRange(new Control[] { chkSoft, chkHomeStart, tbSafeZ });
                root.Controls.Add(cardSeq);
            }
            finally
            {
                _isBinding = false;
            }

            return root;
        }

        // =========================================================================
        // STEP 5: E-STOP, SPINDLE, PROBES & PINOUT AUDIT SUMMARY
        // =========================================================================
        private Panel BuildStep5IoAndSummaryPanel()
        {
            var spec = CurrentSpec;
            var root = CreateStepContainer("Step 5: E-Stop, Spindle, Probes & Stage 2 ATC Readiness Audit",
                "Assign E-Stop, Spindle outputs, and Tool Setter / Touch Probe inputs, then review conflicts and remaining ATC capacity before applying.");

            _isBinding = true;
            try
            {
                var cardIo = CreateCardPanel("Safety, Spindle & Probing I/O", 215);

                // Left column: Inputs (E-Stop, Probe 1, Probe 2)
                AddFormLabel(cardIo, "E-Stop Input:", 18, 40);
                var cbEstop = CreatePinComboBox(spec, true, 195, 36, 175, _workingConfig.EstopPort, _workingConfig.EstopPin,
                    (pt, pn) => { _workingConfig.EstopPort = pt; _workingConfig.EstopPin = pn; RenderCurrentStep(); });
                var chkEstopNeg = CreateCompactTextCheck(380, 40, "Active Low (NC)", _workingConfig.EstopNegate,
                    v => _workingConfig.EstopNegate = v);

                AddFormLabel(cardIo, "Probe 1 (Tool Setter):", 18, 76);
                var cbProbe1 = CreatePinComboBox(spec, true, 195, 72, 175, _workingConfig.Probe1Port, _workingConfig.Probe1Pin,
                    (pt, pn) => { _workingConfig.Probe1Port = pt; _workingConfig.Probe1Pin = pn; RenderCurrentStep(); });
                var chkP1Neg = CreateCompactTextCheck(380, 76, "Active Low", _workingConfig.Probe1Negate,
                    v => _workingConfig.Probe1Negate = v);

                var chkUseP2 = new CheckBox
                {
                    Text = "Probe 2 (3D Probe):",
                    Location = new Point(18, 112),
                    Size = new Size(170, 24),
                    ForeColor = Color.White,
                    Checked = _workingConfig.UseProbe2
                };
                var cbProbe2 = CreatePinComboBox(spec, true, 195, 108, 175, _workingConfig.Probe2Port, _workingConfig.Probe2Pin,
                    (pt, pn) => { _workingConfig.Probe2Port = pt; _workingConfig.Probe2Pin = pn; RenderCurrentStep(); });
                cbProbe2.Enabled = _workingConfig.UseProbe2;
                var chkP2Neg = CreateCompactTextCheck(380, 112, "Active Low", _workingConfig.Probe2Negate,
                    v => _workingConfig.Probe2Negate = v);
                chkP2Neg.Enabled = _workingConfig.UseProbe2;

                chkUseP2.CheckedChanged += (s, e) =>
                {
                    if (_isBinding) return;
                    _workingConfig.UseProbe2 = chkUseP2.Checked;
                    if (!chkUseP2.Checked) { _workingConfig.Probe2Port = 0; _workingConfig.Probe2Pin = 0; }
                    RenderCurrentStep();
                };

                // Right column: Spindle & Coolant Outputs
                AddFormLabel(cardIo, "Spindle Mode:", 530, 40);
                var cmbSpindle = CreateCompactCombo(665, 36, 190, new[] { "PWM + Relay (M3)", "Step/Dir + Relay", "Relay / Modbus Only" });
                cmbSpindle.SelectedIndex = (int)_workingConfig.SpindleMode;
                cmbSpindle.SelectedIndexChanged += (s, e) =>
                {
                    if (_isBinding) return;
                    _workingConfig.SpindleMode = (SpindleControlMode)cmbSpindle.SelectedIndex;
                    RenderCurrentStep();
                };

                AddFormLabel(cardIo, "Spindle PWM Out:", 530, 76);
                var cbPwm = CreatePinComboBox(spec, false, 665, 72, 175, _workingConfig.SpindlePwmPort, _workingConfig.SpindlePwmPin,
                    (pt, pn) => { _workingConfig.SpindlePwmPort = pt; _workingConfig.SpindlePwmPin = pn; RenderCurrentStep(); });
                cbPwm.Enabled = _workingConfig.SpindleMode != SpindleControlMode.RelayOrModbusOnly;
                var chkPwmNeg = CreateCompactTextCheck(850, 76, "Inv", _workingConfig.SpindlePwmNegate,
                    v => _workingConfig.SpindlePwmNegate = v);

                AddFormLabel(cardIo, "Spindle M3 Relay:", 530, 112);
                var cbM3 = CreatePinComboBox(spec, false, 665, 108, 175, _workingConfig.M3RelayPort, _workingConfig.M3RelayPin,
                    (pt, pn) => { _workingConfig.M3RelayPort = pt; _workingConfig.M3RelayPin = pn; RenderCurrentStep(); });
                var chkM3Neg = CreateCompactTextCheck(850, 112, "Inv", _workingConfig.M3RelayNegate,
                    v => _workingConfig.M3RelayNegate = v);

                var chkM7 = new CheckBox
                {
                    Text = "Mist (M7):",
                    Location = new Point(530, 148),
                    Size = new Size(120, 24),
                    ForeColor = Color.White,
                    Checked = _workingConfig.UseMistM7
                };
                var cbM7 = CreatePinComboBox(spec, false, 665, 144, 175, _workingConfig.M7MistPort, _workingConfig.M7MistPin,
                    (pt, pn) => { _workingConfig.M7MistPort = pt; _workingConfig.M7MistPin = pn; RenderCurrentStep(); });
                cbM7.Enabled = _workingConfig.UseMistM7;
                chkM7.CheckedChanged += (s, e) =>
                {
                    if (_isBinding) return;
                    _workingConfig.UseMistM7 = chkM7.Checked;
                    if (!chkM7.Checked) { _workingConfig.M7MistPort = 0; _workingConfig.M7MistPin = 0; }
                    RenderCurrentStep();
                };

                var chkM8 = new CheckBox
                {
                    Text = "Flood (M8):",
                    Location = new Point(530, 180),
                    Size = new Size(120, 24),
                    ForeColor = Color.White,
                    Checked = _workingConfig.UseFloodM8
                };
                var cbM8 = CreatePinComboBox(spec, false, 665, 176, 175, _workingConfig.M8FloodPort, _workingConfig.M8FloodPin,
                    (pt, pn) => { _workingConfig.M8FloodPort = pt; _workingConfig.M8FloodPin = pn; RenderCurrentStep(); });
                cbM8.Enabled = _workingConfig.UseFloodM8;
                chkM8.CheckedChanged += (s, e) =>
                {
                    if (_isBinding) return;
                    _workingConfig.UseFloodM8 = chkM8.Checked;
                    if (!chkM8.Checked) { _workingConfig.M8FloodPort = 0; _workingConfig.M8FloodPin = 0; }
                    RenderCurrentStep();
                };

                cardIo.Controls.AddRange(new Control[]
                {
                    cbEstop, chkEstopNeg, cbProbe1, chkP1Neg, chkUseP2, cbProbe2, chkP2Neg,
                    cmbSpindle, cbPwm, chkPwmNeg, cbM3, chkM3Neg, chkM7, cbM7, chkM8, cbM8
                });
                root.Controls.Add(cardIo);

                // Audit & Stage 2 Readiness Panel
                var report = PinAllocator.ValidateAndAudit(_workingConfig, spec);
                var cardAudit = CreateCardPanel("Pinout Audit & Stage 2 (ATC) Readiness", 230);

                var tbAudit = new TextBox
                {
                    Location = new Point(18, 38),
                    Size = new Size(990, 176),
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = Color.FromArgb(24, 27, 33),
                    ForeColor = report.Conflicts.Count > 0 || report.InvalidPins.Count > 0 ? _accentRed : Color.FromArgb(190, 240, 200),
                    Font = new Font("Consolas", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = _profileService.ExportPinoutSummary(_workingConfig)
                };

                cardAudit.Controls.Add(tbAudit);
                root.Controls.Add(cardAudit);
            }
            finally
            {
                _isBinding = false;
            }

            return root;
        }

        // =========================================================================
        // APPLY & SAVE HANDLER
        // =========================================================================
        private void BtnApplySave_Click(object sender, EventArgs e)
        {
            var spec = CurrentSpec;
            var report = PinAllocator.ValidateAndAudit(_workingConfig, spec);

            if (report.InvalidPins.Count > 0 || report.Conflicts.Count > 0)
            {
                string issues = string.Join("\n", report.InvalidPins.Concat(report.Conflicts).Take(8));
                var res = MessageBox.Show(
                    this,
                    $"Pin validation detected {report.InvalidPins.Count + report.Conflicts.Count} issue(s):\n\n{issues}\n\nDo you want to fix them before saving?",
                    "Pin Validation Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (res == DialogResult.Yes)
                {
                    NavigateToStep(4);
                    return;
                }
            }

            try
            {
                _profileService.ApplyAndSaveToUccnc(_workingConfig, _uccncMainForm);
                _lblStatusFooter.ForeColor = _accentGreen;
                _lblStatusFooter.Text = $"Stage 1 applied & saved to [{_profileService.ProfileName}]!";
                OnStage1Saved?.Invoke();

                MessageBox.Show(
                    this,
                    $"Stage 1 Machine Configuration applied and saved successfully!\n\n" +
                    $"Profile: {_profileService.ProfileName}\n" +
                    $"Controller: {spec.DisplayName}\n" +
                    $"Wiring Summary saved to:\n{_profileService.PinoutSummaryPath}\n\n" +
                    $"Note: Doontoolchange=2 (Run M6 Macro) and Soft Limits have been configured automatically.",
                    $"{PluginVersion.FullTitle} — Stage 1 Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("BtnApplySave_Click", ex);
                MessageBox.Show(this, $"Error applying Stage 1 settings: {ex.Message}", "Apply Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowExportSummaryModal()
        {
            string summary = _profileService.ExportPinoutSummary(_workingConfig);
            MessageBox.Show(
                this,
                $"Pinout & Wiring Summary exported to:\n{_profileService.PinoutSummaryPath}\n\n" +
                $"Preview:\n" + string.Join("\n", summary.Split('\n').Take(18)),
                "Wiring Summary Exported",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================================
        // UI FACTORY HELPERS
        // =========================================================================
        private Panel CreateStepContainer(string title, string subtitle)
        {
            var container = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Width = 1040,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };

            var lblH = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 2)
            };
            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9f),
                ForeColor = _textMuted,
                AutoSize = true,
                MaximumSize = new Size(1020, 40),
                Margin = new Padding(0, 0, 0, 10)
            };

            container.Controls.Add(lblH);
            container.Controls.Add(lblSub);
            return container;
        }

        private Panel CreateCardPanel(string title, int height)
        {
            var p = new Panel
            {
                Width = 1028,
                Height = height,
                BackColor = _bgCard,
                Margin = new Padding(0, 0, 0, 12)
            };
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(_borderCol, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };

            var lbl = new Label
            {
                Text = title,
                Location = new Point(14, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 195, 255)
            };
            p.Controls.Add(lbl);
            return p;
        }

        private void AddFormLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = _textMuted
            });
        }

        private void AddFormValue(Control parent, string text, int x, int y, Color col)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = col
            });
        }

        private void AddColHeader(Control parent, string text, int x, int y, int width)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 20),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 195, 255)
            });
        }

        private RadioButton CreateRadioButton(string text, int x, int y, int w, bool isChecked)
        {
            return new RadioButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 24),
                ForeColor = Color.White,
                Checked = isChecked
            };
        }

        private TextBox CreateNumericBox(int x, int y, int w, string initial, bool enabled)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                BackColor = enabled ? _bgInput : Color.FromArgb(35, 38, 46),
                ForeColor = enabled ? Color.White : _textMuted,
                BorderStyle = BorderStyle.FixedSingle,
                Text = initial,
                Enabled = enabled
            };
        }

        private ComboBox CreateCompactCombo(int x, int y, int w, string[] items)
        {
            var cb = new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _bgInput,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cb.Items.AddRange(items);
            return cb;
        }

        private ComboBox CreatePinComboBox(
            ControllerHardwareSpec spec,
            bool isInput,
            int x,
            int y,
            int w,
            int curPort,
            int curPin,
            Action<int, int> onChanged)
        {
            var cb = new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _bgInput,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var choices = new List<PinChoice> { new PinChoice(0, 0) };
            var pins = isInput ? spec.EnumerateAllInputs(false) : spec.EnumerateAllOutputs(false);
            foreach (var pp in pins)
            {
                choices.Add(new PinChoice(pp.Port, pp.Pin));
            }

            if ((curPort != 0 || curPin != 0) && !choices.Any(c => c.Port == curPort && c.Pin == curPin))
            {
                choices.Add(new PinChoice(curPort, curPin, $"Port {curPort} - Pin {curPin} [INVALID]"));
            }

            foreach (var c in choices) cb.Items.Add(c);

            int selIdx = choices.FindIndex(c => c.Port == curPort && c.Pin == curPin);
            cb.SelectedIndex = selIdx >= 0 ? selIdx : 0;

            cb.SelectedIndexChanged += (s, e) =>
            {
                if (_isBinding) return;
                if (cb.SelectedItem is PinChoice chosen)
                {
                    onChanged(chosen.Port, chosen.Pin);
                }
            };
            return cb;
        }

        private CheckBox CreateCompactCheck(int x, int y, bool initial, Action<bool> onChanged)
        {
            var chk = new CheckBox
            {
                Location = new Point(x, y),
                Size = new Size(20, 20),
                Checked = initial
            };
            chk.CheckedChanged += (s, e) =>
            {
                if (!_isBinding) onChanged(chk.Checked);
            };
            return chk;
        }

        private CheckBox CreateCompactTextCheck(int x, int y, string text, bool initial, Action<bool> onChanged)
        {
            var chk = new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = _textMuted,
                Checked = initial
            };
            chk.CheckedChanged += (s, e) =>
            {
                if (!_isBinding) onChanged(chk.Checked);
            };
            return chk;
        }

        private static void BindDoubleInput(TextBox tb, Action<double> setter)
        {
            tb.Leave += (s, e) =>
            {
                if (double.TryParse(tb.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                {
                    setter(val);
                }
            };
        }

        private Button CreateButton(string text, Color backCol, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                BackColor = backCol,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private string GetSlaveChannelForMaster(string masterAxis)
        {
            var sl = _workingConfig.Axes.FirstOrDefault(a => a.Role == AxisRole.GantrySlave && a.MasterAxis == masterAxis);
            return sl?.AxisName ?? "None";
        }

        private static MachineConfig CloneConfig(MachineConfig src)
        {
            var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
            return ser.Deserialize<MachineConfig>(ser.Serialize(src)) ?? new MachineConfig();
        }
    }
}
