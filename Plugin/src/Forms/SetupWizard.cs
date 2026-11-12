using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using Plugins.Core;
using Plugins.Persistence;

namespace Plugins.Forms
{
    public partial class SetupWizard : Form
    {
        private readonly ConfigStore _configStore;
        private readonly Plugininterface.Entry _uc;
        private ATCConfig _tempConfig;
        
        private int _currentStep = 0;
        private const int TotalSteps = 14;

        private readonly string[] StepTitles = new string[]
        {
            "Welcome",
            "Rack Orientation",
            "Slot 1 Position",
            "Slot Spacing",
            "Z Heights",
            "Drawbar Output",
            "Air Blow",
            "Pressure Sensor",
            "Slot Sensors",
            "Enable Slots",
            "Speeds & Timing",
            "Tool Change Position",
            "Options",
            "Summary"
        };

        public SetupWizard(ConfigStore configStore, Plugininterface.Entry uc)
        {
            _configStore = configStore;
            _uc = uc;
            _tempConfig = CloneConfig(_configStore.Config);
            
            InitializeComponent();
            SetupWizardUI();
            ShowStep(0);
        }

        private void SetupWizardUI()
        {
            this.Text = "HTM ATC Setup Wizard";
            UpdateStepIndicator();
        }

        private ATCConfig CloneConfig(ATCConfig source)
        {
            var clone = ATCConfig.CreateDefault();
            
            clone.Rack.Axis = source.Rack.Axis;
            clone.Rack.SlotCount = source.Rack.SlotCount;
            clone.Rack.Slot1Position = source.Rack.Slot1Position;
            clone.Rack.Slot1PositionSecondary = source.Rack.Slot1PositionSecondary;
            clone.Rack.Pitch = source.Rack.Pitch;
            
            clone.Heights.ZSafe = source.Heights.ZSafe;
            clone.Heights.ZEmpty = source.Heights.ZEmpty;
            clone.Heights.ZApproach = source.Heights.ZApproach;
            clone.Heights.ZClamp = source.Heights.ZClamp;
            clone.Heights.ZCheck = source.Heights.ZCheck;
            
            clone.FeedsAndTiming.FeedRate = source.FeedsAndTiming.FeedRate;
            clone.FeedsAndTiming.OperationDelay = source.FeedsAndTiming.OperationDelay;
            clone.FeedsAndTiming.SensorWaitTime = source.FeedsAndTiming.SensorWaitTime;
            
            clone.TCP.X = source.TCP.X;
            clone.TCP.Y = source.TCP.Y;
            clone.TCP.Z = source.TCP.Z;
            
            clone.IO.Drawbar.Port = source.IO.Drawbar.Port;
            clone.IO.Drawbar.Pin = source.IO.Drawbar.Pin;
            clone.IO.AirBlow.Enabled = source.IO.AirBlow.Enabled;
            clone.IO.AirBlow.Port = source.IO.AirBlow.Port;
            clone.IO.AirBlow.Pin = source.IO.AirBlow.Pin;
            clone.IO.PressureSensor.Enabled = source.IO.PressureSensor.Enabled;
            clone.IO.PressureSensor.Port = source.IO.PressureSensor.Port;
            clone.IO.PressureSensor.Pin = source.IO.PressureSensor.Pin;
            clone.IO.SlotSensors.Enabled = source.IO.SlotSensors.Enabled;
            clone.IO.SlotSensors.Port = source.IO.SlotSensors.Port;
            Array.Copy(source.IO.SlotSensors.Pins, clone.IO.SlotSensors.Pins, 10);
            
            for (int i = 0; i < 10; i++)
            {
                clone.Slots[i].Enabled = source.Slots[i].Enabled;
                clone.Slots[i].ToolNumber = source.Slots[i].ToolNumber;
            }
            
            clone.Options.ApplyG43 = source.Options.ApplyG43;
            clone.Options.ReturnToXY = source.Options.ReturnToXY;
            clone.Options.MeasureNonRH = source.Options.MeasureNonRH;
            clone.Options.ShowATCPage = source.Options.ShowATCPage;
            clone.Options.AirBlowDuringTC = source.Options.AirBlowDuringTC;
            
            return clone;
        }

