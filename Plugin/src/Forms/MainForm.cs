using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using Plugins.Core;
using Plugins.Persistence;

namespace Plugins.Forms
{
    public partial class MainForm : Form
    {
        private readonly Plugininterface.Entry _uc;
        private readonly ConfigStore _configStore;
        private readonly ATCStateMachine _stateMachine;
        private readonly SafetyMonitor _safetyMonitor;
        
        private volatile bool _forceClose;
        private volatile bool _isClosing;
        private volatile bool _updatePending;
        private readonly object _updateLock = new object();
        private System.Windows.Forms.Timer _uiTimer;
        private BackgroundWorker _testWorker;

        public MainForm(UCCNCplugin plugin, Plugininterface.Entry uc, ConfigStore configStore, ATCStateMachine stateMachine)
        {
            _uc = uc;
            _configStore = configStore;
            _stateMachine = stateMachine;
            
            _safetyMonitor = new SafetyMonitor(
                _configStore.Config, 
                _uc, 
                LogMessage, 
                () => _configStore.Config.Simulation.Enabled);

            InitializeComponent();
            SetupUI();
            SetupUITimer();
            SetupBackgroundWorker();
            
            ReconcileToolTableWithConfig();
        }
        
        private void ReconcileToolTableWithConfig()
        {
            try
            {
                _stateMachine.ToolTable.ReconcileWithConfig(_configStore.Config);
                _configStore.Save();
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"Error reconciling tooltable: {ex.Message}");
            }
        }

        private void SetupUI()
        {
            this.Text = "HTM ATC - Automatic Tool Changer";
            UpdateUIFromConfig();
        }

        private void SetupUITimer()
        {
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 100;
            _uiTimer.Tick += UITimer_Tick;
            _uiTimer.Start();
        }

        private void SetupBackgroundWorker()
        {
            _testWorker = new BackgroundWorker();
            _testWorker.WorkerSupportsCancellation = true;
            _testWorker.DoWork += TestWorker_DoWork;
            _testWorker.RunWorkerCompleted += TestWorker_Completed;
        }

        private void UITimer_Tick(object sender, EventArgs e)
        {
            if (_isClosing) return;

            lock (_updateLock)
            {
                if (!_updatePending) return;
                _updatePending = false;
            }

            try
            {
                bool simOrDemo = _configStore.Config.Simulation.Enabled || IsDemoMode();
                if (!simOrDemo)
                {
                    _safetyMonitor.CheckSafety();
                }
                UpdateStatusDisplay();
                UpdateSlotDisplay();
                UpdateDiagnosticsDisplay();
            }
            catch (Exception ex)
            {
                PluginLogger.LogLoopException(ex);
            }
        }

        private bool IsDemoMode()
        {
            try
            {
                return _uc.GetLED(218);
            }
            catch
            {
                return true;
            }
        }

