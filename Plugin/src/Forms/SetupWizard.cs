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
        private const int TotalSteps = 17;

        private readonly string[] StepTitles = new string[]
        {
            "Welcome",
            "Rack Orientation",
            "Slot 1 Position",
            "Slot Spacing",
            "Z Heights",
            "Drawbar Output",
            "Air Blow",
            "Dust Hood",
            "Pressure Sensor",
            "Spindle Tool Sensor",
            "Slot Sensors",
            "Safety Inputs",
            "Tool Length Probe",
            "Enable Slots",
            "Speeds & Timing",
            "Options",
            "Summary"
        };

        public SetupWizard(ConfigStore configStore, Plugininterface.Entry uc)
        {
            _configStore = configStore;
            _uc = uc;
            _tempConfig = CloneConfig(_configStore.Config);
            
            InitializeComponent();
            PreFillFromUCCNC();
            SetupWizardUI();
            ShowStep(0);
        }

        private void PreFillFromUCCNC()
        {
            try
            {
                string fastFeedStr = _uc.Getfield(true, 2709);
                if (double.TryParse(fastFeedStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double fastFeed))
                    _tempConfig.Probe.FastFeed = fastFeed;
                
                string slowFeedStr = _uc.Getfield(true, 2710);
                if (double.TryParse(slowFeedStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double slowFeed))
                    _tempConfig.Probe.SlowFeed = slowFeed;
                
                string retractStr = _uc.Getfield(true, 2706);
                if (double.TryParse(retractStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double retract))
                    _tempConfig.Probe.Retract = retract;
                
                bool dualCycle = _uc.GetLED(853);
                _tempConfig.Probe.DualCycle = dualCycle;
                
                string minZStr = _uc.Getfield(true, 41);
                if (double.TryParse(minZStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double minZ))
                    _tempConfig.Probe.MinZ = minZ;
            }
            catch { }
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
            clone.IO.DustHood.Enabled = source.IO.DustHood.Enabled;
            clone.IO.DustHood.Port = source.IO.DustHood.Port;
            clone.IO.DustHood.Pin = source.IO.DustHood.Pin;
            clone.IO.DustHood.RaiseBeforeTC = source.IO.DustHood.RaiseBeforeTC;
            clone.IO.DustHood.LowerAfterTC = source.IO.DustHood.LowerAfterTC;
            clone.IO.PressureSensor.Enabled = source.IO.PressureSensor.Enabled;
            clone.IO.PressureSensor.Port = source.IO.PressureSensor.Port;
            clone.IO.PressureSensor.Pin = source.IO.PressureSensor.Pin;
            clone.IO.SpindleToolSensor.Enabled = source.IO.SpindleToolSensor.Enabled;
            clone.IO.SpindleToolSensor.Port = source.IO.SpindleToolSensor.Port;
            clone.IO.SpindleToolSensor.Pin = source.IO.SpindleToolSensor.Pin;
            clone.IO.SpindleToolSensor.ActiveLow = source.IO.SpindleToolSensor.ActiveLow;
            clone.IO.SlotSensors.Enabled = source.IO.SlotSensors.Enabled;
            clone.IO.SlotSensors.Port = source.IO.SlotSensors.Port;
            Array.Copy(source.IO.SlotSensors.Pins, clone.IO.SlotSensors.Pins, 10);
            
            clone.Safety.Enabled = source.Safety.Enabled;
            clone.Safety.OssdInputs.Clear();
            foreach (var ossd in source.Safety.OssdInputs)
            {
                clone.Safety.OssdInputs.Add(new OssdInputConfig
                {
                    Name = ossd.Name,
                    LedNumber = ossd.LedNumber,
                    ActiveLow = ossd.ActiveLow,
                    Enabled = ossd.Enabled
                });
            }
            
            clone.Probe.UseUCCNCSettings = source.Probe.UseUCCNCSettings;
            clone.Probe.FastFeed = source.Probe.FastFeed;
            clone.Probe.SlowFeed = source.Probe.SlowFeed;
            clone.Probe.Retract = source.Probe.Retract;
            clone.Probe.DualCycle = source.Probe.DualCycle;
            clone.Probe.MinZ = source.Probe.MinZ;
            
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
            
            HideAllPanels();
            
            switch (step)
            {
                case 0: panelWelcome.Visible = true; break;
                case 1: panelRackOrientation.Visible = true; break;
                case 2: panelSlot1Position.Visible = true; UpdateSlot1Labels(); break;
                case 3: panelSlotPitch.Visible = true; break;
                case 4: panelZHeights.Visible = true; break;
                case 5: panelDrawbar.Visible = true; break;
                case 6: panelAirBlow.Visible = true; break;
                case 7: panelDustHood.Visible = true; break;
                case 8: panelPressureSensor.Visible = true; break;
                case 9: panelSpindleToolSensor.Visible = true; break;
                case 10: panelSlotSensors.Visible = true; break;
                case 11: panelSafety.Visible = true; break;
                case 12: panelProbe.Visible = true; break;
                case 13: panelEnableSlots.Visible = true; break;
                case 14: panelFeedsAndTiming.Visible = true; break;
                case 15: panelOptions.Visible = true; break;
                case 16: panelSummary.Visible = true; UpdateSummary(); break;
            }
            
            btnBack.Enabled = step > 0;
            btnNext.Text = step < TotalSteps - 1 ? "Next >" : "Finish";
            
            PopulateStepControls(step);
        }

        private void HideAllPanels()
        {
            panelWelcome.Visible = false;
            panelRackOrientation.Visible = false;
            panelSlot1Position.Visible = false;
            panelSlotPitch.Visible = false;
            panelZHeights.Visible = false;
            panelDrawbar.Visible = false;
            panelAirBlow.Visible = false;
            panelDustHood.Visible = false;
            panelPressureSensor.Visible = false;
            panelSpindleToolSensor.Visible = false;
            panelSlotSensors.Visible = false;
            panelSafety.Visible = false;
            panelProbe.Visible = false;
            panelEnableSlots.Visible = false;
            panelFeedsAndTiming.Visible = false;
            panelOptions.Visible = false;
            panelSummary.Visible = false;
        }

        private void UpdateSlot1Labels()
        {
            string rackAxis = _tempConfig.Rack.Axis;
            string secondaryAxis = _tempConfig.Rack.AxisSecondary;
            
            lblSlot1XLabel.Text = $"{rackAxis} (along rack) - Machine coordinate (G53):";
            lblSlot1YLabel.Text = $"{secondaryAxis} (secondary) - Machine coordinate (G53):";
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
                    chkDustHoodEnabled.Checked = _tempConfig.IO.DustHood.Enabled;
                    nudDustHoodPort.Value = _tempConfig.IO.DustHood.Port;
                    nudDustHoodPin.Value = _tempConfig.IO.DustHood.Pin;
                    chkDustHoodRaise.Checked = _tempConfig.IO.DustHood.RaiseBeforeTC;
                    chkDustHoodLower.Checked = _tempConfig.IO.DustHood.LowerAfterTC;
                    break;
                    
                case 8:
                    chkPressureEnabled.Checked = _tempConfig.IO.PressureSensor.Enabled;
                    nudPressurePort.Value = _tempConfig.IO.PressureSensor.Port;
                    nudPressurePin.Value = _tempConfig.IO.PressureSensor.Pin;
                    break;
                    
                case 9:
                    chkSpindleToolSensorEnabled.Checked = _tempConfig.IO.SpindleToolSensor.Enabled;
                    nudSpindleToolSensorPort.Value = _tempConfig.IO.SpindleToolSensor.Port;
                    nudSpindleToolSensorPin.Value = _tempConfig.IO.SpindleToolSensor.Pin;
                    chkSpindleToolSensorActiveLow.Checked = _tempConfig.IO.SpindleToolSensor.ActiveLow;
                    break;
                    
                case 10:
                    chkSlotSensorsEnabled.Checked = _tempConfig.IO.SlotSensors.Enabled;
                    nudSlotSensorPort.Value = _tempConfig.IO.SlotSensors.Port;
                    for (int i = 0; i < 10; i++)
                    {
                        nudSlotPins[i].Value = _tempConfig.IO.SlotSensors.Pins[i];
                    }
                    break;
                    
                case 11:
                    chkSafetyEnabled.Checked = _tempConfig.Safety.Enabled;
                    PopulateSafetyInputs();
                    break;
                    
                case 12:
                    chkUseUCCNCProbe.Checked = _tempConfig.Probe.UseUCCNCSettings;
                    txtProbeFastFeed.Text = F(_tempConfig.Probe.FastFeed);
                    txtProbeSlowFeed.Text = F(_tempConfig.Probe.SlowFeed);
                    txtProbeRetract.Text = F(_tempConfig.Probe.Retract);
                    chkProbeDualCycle.Checked = _tempConfig.Probe.DualCycle;
                    txtProbeMinZ.Text = F(_tempConfig.Probe.MinZ);
                    UpdateProbeFieldsEnabled();
                    break;
                    
                case 13:
                    for (int i = 0; i < 10; i++)
                    {
                        chkSlotEnabled[i].Checked = _tempConfig.Slots[i].Enabled;
                    }
                    break;
                    
                case 14:
                    txtFeedRate.Text = F(_tempConfig.FeedsAndTiming.FeedRate);
                    txtOperationDelay.Text = _tempConfig.FeedsAndTiming.OperationDelay.ToString();
                    txtSensorWaitTime.Text = _tempConfig.FeedsAndTiming.SensorWaitTime.ToString();
                    break;
                    
                case 15:
                    chkApplyG43.Checked = _tempConfig.Options.ApplyG43;
                    chkReturnToXY.Checked = _tempConfig.Options.ReturnToXY;
                    chkMeasureNonRH.Checked = _tempConfig.Options.MeasureNonRH;
                    chkShowATCPage.Checked = _tempConfig.Options.ShowATCPage;
                    chkAirBlowDuringTC.Checked = _tempConfig.Options.AirBlowDuringTC;
                    break;
            }
        }

        private void PopulateSafetyInputs()
        {
            for (int i = 0; i < _tempConfig.Safety.OssdInputs.Count && i < 5; i++)
            {
                var ossd = _tempConfig.Safety.OssdInputs[i];
                txtOssdName[i].Text = ossd.Name;
                nudOssdLed[i].Value = ossd.LedNumber;
                chkOssdActiveLow[i].Checked = ossd.ActiveLow;
                chkOssdEnabled[i].Checked = ossd.Enabled;
            }
        }

        private void UpdateProbeFieldsEnabled()
        {
            bool manualEnabled = !chkUseUCCNCProbe.Checked;
            txtProbeFastFeed.Enabled = manualEnabled;
            txtProbeSlowFeed.Enabled = manualEnabled;
            txtProbeRetract.Enabled = manualEnabled;
            chkProbeDualCycle.Enabled = manualEnabled;
            txtProbeMinZ.Enabled = manualEnabled;
        }

        private bool ValidateStep(int step)
        {
            switch (step)
            {
                case 2:
                    if (!TryParseDouble(txtSlot1X.Text, out _))
                    {
                        ShowValidationError($"Invalid {_tempConfig.Rack.Axis} position for Slot 1");
                        return false;
                    }
                    if (!TryParseDouble(txtSlot1Y.Text, out _))
                    {
                        ShowValidationError($"Invalid {_tempConfig.Rack.AxisSecondary} position for Slot 1");
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
                    
                case 13:
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
                    
                case 14:
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
                    _tempConfig.IO.DustHood.Enabled = chkDustHoodEnabled.Checked;
                    _tempConfig.IO.DustHood.Port = (int)nudDustHoodPort.Value;
                    _tempConfig.IO.DustHood.Pin = (int)nudDustHoodPin.Value;
                    _tempConfig.IO.DustHood.RaiseBeforeTC = chkDustHoodRaise.Checked;
                    _tempConfig.IO.DustHood.LowerAfterTC = chkDustHoodLower.Checked;
                    break;
                    
                case 8:
                    _tempConfig.IO.PressureSensor.Enabled = chkPressureEnabled.Checked;
                    _tempConfig.IO.PressureSensor.Port = (int)nudPressurePort.Value;
                    _tempConfig.IO.PressureSensor.Pin = (int)nudPressurePin.Value;
                    break;
                    
                case 9:
                    _tempConfig.IO.SpindleToolSensor.Enabled = chkSpindleToolSensorEnabled.Checked;
                    _tempConfig.IO.SpindleToolSensor.Port = (int)nudSpindleToolSensorPort.Value;
                    _tempConfig.IO.SpindleToolSensor.Pin = (int)nudSpindleToolSensorPin.Value;
                    _tempConfig.IO.SpindleToolSensor.ActiveLow = chkSpindleToolSensorActiveLow.Checked;
                    break;
                    
                case 10:
                    _tempConfig.IO.SlotSensors.Enabled = chkSlotSensorsEnabled.Checked;
                    _tempConfig.IO.SlotSensors.Port = (int)nudSlotSensorPort.Value;
                    for (int i = 0; i < 10; i++)
                    {
                        _tempConfig.IO.SlotSensors.Pins[i] = (int)nudSlotPins[i].Value;
                    }
                    break;
                    
                case 11:
                    _tempConfig.Safety.Enabled = chkSafetyEnabled.Checked;
                    for (int i = 0; i < _tempConfig.Safety.OssdInputs.Count && i < 5; i++)
                    {
                        _tempConfig.Safety.OssdInputs[i].Name = txtOssdName[i].Text;
                        _tempConfig.Safety.OssdInputs[i].LedNumber = (int)nudOssdLed[i].Value;
                        _tempConfig.Safety.OssdInputs[i].ActiveLow = chkOssdActiveLow[i].Checked;
                        _tempConfig.Safety.OssdInputs[i].Enabled = chkOssdEnabled[i].Checked;
                    }
                    break;
                    
                case 12:
                    _tempConfig.Probe.UseUCCNCSettings = chkUseUCCNCProbe.Checked;
                    TryParseDouble(txtProbeFastFeed.Text, out double fastFeed);
                    TryParseDouble(txtProbeSlowFeed.Text, out double slowFeed);
                    TryParseDouble(txtProbeRetract.Text, out double retract);
                    TryParseDouble(txtProbeMinZ.Text, out double minZ);
                    _tempConfig.Probe.FastFeed = fastFeed;
                    _tempConfig.Probe.SlowFeed = slowFeed;
                    _tempConfig.Probe.Retract = retract;
                    _tempConfig.Probe.DualCycle = chkProbeDualCycle.Checked;
                    _tempConfig.Probe.MinZ = minZ;
                    break;
                    
                case 13:
                    for (int i = 0; i < 10; i++)
                    {
                        _tempConfig.Slots[i].Enabled = chkSlotEnabled[i].Checked;
                    }
                    break;
                    
                case 14:
                    TryParseDouble(txtFeedRate.Text, out double feedRate);
                    int.TryParse(txtOperationDelay.Text, out int delay);
                    int.TryParse(txtSensorWaitTime.Text, out int waitTime);
                    _tempConfig.FeedsAndTiming.FeedRate = feedRate;
                    _tempConfig.FeedsAndTiming.OperationDelay = delay;
                    _tempConfig.FeedsAndTiming.SensorWaitTime = waitTime;
                    break;
                    
                case 15:
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
            sb.AppendLine($"Slot 1: {_tempConfig.Rack.Axis}={F(_tempConfig.Rack.Slot1Position)}, {_tempConfig.Rack.AxisSecondary}={F(_tempConfig.Rack.Slot1PositionSecondary)} (G53)");
            sb.AppendLine($"Slot Pitch: {F(_tempConfig.Rack.Pitch)} mm");
            sb.AppendLine();
            
            sb.AppendLine("=== Z HEIGHTS (G53) ===");
            sb.AppendLine($"Z Safe: {F(_tempConfig.Heights.ZSafe)}");
            sb.AppendLine($"Z Empty: {F(_tempConfig.Heights.ZEmpty)}");
            sb.AppendLine($"Z Approach: {F(_tempConfig.Heights.ZApproach)}");
            sb.AppendLine($"Z Clamp: {F(_tempConfig.Heights.ZClamp)}");
            sb.AppendLine($"Z Check: {F(_tempConfig.Heights.ZCheck)}");
            sb.AppendLine();
            
            sb.AppendLine("=== I/O CONFIGURATION ===");
            sb.AppendLine($"Drawbar: Port {_tempConfig.IO.Drawbar.Port}, Pin {_tempConfig.IO.Drawbar.Pin}");
            sb.AppendLine($"Air Blow: {(_tempConfig.IO.AirBlow.Enabled ? $"Port {_tempConfig.IO.AirBlow.Port}, Pin {_tempConfig.IO.AirBlow.Pin}" : "Disabled")}");
            sb.AppendLine($"Dust Hood: {(_tempConfig.IO.DustHood.Enabled ? $"Port {_tempConfig.IO.DustHood.Port}, Pin {_tempConfig.IO.DustHood.Pin} (Raise:{_tempConfig.IO.DustHood.RaiseBeforeTC}, Lower:{_tempConfig.IO.DustHood.LowerAfterTC})" : "Disabled")}");
            sb.AppendLine($"Pressure Sensor: {(_tempConfig.IO.PressureSensor.Enabled ? $"Port {_tempConfig.IO.PressureSensor.Port}, Pin {_tempConfig.IO.PressureSensor.Pin}" : "Disabled")}");
            sb.AppendLine($"Spindle Tool Sensor: {(_tempConfig.IO.SpindleToolSensor.Enabled ? $"Port {_tempConfig.IO.SpindleToolSensor.Port}, Pin {_tempConfig.IO.SpindleToolSensor.Pin} ({(_tempConfig.IO.SpindleToolSensor.ActiveLow ? "Active Low" : "Active High")})" : "Disabled")}");
            sb.AppendLine($"Slot Sensors: {(_tempConfig.IO.SlotSensors.Enabled ? $"Port {_tempConfig.IO.SlotSensors.Port}" : "Disabled")}");
            sb.AppendLine();
            
            sb.AppendLine("=== SAFETY INPUTS ===");
            sb.AppendLine($"Safety Monitoring: {(_tempConfig.Safety.Enabled ? "Enabled" : "Disabled")}");
            if (_tempConfig.Safety.Enabled)
            {
                foreach (var ossd in _tempConfig.Safety.OssdInputs)
                {
                    if (ossd.Enabled)
                    {
                        sb.AppendLine($"  {ossd.Name}: LED {ossd.LedNumber} ({(ossd.ActiveLow ? "Active Low" : "Active High")})");
                    }
                }
            }
            sb.AppendLine();
            
            sb.AppendLine("=== TOOL LENGTH PROBE ===");
            sb.AppendLine($"Use UCCNC Settings: {(_tempConfig.Probe.UseUCCNCSettings ? "Yes" : "No")}");
            if (!_tempConfig.Probe.UseUCCNCSettings)
            {
                sb.AppendLine($"  Fast Feed: {F(_tempConfig.Probe.FastFeed)} mm/min");
                sb.AppendLine($"  Slow Feed: {F(_tempConfig.Probe.SlowFeed)} mm/min");
                sb.AppendLine($"  Retract: {F(_tempConfig.Probe.Retract)} mm");
                sb.AppendLine($"  Dual Cycle: {(_tempConfig.Probe.DualCycle ? "Yes" : "No")}");
                sb.AppendLine($"  Min Z: {F(_tempConfig.Probe.MinZ)}");
            }
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
                PluginLogger.Log("Setup Wizard: Finishing and saving configuration");
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
                c.IO.DustHood.Enabled = _tempConfig.IO.DustHood.Enabled;
                c.IO.DustHood.Port = _tempConfig.IO.DustHood.Port;
                c.IO.DustHood.Pin = _tempConfig.IO.DustHood.Pin;
                c.IO.DustHood.RaiseBeforeTC = _tempConfig.IO.DustHood.RaiseBeforeTC;
                c.IO.DustHood.LowerAfterTC = _tempConfig.IO.DustHood.LowerAfterTC;
                c.IO.PressureSensor.Enabled = _tempConfig.IO.PressureSensor.Enabled;
                c.IO.PressureSensor.Port = _tempConfig.IO.PressureSensor.Port;
                c.IO.PressureSensor.Pin = _tempConfig.IO.PressureSensor.Pin;
                c.IO.SpindleToolSensor.Enabled = _tempConfig.IO.SpindleToolSensor.Enabled;
                c.IO.SpindleToolSensor.Port = _tempConfig.IO.SpindleToolSensor.Port;
                c.IO.SpindleToolSensor.Pin = _tempConfig.IO.SpindleToolSensor.Pin;
                c.IO.SpindleToolSensor.ActiveLow = _tempConfig.IO.SpindleToolSensor.ActiveLow;
                c.IO.SlotSensors.Enabled = _tempConfig.IO.SlotSensors.Enabled;
                c.IO.SlotSensors.Port = _tempConfig.IO.SlotSensors.Port;
                Array.Copy(_tempConfig.IO.SlotSensors.Pins, c.IO.SlotSensors.Pins, 10);
                
                c.Safety.Enabled = _tempConfig.Safety.Enabled;
                c.Safety.OssdInputs.Clear();
                foreach (var ossd in _tempConfig.Safety.OssdInputs)
                {
                    c.Safety.OssdInputs.Add(new OssdInputConfig
                    {
                        Name = ossd.Name,
                        LedNumber = ossd.LedNumber,
                        ActiveLow = ossd.ActiveLow,
                        Enabled = ossd.Enabled
                    });
                }
                
                c.Probe.UseUCCNCSettings = _tempConfig.Probe.UseUCCNCSettings;
                c.Probe.FastFeed = _tempConfig.Probe.FastFeed;
                c.Probe.SlowFeed = _tempConfig.Probe.SlowFeed;
                c.Probe.Retract = _tempConfig.Probe.Retract;
                c.Probe.DualCycle = _tempConfig.Probe.DualCycle;
                c.Probe.MinZ = _tempConfig.Probe.MinZ;
                
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
                else if (btn == btnReadZApproach) txtZApproach.Text = F(z);
                else if (btn == btnReadZClamp) txtZClamp.Text = F(z);
                else if (btn == btnReadZCheck) txtZCheck.Text = F(z);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error reading Z position: {ex.Message}", "Error",
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

        private void chkUseUCCNCProbe_CheckedChanged(object sender, EventArgs e)
        {
            UpdateProbeFieldsEnabled();
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