        private void ShowStep(int step)
        {
            _currentStep = step;
            UpdateStepIndicator();
            
            panelWelcome.Visible = step == 0;
            panelRackOrientation.Visible = step == 1;
            panelSlot1Position.Visible = step == 2;
            panelSlotPitch.Visible = step == 3;
            panelZHeights.Visible = step == 4;
            panelDrawbar.Visible = step == 5;
            panelAirBlow.Visible = step == 6;
            panelPressureSensor.Visible = step == 7;
            panelSlotSensors.Visible = step == 8;
            panelEnableSlots.Visible = step == 9;
            panelFeedsAndTiming.Visible = step == 10;
            panelTCP.Visible = step == 11;
            panelOptions.Visible = step == 12;
            panelSummary.Visible = step == 13;
            
            btnBack.Enabled = step > 0;
            btnNext.Text = step < TotalSteps - 1 ? "Next >" : "Finish";
            
            if (step == 13)
            {
                UpdateSummary();
            }
            
            PopulateStepControls(step);
        }

        private void UpdateStepIndicator()
        {
            lblStepIndicator.Text = $"Step {_currentStep + 1} of {TotalSteps}: {StepTitles[_currentStep]}";
            progressBar.Value = (_currentStep * 100) / (TotalSteps - 1);
        }

        private void PopulateStepControls(int step)
        {
            switch (step)
            {
                case 1:
                    rbAxisX.Checked = _tempConfig.Rack.Axis == "X";
                    rbAxisY.Checked = _tempConfig.Rack.Axis == "Y";
                    break;
                    
                case 2:
                    txtSlot1X.Text = F(_tempConfig.Rack.Slot1Position);
                    txtSlot1Y.Text = F(_tempConfig.Rack.Slot1PositionSecondary);
                    break;
                    
                case 3:
                    txtPitch.Text = F(_tempConfig.Rack.Pitch);
                    break;
                    
                case 4:
                    txtZSafe.Text = F(_tempConfig.Heights.ZSafe);
                    txtZEmpty.Text = F(_tempConfig.Heights.ZEmpty);
                    txtZApproach.Text = F(_tempConfig.Heights.ZApproach);
                    txtZClamp.Text = F(_tempConfig.Heights.ZClamp);
                    txtZCheck.Text = F(_tempConfig.Heights.ZCheck);
                    chkZCheckSameAsClamp.Checked = Math.Abs(_tempConfig.Heights.ZCheck - _tempConfig.Heights.ZClamp) < 0.001;
                    break;
                    
                case 5:
                    nudDrawbarPort.Value = _tempConfig.IO.Drawbar.Port;
                    nudDrawbarPin.Value = _tempConfig.IO.Drawbar.Pin;
                    break;
                    
                case 6:
                    chkAirBlowEnabled.Checked = _tempConfig.IO.AirBlow.Enabled;
                    nudAirBlowPort.Value = _tempConfig.IO.AirBlow.Port;
                    nudAirBlowPin.Value = _tempConfig.IO.AirBlow.Pin;
                    break;
                    
                case 7:
                    chkPressureEnabled.Checked = _tempConfig.IO.PressureSensor.Enabled;
                    nudPressurePort.Value = _tempConfig.IO.PressureSensor.Port;
                    nudPressurePin.Value = _tempConfig.IO.PressureSensor.Pin;
                    break;
                    
                case 8:
                    chkSlotSensorsEnabled.Checked = _tempConfig.IO.SlotSensors.Enabled;
                    nudSlotSensorPort.Value = _tempConfig.IO.SlotSensors.Port;
                    for (int i = 0; i < 10; i++)
                    {
                        nudSlotPins[i].Value = _tempConfig.IO.SlotSensors.Pins[i];
                    }
                    break;
                    
                case 9:
                    for (int i = 0; i < 10; i++)
                    {
                        chkSlotEnabled[i].Checked = _tempConfig.Slots[i].Enabled;
                    }
                    break;
                    
                case 10:
                    txtFeedRate.Text = F(_tempConfig.FeedsAndTiming.FeedRate);
                    txtOperationDelay.Text = _tempConfig.FeedsAndTiming.OperationDelay.ToString();
                    txtSensorWaitTime.Text = _tempConfig.FeedsAndTiming.SensorWaitTime.ToString();
                    break;
                    
                case 11:
                    txtTcpX.Text = F(_tempConfig.TCP.X);
                    txtTcpY.Text = F(_tempConfig.TCP.Y);
                    txtTcpZ.Text = F(_tempConfig.TCP.Z);
                    break;
                    
                case 12:
                    chkApplyG43.Checked = _tempConfig.Options.ApplyG43;
                    chkReturnToXY.Checked = _tempConfig.Options.ReturnToXY;
                    chkMeasureNonRH.Checked = _tempConfig.Options.MeasureNonRH;
                    chkShowATCPage.Checked = _tempConfig.Options.ShowATCPage;
                    chkAirBlowDuringTC.Checked = _tempConfig.Options.AirBlowDuringTC;
                    break;
            }
        }