        public void RequestUIUpdate()
        {
            if (_isClosing) return;

            lock (_updateLock)
            {
                _updatePending = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PluginLogger.Log("MainForm_Load");
            
            if (!_configStore.ConfigExists)
            {
                PluginLogger.Log("No config exists - will show wizard");
                this.BeginInvoke(new Action(() =>
                {
                    ShowSetupWizard();
                }));
            }
        }

        private void UpdateStatusDisplay()
        {
            int currentTool = 0;
            try { currentTool = _uc.Getcurrenttool(); }
            catch { }

            lblCurrentTool.Text = currentTool > 0 ? $"T{currentTool}" : "---";
            lblCurrentTool.ForeColor = currentTool > 0 ? Color.Green : Color.Gray;

            bool isHomed = _safetyMonitor.AreAllAxesHomed();
            ledHomed.BackColor = isHomed ? Color.Lime : Color.Red;

            bool hasAnyFault = _safetyMonitor.HasActiveFaults();
            ledSafety.BackColor = hasAnyFault ? Color.Red : Color.Lime;
            
            var snapshot = _stateMachine.GetSnapshot();
            if (snapshot.IsRunning)
            {
                lblCurrentTool.Text = $"TC: {snapshot.State}";
                lblCurrentTool.ForeColor = Color.Orange;
            }
        }

        private void UpdateSlotDisplay()
        {
            var config = _configStore.Config;
            int currentTool = 0;
            try { currentTool = _uc.Getcurrenttool(); }
            catch { }
            
            for (int i = 0; i < 10; i++)
            {
                var slot = config.Slots[i];
                var panel = slotPanels[i];
                
                if (panel == null) continue;

                var lblTool = panel.Controls["lblTool"] as Label;
                var lblToolInfo = panel.Controls["lblToolInfo"] as Label;
                var ledSensor = panel.Controls["ledSensor"] as Panel;
                var ledEnabled = panel.Controls["ledEnabled"] as Panel;
                var ledActive = panel.Controls["ledActive"] as Panel;
                
                if (lblTool != null)
                {
                    lblTool.Text = slot.ToolNumber > 0 ? $"T{slot.ToolNumber}" : "-";
                }
                
                if (lblToolInfo != null && slot.ToolNumber > 0)
                {
                    var toolInfo = _stateMachine.ToolTable.ReadToolInfo(slot.ToolNumber);
                    if (toolInfo != null)
                    {
                        lblToolInfo.Text = $"D{toolInfo.Diameter:F1} L{toolInfo.Length:F1} {toolInfo.Type}";
                    }
                    else
                    {
                        lblToolInfo.Text = "";
                    }
                }
                else if (lblToolInfo != null)
                {
                    lblToolInfo.Text = "";
                }
                
                if (ledEnabled != null)
                {
                    ledEnabled.BackColor = slot.Enabled ? Color.Lime : Color.Gray;
                }
                
                if (ledActive != null)
                {
                    bool isActive = slot.ToolNumber > 0 && slot.ToolNumber == currentTool;
                    ledActive.BackColor = isActive ? Color.Orange : Color.DarkGray;
                }
                
                if (ledSensor != null && config.IO.SlotSensors.Enabled)
                {
                    bool sensorState;
                    if (config.Simulation.Enabled)
                    {
                        sensorState = config.Simulation.SlotStates[i];
                    }
                    else
                    {
                        int ledNum = GetLEDForPin(config.IO.SlotSensors.Port, config.IO.SlotSensors.GetPinForSlot(i + 1));
                        try { sensorState = !_uc.GetLED(ledNum); }
                        catch { sensorState = false; }
                    }
                    ledSensor.BackColor = sensorState ? Color.Yellow : Color.DarkGray;
                }
            }
        }

        private void UpdateDiagnosticsDisplay()
        {
            var config = _configStore.Config;

            try
            {
                lblMachX.Text = $"X: {_uc.GetXmachpos().ToString("F3", CultureInfo.InvariantCulture)}";
                lblMachY.Text = $"Y: {_uc.GetYmachpos().ToString("F3", CultureInfo.InvariantCulture)}";
                lblMachZ.Text = $"Z: {_uc.GetZmachpos().ToString("F3", CultureInfo.InvariantCulture)}";
            }
            catch
            {
                lblMachX.Text = "X: ---";
                lblMachY.Text = "Y: ---";
                lblMachZ.Text = "Z: ---";
            }

            int drawbarLed = GetLEDForPin(config.IO.Drawbar.Port, config.IO.Drawbar.Pin);
            try { ledDrawbar.BackColor = _uc.GetLED(drawbarLed) ? Color.Orange : Color.DarkGray; }
            catch { ledDrawbar.BackColor = Color.DarkGray; }

            if (config.IO.AirBlow.Enabled)
            {
                int airLed = GetLEDForPin(config.IO.AirBlow.Port, config.IO.AirBlow.Pin);
                try { ledAirBlow.BackColor = _uc.GetLED(airLed) ? Color.Cyan : Color.DarkGray; }
                catch { ledAirBlow.BackColor = Color.DarkGray; }
            }

            if (config.IO.PressureSensor.Enabled)
            {
                int pressureLed = GetLEDForPin(config.IO.PressureSensor.Port, config.IO.PressureSensor.Pin);
                try
                {
                    bool lowPressure = _uc.GetLED(pressureLed);
                    ledPressure.BackColor = lowPressure ? Color.Red : Color.Lime;
                    lblPressure.Text = lowPressure ? "LOW" : "OK";
                }
                catch
                {
                    ledPressure.BackColor = Color.Gray;
                    lblPressure.Text = "?";
                }
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => LogMessage(message)));
                    return;
                }

                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                txtLog.AppendText($"[{timestamp}] {message}\r\n");
                txtLog.ScrollToCaret();
                
