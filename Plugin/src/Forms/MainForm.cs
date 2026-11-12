using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
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
        private readonly UCCNCplugin _plugin;
        
        private bool _mustClose;
        private bool _isClosing;

        public MainForm(UCCNCplugin plugin, Plugininterface.Entry uc, ConfigStore configStore)
        {
            _plugin = plugin;
            _uc = uc;
            _configStore = configStore;
            
            _stateMachine = new ATCStateMachine(
                _configStore.Config, 
                _uc, 
                LogMessage, 
                () => _configStore.Config.Simulation.Enabled);
            _safetyMonitor = new SafetyMonitor(
                _configStore.Config, 
                _uc, 
                LogMessage, 
                () => _configStore.Config.Simulation.Enabled);

            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "HTM ATC - Automatic Tool Changer";
            UpdateUIFromConfig();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            
            if (!_configStore.ConfigExists)
            {
                this.BeginInvoke(new Action(() =>
                {
                    ShowSetupWizard();
                }));
            }
        }

        public void UpdateLoop()
        {
            if (_isClosing) return;

            try
            {
                _safetyMonitor.CheckSafety();
                UpdateStatusDisplay();
                UpdateSlotDisplay();
                UpdateDiagnosticsDisplay();
            }
            catch (Exception)
            {
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
        }

        private void UpdateSlotDisplay()
        {
            var config = _configStore.Config;
            
            for (int i = 0; i < 10; i++)
            {
                var slot = config.Slots[i];
                var panel = slotPanels[i];
                
                if (panel == null) continue;

                var lblTool = panel.Controls["lblTool"] as Label;
                var ledSensor = panel.Controls["ledSensor"] as Panel;
                var ledEnabled = panel.Controls["ledEnabled"] as Panel;
                
                if (lblTool != null)
                {
                    lblTool.Text = slot.ToolNumber > 0 ? $"T{slot.ToolNumber}" : "-";
                }
                
                if (ledEnabled != null)
                {
                    ledEnabled.BackColor = slot.Enabled ? Color.Lime : Color.Gray;
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
                if (txtLog.InvokeRequired)
                {
                    txtLog.BeginInvoke(new Action(() => LogMessage(message)));
                    return;
                }

                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                txtLog.AppendText($"[{timestamp}] {message}\r\n");
                txtLog.ScrollToCaret();
            }
            catch { }
        }

        private void ShowSetupWizard()
        {
            using (var wizard = new SetupWizard(_configStore, _uc))
            {
                if (wizard.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateUIFromConfig();
                    LogMessage("Configuration saved from setup wizard");
                }
            }
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
            
            if (!_safetyMonitor.AreAllAxesHomed())
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
                    
                    var timer = new Timer { Interval = 3000 };
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

            string error = _stateMachine.StartToolChange(testTool);
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
        }

        public string ExecuteToolChange(int newTool)
        {
            LogMessage($"M6 T{newTool} requested");
            string error = _stateMachine.StartToolChange(newTool);
            
            if (!string.IsNullOrEmpty(error))
            {
                LogMessage($"Tool change error: {error}");
            }
            
            _configStore.Save();
            return error;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_mustClose)
            {
                e.Cancel = true;
                _isClosing = true;
                
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    _plugin.StopLoop();
                    System.Threading.Thread.Sleep(100);
                    _mustClose = true;
                    
                    try
                    {
                        this.BeginInvoke(new Action(() => this.Close()));
                    }
                    catch { }
                });
            }
        }
    }
}