        private bool ValidateStep(int step)
        {
            switch (step)
            {
                case 2:
                    if (!TryParseDouble(txtSlot1X.Text, out _))
                    {
                        ShowValidationError("Invalid Slot 1 X position");
                        return false;
                    }
                    if (!TryParseDouble(txtSlot1Y.Text, out _))
                    {
                        ShowValidationError("Invalid Slot 1 Y position");
                        return false;
                    }
                    break;
                    
                case 3:
                    if (!TryParseDouble(txtPitch.Text, out double pitch) || pitch <= 0)
                    {
                        ShowValidationError("Pitch must be a positive number");
                        return false;
                    }
                    break;
                    
                case 4:
                    if (!TryParseDouble(txtZSafe.Text, out _) ||
                        !TryParseDouble(txtZEmpty.Text, out _) ||
                        !TryParseDouble(txtZApproach.Text, out _) ||
                        !TryParseDouble(txtZClamp.Text, out _))
                    {
                        ShowValidationError("Invalid Z height value");
                        return false;
                    }
                    break;
                    
                case 9:
                    bool anyEnabled = false;
                    for (int i = 0; i < 10; i++)
                    {
                        if (chkSlotEnabled[i].Checked)
                        {
                            anyEnabled = true;
                            break;
                        }
                    }
                    if (!anyEnabled)
                    {
                        ShowValidationError("At least one slot must be enabled");
                        return false;
                    }
                    break;
                    
                case 10:
                    if (!TryParseDouble(txtFeedRate.Text, out double feedRate) || feedRate <= 0)
                    {
                        ShowValidationError("Feed rate must be a positive number");
                        return false;
                    }
                    if (!int.TryParse(txtOperationDelay.Text, out int delay) || delay < 0)
                    {
                        ShowValidationError("Operation delay must be a non-negative integer");
                        return false;
                    }
                    if (!int.TryParse(txtSensorWaitTime.Text, out int waitTime) || waitTime < 0)
                    {
                        ShowValidationError("Sensor wait time must be a non-negative integer");
                        return false;
                    }
                    break;
            }
            
            return true;
        }