                PluginLogger.Log($"[UI] {message}");
            }
            catch { }
        }

        private void ShowSetupWizard()
        {
            PluginLogger.Log("ShowSetupWizard starting");
            using (var wizard = new SetupWizard(_configStore, _uc))
            {
                if (wizard.ShowDialog(this) == DialogResult.OK)
                {
                    PluginLogger.LogWizardFinish();
                    UpdateUIFromConfig();
                    LogMessage("Configuration saved from setup wizard");
                }
                else
                {
                    PluginLogger.Log("Setup Wizard cancelled");
                }
            }
            PluginLogger.Log("ShowSetupWizard completed");
        }

        private void UpdateUIFromConfig()
        {
            var config = _configStore.Config;
            
            chkSimulation.Checked = config.Simulation.Enabled;
            
            for (int i = 0; i < 10; i++)
            {
                if (i < chkSimSlots.Length)
                {
                    chkSimSlots[i].Checked = config.Simulation.SlotStates[i];
                }
            }

            chkSimPressureOK.Checked = config.Simulation.PressureOK;
            chkSimOssdOK.Checked = config.Simulation.OssdOK;
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

        private void btnSetupWizard_Click(object sender, EventArgs e)
        {
            ShowSetupWizard();
        }

        private void btnGoToTCP_Click(object sender, EventArgs e)
        {
            var config = _configStore.Config;
            
            if (!_safetyMonitor.AreAllAxesHomed() && !IsDemoMode())
            {
                MessageBox.Show(this, "Machine must be homed before moving.", "Not Homed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                LogMessage($"Moving to TCP: X{config.TCP.X} Y{config.TCP.Y} Z{config.TCP.Z}");
                _uc.Code($"G53 G00 Z{config.Heights.ZSafe.ToString("F3", CultureInfo.InvariantCulture)}");
                _uc.Code($"G53 G00 X{config.TCP.X.ToString("F3", CultureInfo.InvariantCulture)} Y{config.TCP.Y.ToString("F3", CultureInfo.InvariantCulture)}");
                _uc.Code($"G53 G00 Z{config.TCP.Z.ToString("F3", CultureInfo.InvariantCulture)}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error moving to TCP: {ex.Message}");
            }
        }

        private void btnTestDrawbar_Click(object sender, EventArgs e)
        {
            var config = _configStore.Config;
            
            var result = MessageBox.Show(this, 
                "This will toggle the drawbar output.\n\nEnsure no tool is in spindle or hold the tool firmly!",
                "Test Drawbar", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            
            if (result != DialogResult.OK) return;

            try
            {
                int ledNum = GetLEDForPin(config.IO.Drawbar.Port, config.IO.Drawbar.Pin);
                bool currentState = _uc.GetLED(ledNum);
                
                if (currentState)
                {
                    LogMessage("Closing drawbar");
                    _uc.Clroutpin(config.IO.Drawbar.Port, config.IO.Drawbar.Pin);
                }
                else
                {
                    LogMessage("Opening drawbar (will auto-close in 3 seconds)");
                    _uc.Setoutpin(config.IO.Drawbar.Port, config.IO.Drawbar.Pin);
                    
                    var timer = new System.Windows.Forms.Timer { Interval = 3000 };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        try
                        {
                            _uc.Clroutpin(config.IO.Drawbar.Port, config.IO.Drawbar.Pin);
                            LogMessage("Drawbar auto-closed");
                        }
                        catch { }
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Drawbar test error: {ex.Message}");
            }
        }

        private void btnTestAirBlow_Click(object sender, EventArgs e)
        {
            var config = _configStore.Config;
            if (!config.IO.AirBlow.Enabled)
            {
                MessageBox.Show(this, "Air blow is not enabled.", "Info", MessageBoxButtons.OK);
                return;
            }

            try
            {
                int ledNum = GetLEDForPin(config.IO.AirBlow.Port, config.IO.AirBlow.Pin);
                bool currentState = _uc.GetLED(ledNum);
                
                if (currentState)
                {
                    LogMessage("Air blow OFF");
                    _uc.Clroutpin(config.IO.AirBlow.Port, config.IO.AirBlow.Pin);
                }
                else
                {
                    LogMessage("Air blow ON");
                    _uc.Setoutpin(config.IO.AirBlow.Port, config.IO.AirBlow.Pin);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Air blow test error: {ex.Message}");
            }
        }

        private void chkSimulation_CheckedChanged(object sender, EventArgs e)
        {
            _configStore.UpdateConfig(c => c.Simulation.Enabled = chkSimulation.Checked);
            LogMessage($"Simulation mode: {(chkSimulation.Checked ? "ENABLED" : "DISABLED")}");
        }

        private void chkSimSlot_CheckedChanged(object sender, EventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk == null) return;
            
            int slotIndex = Array.IndexOf(chkSimSlots, chk);
            if (slotIndex >= 0 && slotIndex < 10)
            {
                _configStore.UpdateConfig(c => c.Simulation.SlotStates[slotIndex] = chk.Checked);
            }
        }

        private void chkSimPressureOK_CheckedChanged(object sender, EventArgs e)
        {
            _configStore.UpdateConfig(c => c.Simulation.PressureOK = chkSimPressureOK.Checked);
        }

        private void chkSimOssdOK_CheckedChanged(object sender, EventArgs e)
        {
            _configStore.UpdateConfig(c => c.Simulation.OssdOK = chkSimOssdOK.Checked);
        }

        private void btnRunTestCycle_Click(object sender, EventArgs e)
        {
            if (!_configStore.Config.Simulation.Enabled)
            {
                MessageBox.Show(this, "Enable simulation mode first.", "Simulation Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_testWorker.IsBusy)
            {
                MessageBox.Show(this, "Test cycle already running.", "Busy", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            LogMessage("Starting simulated tool change cycle...");
            
            int testTool = 1;
            for (int i = 0; i < 10; i++)
            {
                if (_configStore.Config.Simulation.SlotStates[i])
                {
                    testTool = _configStore.Config.Slots[i].ToolNumber;
                    if (testTool > 0) break;
                    testTool = i + 1;
                    break;
                }
            }

            _testWorker.RunWorkerAsync(testTool);
        }

        private void TestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int testTool = (int)e.Argument;
            PluginLogger.Log($"Test cycle starting on background thread for T{testTool}");
            string error = _stateMachine.StartToolChange(testTool);
            e.Result = error;
        }

        private void TestWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            string error = e.Result as string;
            
            if (!string.IsNullOrEmpty(error))
            {
                LogMessage($"Test cycle error: {error}");
                MessageBox.Show(this, $"Test cycle error: {error}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                LogMessage("Test cycle completed successfully");
            }
            
            _configStore.Save();
        }

        private void SlotPanel_Click(object sender, EventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;
            
            int slotIndex = Array.IndexOf(slotPanels, panel);
            if (slotIndex < 0 || slotIndex >= 10) return;
            
            int slotNumber = slotIndex + 1;
            var slot = _configStore.Config.Slots[slotIndex];
            
            if (!slot.Enabled)
            {
                LogMessage($"Slot {slotNumber} is disabled");
                return;
            }
            
            int currentToolInSlot = _stateMachine.ToolTable.FindToolInSlot(slotNumber);
            
            int? newTool = ToolChangeDialogs.ShowSlotAssignmentDialog(slotNumber, currentToolInSlot);
            
            if (!newTool.HasValue)
                return;
            
            int newToolNum = newTool.Value;
            
            if (newToolNum < 0 || newToolNum > 96)
            {
                LogMessage("Invalid tool number (0-96)");
                PlayErrorSound();
                return;
            }
            
            if (newToolNum == currentToolInSlot && newToolNum > 0)
            {
                LogMessage($"Same Tool T{newToolNum} already in slot {slotNumber}");
                PlayErrorSound();
                return;
            }
            
            if (newToolNum > 0)
            {
                int existingSlot = _stateMachine.ToolTable.FindToolSlot(newToolNum);
                if (existingSlot > 0 && existingSlot != slotNumber)
                {
                    LogMessage($"T{newToolNum} is already in Slot {existingSlot}");
                    PlayErrorSound();
                    return;
                }
            }
            
            if (currentToolInSlot > 0)
            {
                _stateMachine.ToolTable.WriteToolSlot(currentToolInSlot, 0);
            }
            
            if (newToolNum > 0)
            {
                _stateMachine.ToolTable.WriteToolSlot(newToolNum, slotNumber);
            }
            
            _stateMachine.ToolTable.SaveToolTable();
            
            slot.ToolNumber = newToolNum;
            _configStore.Save();
            
            if (currentToolInSlot > 0 && newToolNum > 0)
            {
                LogMessage($"Slot {slotNumber}: T{currentToolInSlot} replaced by T{newToolNum}");
            }
            else if (newToolNum > 0)
            {
                LogMessage($"Slot {slotNumber}: T{newToolNum} assigned");
            }
            else
            {
                LogMessage($"Slot {slotNumber}: Cleared (was T{currentToolInSlot})");
            }
            
            UpdateSlotDisplay();
        }

        private void btnRemoveToolFromSpindle_Click(object sender, EventArgs e)
        {
            int currentTool = 0;
            try { currentTool = _uc.Getcurrenttool(); }
            catch { }
            
            if (currentTool == 0)
            {
                LogMessage("No tool in spindle");
                return;
            }
            
            DialogResult result = ToolChangeDialogs.ShowConfirmRemoveFromSpindleDialog(currentTool);
            
            if (result == DialogResult.OK)
            {
                try
                {
                    _uc.Setcurrenttool(0);
                    LogMessage($"Tool T{currentTool} removed from spindle");
                    UpdateStatusDisplay();
                    UpdateSlotDisplay();
                }
                catch (Exception ex)
                {
                    LogMessage($"Error: {ex.Message}");
                }
            }
        }

        private void btnEditTool_Click(object sender, EventArgs e)
        {
            int? toolNum = ShowToolNumberInputDialog();
            if (!toolNum.HasValue)
                return;
            
            var info = _stateMachine.ToolTable.ReadToolInfo(toolNum.Value);
            if (info == null)
            {
                info = new ToolInfo { ToolNumber = toolNum.Value, Diameter = 0, Length = 0, Type = "NRH" };
            }
            
            double diameter = info.Diameter;
            double length = info.Length;
            string toolType = info.Type;
            
            if (ToolChangeDialogs.ShowToolEditDialog(toolNum.Value, ref diameter, ref length, ref toolType))
            {
                _stateMachine.ToolTable.WriteToolInfo(toolNum.Value, diameter, length, toolType);
                _stateMachine.ToolTable.SaveToolTable();
                LogMessage($"Tool T{toolNum.Value} updated: D={diameter:F3}, L={length:F3}, Type={toolType}");
            }
        }

        private int? ShowToolNumberInputDialog()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Edit Tool";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(250, 120);

                Label label = new Label
                {
                    Text = "Enter tool number to edit (1-96):",
                    Top = 15,
                    Left = 15,
                    Width = 220
                };

                TextBox inputBox = new TextBox
                {
                    Left = 75,
                    Top = 45,
                    Width = 100,
                    Text = "1"
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 50,
                    Top = 80,
                    Width = 70,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 130,
                    Top = 80,
                    Width = 70,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(inputBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    int toolNum;
                    if (int.TryParse(inputBox.Text, out toolNum) && toolNum >= 1 && toolNum <= 96)
                    {
                        return toolNum;
                    }
                    MessageBox.Show("Invalid tool number (1-96)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
            }
        }
        
        private void PlayErrorSound()
        {
            try
            {
                string soundPath = System.IO.Path.Combine(
                    Application.StartupPath,
                    @"Flashscreen\SOUND\error.wav");
                if (System.IO.File.Exists(soundPath))
                {
                    var player = new System.Media.SoundPlayer(soundPath);
                    player.Play();
                }
            }
            catch { }
        }

        public void ForceClose()
        {
            _forceClose = true;
            _isClosing = true;
            
            if (_uiTimer != null)
            {
                _uiTimer.Stop();
                _uiTimer.Dispose();
                _uiTimer = null;
            }
            
            try
            {
                if (!this.IsDisposed)
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() => this.Close()));
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
            catch { }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PluginLogger.LogFormClose("MainForm");
            _isClosing = true;
            
            if (_uiTimer != null)
            {
                _uiTimer.Stop();
                _uiTimer.Dispose();
                _uiTimer = null;
            }
            
            if (!_forceClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }
        }
    }
}