        private void SaveStepToConfig(int step)
        {
            switch (step)
            {
                case 1:
                    _tempConfig.Rack.Axis = rbAxisX.Checked ? "X" : "Y";
                    break;
                    
                case 2:
                    TryParseDouble(txtSlot1X.Text, out double s1x);
                    TryParseDouble(txtSlot1Y.Text, out double s1y);
                    _tempConfig.Rack.Slot1Position = s1x;
                    _tempConfig.Rack.Slot1PositionSecondary = s1y;
                    break;
                    
                case 3:
                    TryParseDouble(txtPitch.Text, out double pitch);
                    _tempConfig.Rack.Pitch = pitch;
                    break;
                    
                case 4:
                    TryParseDouble(txtZSafe.Text, out double zSafe);
                    TryParseDouble(txtZEmpty.Text, out double zEmpty);
                    TryParseDouble(txtZApproach.Text, out double zApproach);
                    TryParseDouble(txtZClamp.Text, out double zClamp);
                    TryParseDouble(txtZCheck.Text, out double zCheck);
                    _tempConfig.Heights.ZSafe = zSafe;
                    _tempConfig.Heights.ZEmpty = zEmpty;
                    _tempConfig.Heights.ZApproach = zApproach;
                    _tempConfig.Heights.ZClamp = zClamp;
                    _tempConfig.Heights.ZCheck = chkZCheckSameAsClamp.Checked ? zClamp : zCheck;
                    break;
                    
                case 5:
                    _tempConfig.IO.Drawbar.Port = (int)nudDrawbarPort.Value;
                    _tempConfig.IO.Drawbar.Pin = (int)nudDrawbarPin.Value;
                    break;
                    
                case 6:
                    _tempConfig.IO.AirBlow.Enabled = chkAirBlowEnabled.Checked;
                    _tempConfig.IO.AirBlow.Port = (int)nudAirBlowPort.Value;
                    _tempConfig.IO.AirBlow.Pin = (int)nudAirBlowPin.Value;
                    break;
                    
                case 7:
                    _tempConfig.IO.PressureSensor.Enabled = chkPressureEnabled.Checked;
                    _tempConfig.IO.PressureSensor.Port = (int)nudPressurePort.Value;
                    _tempConfig.IO.PressureSensor.Pin = (int)nudPressurePin.Value;
                    break;
                    
                case 8:
                    _tempConfig.IO.SlotSensors.Enabled = chkSlotSensorsEnabled.Checked;
                    _tempConfig.IO.SlotSensors.Port = (int)nudSlotSensorPort.Value;
                    for (int i = 0; i < 10; i++)
                    {
                        _tempConfig.IO.SlotSensors.Pins[i] = (int)nudSlotPins[i].Value;
                    }
                    break;
                    
                case 9:
                    for (int i = 0; i < 10; i++)
                    {
                        _tempConfig.Slots[i].Enabled = chkSlotEnabled[i].Checked;
                    }
                    break;
                    
                case 10:
                    TryParseDouble(txtFeedRate.Text, out double feedRate);
                    int.TryParse(txtOperationDelay.Text, out int delay);
                    int.TryParse(txtSensorWaitTime.Text, out int waitTime);
                    _tempConfig.FeedsAndTiming.FeedRate = feedRate;
                    _tempConfig.FeedsAndTiming.OperationDelay = delay;
                    _tempConfig.FeedsAndTiming.SensorWaitTime = waitTime;
                    break;
                    
                case 11:
                    TryParseDouble(txtTcpX.Text, out double tcpX);
                    TryParseDouble(txtTcpY.Text, out double tcpY);
                    TryParseDouble(txtTcpZ.Text, out double tcpZ);
                    _tempConfig.TCP.X = tcpX;
                    _tempConfig.TCP.Y = tcpY;
                    _tempConfig.TCP.Z = tcpZ;
                    break;
                    
                case 12:
                    _tempConfig.Options.ApplyG43 = chkApplyG43.Checked;
                    _tempConfig.Options.ReturnToXY = chkReturnToXY.Checked;
                    _tempConfig.Options.MeasureNonRH = chkMeasureNonRH.Checked;
                    _tempConfig.Options.ShowATCPage = chkShowATCPage.Checked;
                    _tempConfig.Options.AirBlowDuringTC = chkAirBlowDuringTC.Checked;
                    break;
            }
        }

        private void UpdateSummary()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== RACK CONFIGURATION ===");
            sb.AppendLine($"Rack Axis: {_tempConfig.Rack.Axis}");
            sb.AppendLine($"Slot 1 Position: {_tempConfig.Rack.Axis}={F(_tempConfig.Rack.Slot1Position)}, {_tempConfig.Rack.AxisSecondary}={F(_tempConfig.Rack.Slot1PositionSecondary)}");
            sb.AppendLine($"Slot Pitch: {F(_tempConfig.Rack.Pitch)} mm");
            sb.AppendLine();
            
            sb.AppendLine("=== Z HEIGHTS ===");
            sb.AppendLine($"Z Safe: {F(_tempConfig.Heights.ZSafe)}");
            sb.AppendLine($"Z Empty: {F(_tempConfig.Heights.ZEmpty)}");
            sb.AppendLine($"Z Approach: {F(_tempConfig.Heights.ZApproach)}");
            sb.AppendLine($"Z Clamp: {F(_tempConfig.Heights.ZClamp)}");
            sb.AppendLine($"Z Check: {F(_tempConfig.Heights.ZCheck)}");
            sb.AppendLine();
            
            sb.AppendLine("=== I/O CONFIGURATION ===");
            sb.AppendLine($"Drawbar: Port {_tempConfig.IO.Drawbar.Port}, Pin {_tempConfig.IO.Drawbar.Pin}");
            sb.AppendLine($"Air Blow: {(_tempConfig.IO.AirBlow.Enabled ? $"Port {_tempConfig.IO.AirBlow.Port}, Pin {_tempConfig.IO.AirBlow.Pin}" : "Disabled")}");
            sb.AppendLine($"Pressure Sensor: {(_tempConfig.IO.PressureSensor.Enabled ? $"Port {_tempConfig.IO.PressureSensor.Port}, Pin {_tempConfig.IO.PressureSensor.Pin}" : "Disabled")}");
            sb.AppendLine($"Slot Sensors: {(_tempConfig.IO.SlotSensors.Enabled ? $"Port {_tempConfig.IO.SlotSensors.Port}" : "Disabled")}");
            sb.AppendLine();
            
            sb.AppendLine("=== ENABLED SLOTS ===");
            for (int i = 0; i < 10; i++)
            {
                if (_tempConfig.Slots[i].Enabled)
                {
                    sb.Append($"Slot {i + 1}  ");
                }
            }
            sb.AppendLine();
            sb.AppendLine();
            
            sb.AppendLine("=== FEEDS & TIMING ===");
            sb.AppendLine($"Feed Rate: {F(_tempConfig.FeedsAndTiming.FeedRate)} mm/min");
            sb.AppendLine($"Operation Delay: {_tempConfig.FeedsAndTiming.OperationDelay} ms");
            sb.AppendLine($"Sensor Wait Time: {_tempConfig.FeedsAndTiming.SensorWaitTime} ms");
            sb.AppendLine();
            
            sb.AppendLine("=== TOOL CHANGE POSITION ===");
            sb.AppendLine($"TCP: X={F(_tempConfig.TCP.X)}, Y={F(_tempConfig.TCP.Y)}, Z={F(_tempConfig.TCP.Z)}");
            sb.AppendLine();
            
            sb.AppendLine("=== OPTIONS ===");
            sb.AppendLine($"Apply G43: {(_tempConfig.Options.ApplyG43 ? "Yes" : "No")}");
            sb.AppendLine($"Return to XY: {(_tempConfig.Options.ReturnToXY ? "Yes" : "No")}");
            sb.AppendLine($"Measure non-RH tools: {(_tempConfig.Options.MeasureNonRH ? "Yes" : "No")}");
            sb.AppendLine($"Show ATC page: {(_tempConfig.Options.ShowATCPage ? "Yes" : "No")}");
            sb.AppendLine($"Air blow during TC: {(_tempConfig.Options.AirBlowDuringTC ? "Yes" : "No")}");
            
            txtSummary.Text = sb.ToString();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!ValidateStep(_currentStep))
                return;
            
            SaveStepToConfig(_currentStep);
            
            if (_currentStep < TotalSteps - 1)
            {
                ShowStep(_currentStep + 1);
            }
            else
            {
                SaveConfiguration();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            SaveStepToConfig(_currentStep);
            
            if (_currentStep > 0)
            {
                ShowStep(_currentStep - 1);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this, "Discard all changes?", "Cancel Setup",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void SaveConfiguration()
        {
            _configStore.UpdateConfig(c =>
            {
                c.Rack.Axis = _tempConfig.Rack.Axis;
                c.Rack.Slot1Position = _tempConfig.Rack.Slot1Position;
                c.Rack.Slot1PositionSecondary = _tempConfig.Rack.Slot1PositionSecondary;
                c.Rack.Pitch = _tempConfig.Rack.Pitch;
                
                c.Heights.ZSafe = _tempConfig.Heights.ZSafe;
                c.Heights.ZEmpty = _tempConfig.Heights.ZEmpty;
                c.Heights.ZApproach = _tempConfig.Heights.ZApproach;
                c.Heights.ZClamp = _tempConfig.Heights.ZClamp;
                c.Heights.ZCheck = _tempConfig.Heights.ZCheck;
                
                c.FeedsAndTiming.FeedRate = _tempConfig.FeedsAndTiming.FeedRate;
                c.FeedsAndTiming.OperationDelay = _tempConfig.FeedsAndTiming.OperationDelay;
                c.FeedsAndTiming.SensorWaitTime = _tempConfig.FeedsAndTiming.SensorWaitTime;
                
                c.TCP.X = _tempConfig.TCP.X;
                c.TCP.Y = _tempConfig.TCP.Y;
                c.TCP.Z = _tempConfig.TCP.Z;
                
                c.IO.Drawbar.Port = _tempConfig.IO.Drawbar.Port;
                c.IO.Drawbar.Pin = _tempConfig.IO.Drawbar.Pin;
                c.IO.AirBlow.Enabled = _tempConfig.IO.AirBlow.Enabled;
                c.IO.AirBlow.Port = _tempConfig.IO.AirBlow.Port;
                c.IO.AirBlow.Pin = _tempConfig.IO.AirBlow.Pin;
                c.IO.PressureSensor.Enabled = _tempConfig.IO.PressureSensor.Enabled;
                c.IO.PressureSensor.Port = _tempConfig.IO.PressureSensor.Port;
                c.IO.PressureSensor.Pin = _tempConfig.IO.PressureSensor.Pin;
                c.IO.SlotSensors.Enabled = _tempConfig.IO.SlotSensors.Enabled;
                c.IO.SlotSensors.Port = _tempConfig.IO.SlotSensors.Port;
                Array.Copy(_tempConfig.IO.SlotSensors.Pins, c.IO.SlotSensors.Pins, 10);
                
                for (int i = 0; i < 10; i++)
                {
                    c.Slots[i].Enabled = _tempConfig.Slots[i].Enabled;
                }
                
                c.Options.ApplyG43 = _tempConfig.Options.ApplyG43;
                c.Options.ReturnToXY = _tempConfig.Options.ReturnToXY;
                c.Options.MeasureNonRH = _tempConfig.Options.MeasureNonRH;
                c.Options.ShowATCPage = _tempConfig.Options.ShowATCPage;
                c.Options.AirBlowDuringTC = _tempConfig.Options.AirBlowDuringTC;
            });
        }

        private void btnImportProfile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "UCCNC Profile (*.pro)|*.pro|All Files (*.*)|*.*";
                ofd.Title = "Select HTM_ATC Profile";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _tempConfig = HTMProfileImporter.ImportFromProfile(ofd.FileName);
                        PopulateStepControls(_currentStep);
                        MessageBox.Show(this, "Profile imported successfully!", "Import Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Error importing profile: {ex.Message}", "Import Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnReadPosition_Click(object sender, EventArgs e)
        {
            try
            {
                double x = _uc.GetXmachpos();
                double y = _uc.GetYmachpos();
                
                if (_currentStep == 2)
                {
                    if (_tempConfig.Rack.Axis == "X")
                    {
                        txtSlot1X.Text = F(x);
                        txtSlot1Y.Text = F(y);
                    }
                    else
                    {
                        txtSlot1X.Text = F(y);
                        txtSlot1Y.Text = F(x);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error reading position: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReadZ_Click(object sender, EventArgs e)
        {
            try
            {
                double z = _uc.GetZmachpos();
                var btn = sender as Button;
                
                if (btn == btnReadZSafe) txtZSafe.Text = F(z);
                else if (btn == btnReadZEmpty) txtZEmpty.Text = F(z);
                else if (btn == btnReadZApproach)
                {
                    double y = _uc.GetYmachpos();
                    txtZApproach.Text = F(_tempConfig.Rack.Axis == "X" ? y : _uc.GetXmachpos());
                }
                else if (btn == btnReadZClamp) txtZClamp.Text = F(z);
                else if (btn == btnReadZCheck) txtZCheck.Text = F(z);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error reading position: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReadTCP_Click(object sender, EventArgs e)
        {
            try
            {
                txtTcpX.Text = F(_uc.GetXmachpos());
                txtTcpY.Text = F(_uc.GetYmachpos());
                txtTcpZ.Text = F(_uc.GetZmachpos());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error reading position: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestDrawbar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this,
                "This will toggle the drawbar output.\n\nEnsure no tool is in spindle or hold the tool firmly!",
                "Test Drawbar", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            
            if (result != DialogResult.OK) return;

            try
            {
                int port = (int)nudDrawbarPort.Value;
                int pin = (int)nudDrawbarPin.Value;
                int ledNum = GetLEDForPin(port, pin);
                bool currentState = _uc.GetLED(ledNum);
                
                if (currentState)
                {
                    _uc.Clroutpin(port, pin);
                }
                else
                {
                    _uc.Setoutpin(port, pin);
                    
                    var timer = new Timer { Interval = 3000 };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        try { _uc.Clroutpin(port, pin); }
                        catch { }
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error testing drawbar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetLEDForPin(int port, int pin)
        {
            int baseNum = 0;
            switch (port)
            {
                case 1: baseNum = 0; break;
                case 2: baseNum = 68; break;
                case 3: baseNum = 85; break;
                case 4: baseNum = 102; break;
                case 5: baseNum = 119; break;
            }
            return baseNum + pin;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(this, message, "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string F(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }

        private static bool TryParseDouble(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                   double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
