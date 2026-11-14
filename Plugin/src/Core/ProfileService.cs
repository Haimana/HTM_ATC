using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using HTM_PRO.Config;
using HTM_PRO.Utils;

namespace HTM_PRO.Core
{
    /// <summary>
    /// Reads and writes complete Stage 1 machine settings across UCCNC's active GUI setup fields,
    /// UCCNC's .pro INI store (via UC.Writekey / B168 Apply / B167 Save), JSON config, and wiring summary export.
    /// </summary>
    public class ProfileService
    {
        private readonly Plugininterface.Entry _uc;
        private readonly string _profilesDir;
        private readonly string _profileName;
        private readonly string _jsonConfigPath;
        private readonly string _pinoutSummaryPath;

        public MachineConfig Config { get; private set; }
        public string ProfileName => _profileName;
        public string ProfilesDirectory => _profilesDir;
        public string PinoutSummaryPath => _pinoutSummaryPath;

        public ProfileService(string startupPath, string profileName, Plugininterface.Entry uc)
        {
            _uc = uc;
            _profilesDir = Path.Combine(startupPath, "Profiles");
            _profileName = string.IsNullOrWhiteSpace(profileName) ? "Default" : profileName.Trim();
            _jsonConfigPath = Path.Combine(_profilesDir, $"HTM_PRO_Config_{_profileName}.json");
            _pinoutSummaryPath = Path.Combine(_profilesDir, $"HTM_PRO_Pinout_Summary_{_profileName}.txt");
            Config = new MachineConfig();
        }

        /// <summary>
        /// Loads saved HTM_PRO configuration (JSON + UCCNC profile) and detects active controller & units.
        /// </summary>
        public void Load(Form mainForm)
        {
            try
            {
                bool loadedJson = false;
                if (File.Exists(_jsonConfigPath))
                {
                    string json = File.ReadAllText(_jsonConfigPath);
                    var ser = new JavaScriptSerializer();
                    var deserialized = ser.Deserialize<MachineConfig>(json);
                    if (deserialized != null)
                    {
                        Config = deserialized;
                        Config.EnsureSixAxes();
                        loadedJson = true;
                        PluginLogger.Log($"Loaded JSON configuration from {_jsonConfigPath} (Stage1Completed={Config.Stage1Completed}).");
                    }
                }

                // Always refresh hardware controller detection from UCCNC if not overridden
                var detected = HardwareCatalog.DetectActiveController(mainForm);
                if (!loadedJson || string.IsNullOrWhiteSpace(Config.ControllerType))
                {
                    // Check if .pro has [HTM_PRO] ControllerType
                    string iniCtrl = ReadIni("HTM_PRO", "ControllerType", "");
                    Config.ControllerType = !string.IsNullOrWhiteSpace(iniCtrl)
                        ? HardwareCatalog.ParseModel(iniCtrl, detected.Model).ToString()
                        : detected.Model.ToString();
                }

                if (!loadedJson)
                {
                    bool iniCompleted = ReadIniBool("HTM_PRO", "Stage1Completed", false);
                    bool iniImperial = ReadIniBool("HTM_PRO", "IsImperial", DetectImperialFromUccncProfile());
                    Config.Stage1Completed = iniCompleted;
                    Config.ApplyUnitBaselineDefaults(iniImperial);

                    if (iniCompleted || HasExistingConfiguredAxesInProfile())
                    {
                        ImportExistingUccncProfileSettings(Config);
                    }
                    else
                    {
                        var spec = HardwareCatalog.GetSpec(HardwareCatalog.ParseModel(Config.ControllerType));
                        PinAllocator.AutoAssignOutputs(Config, spec);
                        PinAllocator.AutoAssignInputs(Config, spec);
                    }
                }
                else
                {
                    // Re-verify unit system if Stage 1 wasn't completed yet
                    if (!Config.Stage1Completed)
                    {
                        Config.IsImperial = DetectImperialFromUccncProfile();
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("ProfileService.Load", ex);
                Config = new MachineConfig();
                Config.ApplyUnitBaselineDefaults(DetectImperialFromUccncProfile());
            }
        }

        /// <summary>
        /// Detects Metric (mm) vs Imperial (decimal inch) from standard UCCNC profile values.
        /// </summary>
        public bool DetectImperialFromUccncProfile()
        {
            try
            {
                int posDigits = ReadIniInt("Generalsettings", "POSDROsdigits", 2);
                double linearErr = ReadIniDouble("Generalsettings", "Linearerror", 0.03);
                double cornerErr = ReadIniDouble("Generalsettings", "Cornerserror", 0.03);
                double xVel = ReadIniDouble("axessettingscontrolX", "Velocity", 2000.0);
                double safeZ = ReadIniDouble("Generalsettings", "SafeZ", 25.0);

                int signals = 0;
                if (posDigits >= 4) signals++;
                if (linearErr > 0 && linearErr <= 0.005) signals++;
                if (cornerErr > 0 && cornerErr <= 0.005) signals++;
                if (xVel > 0 && xVel <= 250.0) signals++;
                if (safeZ > 0 && safeZ <= 4.0) signals++;

                return signals >= 2 || (linearErr > 0 && linearErr <= 0.005);
            }
            catch
            {
                return false;
            }
        }

        private bool HasExistingConfiguredAxesInProfile()
        {
            int xStep = ReadIniInt("axessettingscontrolX", "Steppinnumber", 0);
            int yStep = ReadIniInt("axessettingscontrolY", "Steppinnumber", 0);
            int zStep = ReadIniInt("axessettingscontrolZ", "Steppinnumber", 0);
            return xStep > 0 && yStep > 0 && zStep > 0;
        }

        /// <summary>
        /// Imports current axis, I/O, spindle, and general configuration from the active UCCNC profile.
        /// </summary>
        public void ImportExistingUccncProfileSettings(MachineConfig target)
        {
            if (target == null) return;
            target.EnsureSixAxes();

            var slaveToMasterMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string master in new[] { "X", "Y", "Z" })
            {
                string sl = ReadIni($"axessettingscontrol{master}", "Slaveaxis", "None");
                if (!string.IsNullOrWhiteSpace(sl) && sl != "None")
                {
                    slaveToMasterMap[sl] = master;
                }
            }

            foreach (var ax in target.Axes)
            {
                string sec = $"axessettingscontrol{ax.AxisName}";
                ax.Enabled = ReadIniBool(sec, "Axisenabled", ax.AxisName == "X" || ax.AxisName == "Y" || ax.AxisName == "Z");
                ax.StepPort = ReadIniInt(sec, "Stepportnumber", 0);
                ax.StepPin = ReadIniInt(sec, "Steppinnumber", 0);
                ax.StepNegate = ReadIniBool(sec, "Steppinnegate", false);

                ax.DirPort = ReadIniInt(sec, "Dirportnumber", 0);
                ax.DirPin = ReadIniInt(sec, "Dirpinnumber", 0);
                ax.DirNegate = ReadIniBool(sec, "Dirpinnegate", false);

                ax.EnaPort = ReadIniInt(sec, "Enaportnumber", 0);
                ax.EnaPin = ReadIniInt(sec, "Enapinnumber", 0);
                ax.EnaNegate = ReadIniBool(sec, "Enapinnegate", false);

                ax.HomePort = ReadIniInt(sec, "Homeportnumber", 0);
                ax.HomePin = ReadIniInt(sec, "Homepinnumber", 0);
                ax.HomeNegate = ReadIniBool(sec, "Homepinnegate", false);

                ax.LimitMinusPort = ReadIniInt(sec, "Limitminusportnumber", 0);
                ax.LimitMinusPin = ReadIniInt(sec, "Limitminuspinnumber", 0);
                ax.LimitMinusNegate = ReadIniBool(sec, "Limitminuspinnegate", false);

                ax.LimitPlusPort = ReadIniInt(sec, "Limitplusportnumber", 0);
                ax.LimitPlusPin = ReadIniInt(sec, "Limitpluspinnumber", 0);
                ax.LimitPlusNegate = ReadIniBool(sec, "Limitpluspinnegate", false);

                ax.StepsPerUnit = ReadIniDouble(sec, "Stepsperunit", ax.StepsPerUnit);
                ax.MaxVelocity = ReadIniDouble(sec, "Velocity", ax.MaxVelocity);
                ax.Acceleration = ReadIniDouble(sec, "Acceleration", ax.Acceleration);
                ax.SoftLimitNegative = ReadIniDouble(sec, "Softlimitnegative", ax.SoftLimitNegative);
                ax.SoftLimitPositive = ReadIniDouble(sec, "Softlimitpositive", ax.SoftLimitPositive);

                double span = Math.Abs(ax.SoftLimitPositive - ax.SoftLimitNegative);
                if (span > 0 && span < 50000)
                {
                    ax.MaxTravel = span;
                }

                ax.HomeDirectionPositive = ReadIniBool(sec, "Homedirectionpositive", ax.AxisName == "Z");
                ax.HomeAutoZero = ReadIniBool(sec, "Homeautozero", true);
                ax.HomingSpeedUp = ReadIniDouble(sec, "Homingspeedup", ax.HomingSpeedUp);
                ax.HomingSpeedDown = ReadIniDouble(sec, "Homingspeeddown", ax.HomingSpeedDown);
                ax.HomeBackoff = ReadIniDouble(sec, "Homebackoff", ax.HomeBackoff);
                ax.IsRotary = ReadIniBool(sec, "Rotaryaxis", false);
                ax.RotaryRollover = ReadIniBool(sec, "Rotaryrollover", false);

                if (slaveToMasterMap.TryGetValue(ax.AxisName, out string mName))
                {
                    ax.Role = AxisRole.GantrySlave;
                    ax.MasterAxis = mName;
                }
                else if (ax.IsRotaryChannel)
                {
                    ax.MasterAxis = "None";
                    ax.Role = ax.Enabled ? AxisRole.RotaryWorkAxis : AxisRole.Unused;
                }
                else
                {
                    ax.MasterAxis = "None";
                    ax.Role = ax.Enabled ? AxisRole.PrimaryLinear : AxisRole.Unused;
                }
            }

            // General & IO & Spindle
            target.EnableSoftLimits = ReadIniBool("Generalsettings", "Softlimitsenable", false);
            target.CheckHomeOnCycleStart = ReadIniBool("Generalsettings", "CheckHomeonCycleStart", true);
            target.SafeZ = ReadIniDouble("Generalsettings", "SafeZ", target.IsImperial ? 1.0 : 25.0);
            for (int i = 0; i < 6; i++)
            {
                target.HomeSequence[i] = ReadIni("Generalsettings", $"Homesequence{i + 1}", target.HomeSequence[i]);
            }

            target.EstopPort = ReadIniInt("IOsetupsettings", "Estopportnumber", 0);
            target.EstopPin = ReadIniInt("IOsetupsettings", "Estoppinnumber", 0);
            target.EstopNegate = ReadIniBool("IOsetupsettings", "Estoppinnegate", false);

            target.Probe1Port = ReadIniInt("IOsetupsettings", "Probeportnumber", 0);
            target.Probe1Pin = ReadIniInt("IOsetupsettings", "Probepinnumber", 0);
            target.Probe1Negate = ReadIniBool("IOsetupsettings", "Probepinnegate", false);

            target.Probe2Port = ReadIniInt("IOsetupsettings", "Probeport2number", 0);
            target.Probe2Pin = ReadIniInt("IOsetupsettings", "Probepin2number", 0);
            target.Probe2Negate = ReadIniBool("IOsetupsettings", "Probepin2negate", false);
            target.UseProbe2 = target.Probe2Port > 0 && target.Probe2Pin > 0;

            bool pwmMode = ReadIniBool("Spindlesetupsettings", "IsSpindleinPWMMode", true);
            target.SpindlePwmPort = ReadIniInt("Spindlesetupsettings", "SpindlePWMoutport", 0);
            target.SpindlePwmPin = ReadIniInt("Spindlesetupsettings", "SpindlePWMoutpin", 0);
            target.SpindlePwmNegate = ReadIniBool("Spindlesetupsettings", "SpindlePWMoutpinnegate", false);
            target.SpindlePwmFreq = (int)Math.Round(ReadIniDouble("Spindlesetupsettings", "SpindlePWMfrequency", 400));
            target.SpindleMinRpm = ReadIniDouble("Spindlesetupsettings", "Spindleminvelocity", 6000);
            target.SpindleMaxRpm = ReadIniDouble("Spindlesetupsettings", "Spindlemaxvelocity", 24000);
            target.M3RelayPort = ReadIniInt("Spindlesetupsettings", "M3relayport", 0);
            target.M3RelayPin = ReadIniInt("Spindlesetupsettings", "M3relaypin", 0);
            target.M3RelayNegate = ReadIniBool("Spindlesetupsettings", "M3relaypinnegate", false);
            target.M4RelayPort = ReadIniInt("Spindlesetupsettings", "M4relayport", 0);
            target.M4RelayPin = ReadIniInt("Spindlesetupsettings", "M4relaypin", 0);
            target.M4RelayNegate = ReadIniBool("Spindlesetupsettings", "M4relaypinnegate", false);
            target.UseM4Relay = target.M4RelayPort > 0 && target.M4RelayPin > 0;
            target.M7MistPort = ReadIniInt("Spindlesetupsettings", "M7relayport", 0);
            target.M7MistPin = ReadIniInt("Spindlesetupsettings", "M7relaypin", 0);
            target.M7MistNegate = ReadIniBool("Spindlesetupsettings", "M7relaypinnegate", false);
            target.UseMistM7 = target.M7MistPort > 0 && target.M7MistPin > 0;
            target.M8FloodPort = ReadIniInt("Spindlesetupsettings", "M8relayport", 0);
            target.M8FloodPin = ReadIniInt("Spindlesetupsettings", "M8relaypin", 0);
            target.M8FloodNegate = ReadIniBool("Spindlesetupsettings", "M8relaypinnegate", false);
            target.UseFloodM8 = target.M8FloodPort > 0 && target.M8FloodPin > 0;

            target.SpindleMode = pwmMode
                ? SpindleControlMode.PwmAndRelay
                : (ReadIniInt("Spindlesetupsettings", "Spindlesteppin", 0) > 0 ? SpindleControlMode.StepDirAndRelay : SpindleControlMode.RelayOrModbusOnly);
        }

        /// <summary>
        /// Writes Stage 1 configuration to UCCNC's live setup GUI controls, calls ApplySettings (B168)
        /// and SaveSettings (B167), persists [HTM_PRO] keys + JSON config, and exports the wiring summary.
        /// </summary>
        public void ApplyAndSaveToUccnc(MachineConfig cfg, Form mainForm)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            Config = cfg;
            cfg.EnsureSixAxes();
            cfg.SyncSlaveAxisParameters();
            PinAllocator.SyncSharedEnablePin(cfg);

            // Recompute SoftLimits consistently for enabled axes using Travel unless custom rotary rollover
            foreach (var ax in cfg.Axes)
            {
                if (ax.Enabled && ax.Role != AxisRole.GantrySlave)
                {
                    ax.RecalculateSoftLimitsFromTravel();
                }
            }
            cfg.SyncSlaveAxisParameters();

            // Must execute UI/OpenGL calls on UCCNC's Form1 thread
            if (mainForm != null && mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action(() => ApplyAndSaveOnUiThread(cfg, mainForm)));
            }
            else
            {
                ApplyAndSaveOnUiThread(cfg, mainForm);
            }

            cfg.Stage1Completed = true;
            SaveJsonAndHtmProKeys(cfg);
            ExportPinoutSummary(cfg);
            PluginLogger.Log($"Stage 1 configuration applied & saved to profile [{_profileName}].");
        }

        private void ApplyAndSaveOnUiThread(MachineConfig cfg, Form mainForm)
        {
            if (_uc == null) return;

            // 1. Update UCCNC GUI setup screen fields/checkboxes/comboboxes so ApplySettings (168) reads exact values
            for (int i = 0; i < 6; i++)
            {
                var ax = cfg.Axes[i];
                bool isActiveInUccnc = ax.Enabled;

                _uc.Setcheckboxstate(true, isActiveInUccnc, 1 + i * 9);
                _uc.Setfieldtext(true, ax.StepPin.ToString(CultureInfo.InvariantCulture), 1 + i * 15);
                _uc.Setfieldtext(true, ax.StepPort.ToString(CultureInfo.InvariantCulture), 241 + i * 5);
                _uc.Setcheckboxstate(true, ax.StepNegate, 2 + i * 9);

                _uc.Setfieldtext(true, ax.DirPin.ToString(CultureInfo.InvariantCulture), 2 + i * 15);
                _uc.Setfieldtext(true, ax.DirPort.ToString(CultureInfo.InvariantCulture), 242 + i * 5);
                _uc.Setcheckboxstate(true, ax.DirNegate, 3 + i * 9);

                _uc.Setfieldtext(true, ax.EnaPin.ToString(CultureInfo.InvariantCulture), 463 + i * 2);
                _uc.Setfieldtext(true, ax.EnaPort.ToString(CultureInfo.InvariantCulture), 464 + i * 2);
                _uc.Setcheckboxstate(true, ax.EnaNegate, 141 + i);

                _uc.Setfieldtext(true, ax.LimitMinusPin.ToString(CultureInfo.InvariantCulture), 3 + i * 15);
                _uc.Setfieldtext(true, ax.LimitMinusPort.ToString(CultureInfo.InvariantCulture), 243 + i * 5);
                _uc.Setcheckboxstate(true, ax.LimitMinusNegate, 4 + i * 9);

                _uc.Setfieldtext(true, ax.LimitPlusPin.ToString(CultureInfo.InvariantCulture), 4 + i * 15);
                _uc.Setfieldtext(true, ax.LimitPlusPort.ToString(CultureInfo.InvariantCulture), 244 + i * 5);
                _uc.Setcheckboxstate(true, ax.LimitPlusNegate, 5 + i * 9);

                _uc.Setfieldtext(true, ax.HomePin.ToString(CultureInfo.InvariantCulture), 5 + i * 15);
                _uc.Setfieldtext(true, ax.HomePort.ToString(CultureInfo.InvariantCulture), 245 + i * 5);
                _uc.Setcheckboxstate(true, ax.HomeNegate, 6 + i * 9);
                _uc.Setcheckboxstate(true, ax.HomeDirectionPositive, 7 + i * 9);
                _uc.Setcheckboxstate(true, ax.HomeAutoZero, 8 + i * 9);

                _uc.Setfieldtext(true, ax.HomingSpeedUp.ToString("0.###", CultureInfo.InvariantCulture), 6 + i * 15);
                _uc.Setfieldtext(true, ax.HomingSpeedDown.ToString("0.###", CultureInfo.InvariantCulture), 15 + i * 15);
                _uc.Setfieldtext(true, ax.HomeBackoff.ToString("0.###", CultureInfo.InvariantCulture), 2621 + i);

                _uc.Setfieldtext(true, ax.StepsPerUnit.ToString("0.####", CultureInfo.InvariantCulture), 8 + i * 15);
                _uc.Setfieldtext(true, ax.MaxVelocity.ToString("0.###", CultureInfo.InvariantCulture), 9 + i * 15);
                _uc.Setfieldtext(true, ax.Acceleration.ToString("0.###", CultureInfo.InvariantCulture), 10 + i * 15);
                _uc.Setfieldtext(true, ax.SoftLimitNegative.ToString("0.###", CultureInfo.InvariantCulture), 11 + i * 15);
                _uc.Setfieldtext(true, ax.SoftLimitPositive.ToString("0.###", CultureInfo.InvariantCulture), 12 + i * 15);

                if (i >= 3)
                {
                    int rotIdx = i - 3;
                    _uc.Setcheckboxstate(true, ax.IsRotary, 369 + rotIdx);
                    _uc.Setcheckboxstate(true, ax.RotaryRollover, 373 + rotIdx);
                }
            }

            // Update Slave Axis Comboboxes 1 (X), 2 (Y), 3 (Z)
            string[] masterNames = { "X", "Y", "Z" };
            for (int m = 0; m < 3; m++)
            {
                var slave = cfg.Axes.FirstOrDefault(a => a.Role == AxisRole.GantrySlave && a.MasterAxis == masterNames[m]);
                string slaveVal = slave != null ? slave.AxisName : "None";
                SelectComboboxByText(mainForm, m + 1, slaveVal);
            }

            // General settings
            _uc.Setcheckboxstate(true, cfg.EnableSoftLimits, 75);
            _uc.Setcheckboxstate(true, cfg.CheckHomeOnCycleStart, 367);
            _uc.Setcheckboxstate(true, false, 76); // Not ignore M6
            _uc.Setcheckboxstate(true, false, 77); // Not manual M6 wait
            _uc.Setcheckboxstate(true, true, 78);  // Mandatory for Stage 2 ATC: Run M6 macro
            _uc.Setfieldtext(true, cfg.SafeZ.ToString("0.###", CultureInfo.InvariantCulture), 225);

            for (int h = 0; h < 6; h++)
            {
                SelectComboboxByText(mainForm, 4 + h, cfg.HomeSequence[h] ?? "None");
            }

            // IO Setup (E-Stop, Probe 1, Probe 2)
            _uc.Setfieldtext(true, cfg.EstopPin.ToString(CultureInfo.InvariantCulture), 91);
            _uc.Setfieldtext(true, cfg.EstopPort.ToString(CultureInfo.InvariantCulture), 271);
            _uc.Setcheckboxstate(true, cfg.EstopNegate, 55);

            _uc.Setfieldtext(true, cfg.Probe1Pin.ToString(CultureInfo.InvariantCulture), 92);
            _uc.Setfieldtext(true, cfg.Probe1Port.ToString(CultureInfo.InvariantCulture), 272);
            _uc.Setcheckboxstate(true, cfg.Probe1Negate, 56);

            _uc.Setfieldtext(true, (cfg.UseProbe2 ? cfg.Probe2Pin : 0).ToString(CultureInfo.InvariantCulture), 2011);
            _uc.Setfieldtext(true, (cfg.UseProbe2 ? cfg.Probe2Port : 0).ToString(CultureInfo.InvariantCulture), 2012);
            _uc.Setcheckboxstate(true, cfg.Probe2Negate, 225);

            // Spindle Setup
            bool isPwm = cfg.SpindleMode == SpindleControlMode.PwmAndRelay;
            bool isStepDir = cfg.SpindleMode == SpindleControlMode.StepDirAndRelay;
            _uc.Setcheckboxstate(true, isPwm, 61);
            _uc.Setcheckboxstate(true, isStepDir, 62);
            _uc.Setfieldtext(true, (isPwm ? cfg.SpindlePwmPin : 0).ToString(CultureInfo.InvariantCulture), 170);
            _uc.Setfieldtext(true, (isPwm ? cfg.SpindlePwmPort : 0).ToString(CultureInfo.InvariantCulture), 279);
            _uc.Setcheckboxstate(true, cfg.SpindlePwmNegate, 63);
            _uc.Setfieldtext(true, cfg.SpindlePwmFreq.ToString(CultureInfo.InvariantCulture), 172);
            _uc.Setfieldtext(true, cfg.SpindleMinRpm.ToString("0", CultureInfo.InvariantCulture), 177);
            _uc.Setfieldtext(true, cfg.SpindleMaxRpm.ToString("0", CultureInfo.InvariantCulture), 178);

            _uc.Setcheckboxstate(true, cfg.M3RelayPin > 0 || cfg.M4RelayPin > 0, 67);
            _uc.Setfieldtext(true, cfg.M3RelayPin.ToString(CultureInfo.InvariantCulture), 179);
            _uc.Setfieldtext(true, cfg.M3RelayPort.ToString(CultureInfo.InvariantCulture), 283);
            _uc.Setcheckboxstate(true, cfg.M3RelayNegate, 68);
            _uc.Setfieldtext(true, (cfg.UseM4Relay ? cfg.M4RelayPin : 0).ToString(CultureInfo.InvariantCulture), 180);
            _uc.Setfieldtext(true, (cfg.UseM4Relay ? cfg.M4RelayPort : 0).ToString(CultureInfo.InvariantCulture), 284);
            _uc.Setcheckboxstate(true, cfg.M4RelayNegate, 69);

            bool anyCoolant = cfg.UseMistM7 || cfg.UseFloodM8;
            _uc.Setcheckboxstate(true, anyCoolant, 70);
            _uc.Setfieldtext(true, (cfg.UseMistM7 ? cfg.M7MistPin : 0).ToString(CultureInfo.InvariantCulture), 185);
            _uc.Setfieldtext(true, (cfg.UseMistM7 ? cfg.M7MistPort : 0).ToString(CultureInfo.InvariantCulture), 285);
            _uc.Setcheckboxstate(true, cfg.M7MistNegate, 71);
            _uc.Setfieldtext(true, (cfg.UseFloodM8 ? cfg.M8FloodPin : 0).ToString(CultureInfo.InvariantCulture), 186);
            _uc.Setfieldtext(true, (cfg.UseFloodM8 ? cfg.M8FloodPort : 0).ToString(CultureInfo.InvariantCulture), 286);
            _uc.Setcheckboxstate(true, cfg.M8FloodNegate, 72);

            // 2. Also write directly to UCCNC INI store
            WriteAllUccncIniKeys(cfg);

            // 3. Apply live settings in UCCNC kernel (B168) and Save to .pro file (B167)
            try
            {
                _uc.Callbutton(168); // Applysettings
                _uc.Callbutton(167); // Savesettings
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Callbutton(168/167)", ex);
            }
        }

        private void WriteAllUccncIniKeys(MachineConfig cfg)
        {
            foreach (var ax in cfg.Axes)
            {
                string sec = $"axessettingscontrol{ax.AxisName}";
                string slaveVal = "None";
                if (ax.AxisName == "X" || ax.AxisName == "Y" || ax.AxisName == "Z")
                {
                    var slave = cfg.Axes.FirstOrDefault(a => a.Role == AxisRole.GantrySlave && a.MasterAxis == ax.AxisName);
                    if (slave != null) slaveVal = slave.AxisName;
                }

                WriteIni(sec, "Axisenabled", ax.Enabled.ToString());
                WriteIni(sec, "Stepportnumber", ax.StepPort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Steppinnumber", ax.StepPin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Steppinnegate", ax.StepNegate.ToString());
                WriteIni(sec, "Dirportnumber", ax.DirPort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Dirpinnumber", ax.DirPin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Dirpinnegate", ax.DirNegate.ToString());
                WriteIni(sec, "Enaportnumber", ax.EnaPort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Enapinnumber", ax.EnaPin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Enapinnegate", ax.EnaNegate.ToString());
                WriteIni(sec, "Homeportnumber", ax.HomePort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Homepinnumber", ax.HomePin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Homepinnegate", ax.HomeNegate.ToString());
                WriteIni(sec, "Limitminusportnumber", ax.LimitMinusPort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Limitminuspinnumber", ax.LimitMinusPin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Limitminuspinnegate", ax.LimitMinusNegate.ToString());
                WriteIni(sec, "Limitplusportnumber", ax.LimitPlusPort.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Limitpluspinnumber", ax.LimitPlusPin.ToString(CultureInfo.InvariantCulture));
                WriteIni(sec, "Limitpluspinnegate", ax.LimitPlusNegate.ToString());
                WriteIni(sec, "Stepsperunit", ax.StepsPerUnit.ToString("0.####", CultureInfo.InvariantCulture));
                WriteIni(sec, "Velocity", ax.MaxVelocity.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Acceleration", ax.Acceleration.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Softlimitnegative", ax.SoftLimitNegative.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Softlimitpositive", ax.SoftLimitPositive.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Homedirectionpositive", ax.HomeDirectionPositive.ToString());
                WriteIni(sec, "Homeautozero", ax.HomeAutoZero.ToString());
                WriteIni(sec, "Homingspeedup", ax.HomingSpeedUp.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Homingspeeddown", ax.HomingSpeedDown.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Homebackoff", ax.HomeBackoff.ToString("0.###", CultureInfo.InvariantCulture));
                WriteIni(sec, "Slaveaxis", slaveVal);
                WriteIni(sec, "Rotaryaxis", ax.IsRotary.ToString());
                WriteIni(sec, "Rotaryrollover", ax.RotaryRollover.ToString());
            }

            WriteIni("Generalsettings", "Softlimitsenable", cfg.EnableSoftLimits.ToString());
            WriteIni("Generalsettings", "CheckHomeonCycleStart", cfg.CheckHomeOnCycleStart.ToString());
            WriteIni("Generalsettings", "Doontoolchange", "2");
            WriteIni("Generalsettings", "SafeZ", cfg.SafeZ.ToString("0.###", CultureInfo.InvariantCulture));
            for (int i = 0; i < 6; i++)
            {
                WriteIni("Generalsettings", $"Homesequence{i + 1}", cfg.HomeSequence[i] ?? "None");
            }

            WriteIni("IOsetupsettings", "Estopportnumber", cfg.EstopPort.ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Estoppinnumber", cfg.EstopPin.ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Estoppinnegate", cfg.EstopNegate.ToString());
            WriteIni("IOsetupsettings", "Probeportnumber", cfg.Probe1Port.ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Probepinnumber", cfg.Probe1Pin.ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Probepinnegate", cfg.Probe1Negate.ToString());
            WriteIni("IOsetupsettings", "Probeport2number", (cfg.UseProbe2 ? cfg.Probe2Port : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Probepin2number", (cfg.UseProbe2 ? cfg.Probe2Pin : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("IOsetupsettings", "Probepin2negate", cfg.Probe2Negate.ToString());

            WriteIni("Spindlesetupsettings", "IsSpindleinPWMMode", (cfg.SpindleMode == SpindleControlMode.PwmAndRelay).ToString());
            WriteIni("Spindlesetupsettings", "SpindlePWMoutport", cfg.SpindlePwmPort.ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "SpindlePWMoutpin", cfg.SpindlePwmPin.ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "SpindlePWMoutpinnegate", cfg.SpindlePwmNegate.ToString());
            WriteIni("Spindlesetupsettings", "SpindlePWMfrequency", cfg.SpindlePwmFreq.ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "Spindleminvelocity", cfg.SpindleMinRpm.ToString("0", CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "Spindlemaxvelocity", cfg.SpindleMaxRpm.ToString("0", CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M3relayport", cfg.M3RelayPort.ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M3relaypin", cfg.M3RelayPin.ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M3relaypinnegate", cfg.M3RelayNegate.ToString());
            WriteIni("Spindlesetupsettings", "M4relayport", (cfg.UseM4Relay ? cfg.M4RelayPort : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M4relaypin", (cfg.UseM4Relay ? cfg.M4RelayPin : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M4relaypinnegate", cfg.M4RelayNegate.ToString());
            WriteIni("Spindlesetupsettings", "M7relayport", (cfg.UseMistM7 ? cfg.M7MistPort : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M7relaypin", (cfg.UseMistM7 ? cfg.M7MistPin : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M7relaypinnegate", cfg.M7MistNegate.ToString());
            WriteIni("Spindlesetupsettings", "M8relayport", (cfg.UseFloodM8 ? cfg.M8FloodPort : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M8relaypin", (cfg.UseFloodM8 ? cfg.M8FloodPin : 0).ToString(CultureInfo.InvariantCulture));
            WriteIni("Spindlesetupsettings", "M8relaypinnegate", cfg.M8FloodNegate.ToString());
        }

        private void SaveJsonAndHtmProKeys(MachineConfig cfg)
        {
            try
            {
                if (!Directory.Exists(_profilesDir))
                {
                    Directory.CreateDirectory(_profilesDir);
                }

                var ser = new JavaScriptSerializer();
                File.WriteAllText(_jsonConfigPath, ser.Serialize(cfg));

                WriteIni("HTM_PRO", "Stage1Completed", cfg.Stage1Completed.ToString());
                WriteIni("HTM_PRO", "ControllerType", cfg.ControllerType);
                WriteIni("HTM_PRO", "IsImperial", cfg.IsImperial.ToString());
                WriteIni("HTM_PRO", "EnableStrategy", cfg.EnableStrategy.ToString());
                WriteIni("HTM_PRO", "LimitStrategy", cfg.LimitStrategy.ToString());
                WriteIni("HTM_PRO", "ReserveResourcesForAtc", cfg.ReserveResourcesForAtc.ToString());
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("SaveJsonAndHtmProKeys", ex);
            }
        }

        /// <summary>
        /// Generates a human-readable wiring cheat sheet and free-pin readiness report alongside the profile.
        /// </summary>
        public string ExportPinoutSummary(MachineConfig cfg)
        {
            var spec = HardwareCatalog.GetSpec(HardwareCatalog.ParseModel(cfg.ControllerType));
            var report = PinAllocator.ValidateAndAudit(cfg, spec);
            string unit = cfg.IsImperial ? "in" : "mm";

            var sb = new StringBuilder();
            sb.AppendLine($"====================================================================");
            sb.AppendLine($"  {PluginVersion.FullTitle} - Machine Configuration & Pinout Summary");
            sb.AppendLine($"  Profile: {_profileName}   Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"  Controller: {spec.DisplayName}   Units: {(cfg.IsImperial ? "Imperial (inch)" : "Metric (mm)")}");
            sb.AppendLine($"====================================================================");
            sb.AppendLine();

            sb.AppendLine("[ACTIVE AXES & TRAVEL]");
            foreach (var ax in cfg.Axes.Where(a => a.Enabled))
            {
                string roleNote = ax.Role == AxisRole.GantrySlave ? $"Slave of {ax.MasterAxis}" : ax.Role.ToString();
                sb.AppendLine($"  {ax.AxisName}: Role={roleNote,-18} Steps/u={ax.StepsPerUnit,-8:0.###} Vel={ax.MaxVelocity,-7:0.##} SoftLimits=[{ax.SoftLimitNegative:0.##} .. {ax.SoftLimitPositive:0.##} {unit}]");
            }
            sb.AppendLine();

            sb.AppendLine($"[OUTPUT PINS ASSIGNED ({report.UsedOutputCount}/{spec.TotalOutputs})]");
            foreach (var o in report.AssignedOutputs.OrderBy(x => x.Port).ThenBy(x => x.Pin))
            {
                string inv = o.Negate ? " [Active Low]" : "";
                sb.AppendLine($"  Port {o.Port} Pin {o.Pin,-2}  -> {o.FunctionName}{inv}");
            }
            sb.AppendLine();

            sb.AppendLine($"[INPUT PINS ASSIGNED ({report.UsedInputCount}/{spec.TotalInputs})]");
            foreach (var inp in report.AssignedInputs.OrderBy(x => x.Port).ThenBy(x => x.Pin))
            {
                string inv = inp.Negate ? " [Active Low]" : "";
                sb.AppendLine($"  Port {inp.Port} Pin {inp.Pin,-2}  <- {inp.FunctionName}{inv}");
            }
            sb.AppendLine();

            sb.AppendLine($"[FREE OUTPUTS FOR STAGE 2 ATC ({report.FreeOutputCount})]");
            sb.AppendLine("  " + (report.FreeOutputs.Count > 0 ? string.Join(", ", report.FreeOutputs.Select(p => $"P{p.Port}-{p.Pin}")) : "None"));
            sb.AppendLine();

            sb.AppendLine($"[FREE INPUTS FOR STAGE 2 ATC ({report.FreeInputCount})]");
            sb.AppendLine("  " + (report.FreeInputs.Count > 0 ? string.Join(", ", report.FreeInputs.Select(p => $"P{p.Port}-{p.Pin}")) : "None"));
            sb.AppendLine();

            sb.AppendLine("[STAGE 2 ATC READINESS]");
            sb.AppendLine(report.GetAtcReadinessSummary());

            if (report.Conflicts.Count > 0 || report.InvalidPins.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("[ERRORS / CONFLICTS]");
                foreach (var err in report.InvalidPins.Concat(report.Conflicts))
                {
                    sb.AppendLine("  ! " + err);
                }
            }

            string content = sb.ToString();
            try
            {
                File.WriteAllText(_pinoutSummaryPath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("ExportPinoutSummary", ex);
            }
            return content;
        }

        /// <summary>
        /// Updates a UCCNC GUI combobox by item display string via AS3/mainGUI reflection, falling back to Updatecomboboxselection.
        /// </summary>
        private void SelectComboboxByText(Form mainForm, int labelNumber, string targetText)
        {
            if (_uc == null) return;
            try
            {
                int foundIdx = -1;
                if (mainForm != null)
                {
                    FieldInfo as3Field = mainForm.GetType().GetField("AS3", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    object as3Obj = as3Field?.GetValue(mainForm);
                    FieldInfo mainGuiField = as3Obj?.GetType().GetField("mainGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    object mainGuiObj = mainGuiField?.GetValue(as3Obj);
                    FieldInfo combosField = mainGuiObj?.GetType().GetField("GUIcomboboxes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (combosField?.GetValue(mainGuiObj) is IList combos)
                    {
                        foreach (object cb in combos)
                        {
                            if (cb == null) continue;
                            Type ct = cb.GetType();
                            int ln = (int)(ct.GetField("labelnumber")?.GetValue(cb) ?? -1);
                            if (ln != labelNumber) continue;

                            if (ct.GetField("itemsname")?.GetValue(cb) is IList itemsName)
                            {
                                for (int idx = 0; idx < itemsName.Count; idx++)
                                {
                                    if (string.Equals(itemsName[idx]?.ToString(), targetText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        foundIdx = idx;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                if (foundIdx >= 0)
                {
                    _uc.Updatecomboboxselection(true, foundIdx, labelNumber);
                }
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"SelectComboboxByText({labelNumber}, {targetText}): {ex.Message}");
            }
        }

        private string ReadIni(string section, string key, string def)
        {
            try
            {
                string val = _uc?.Readkey(section, key, def);
                return string.IsNullOrEmpty(val) ? def : val;
            }
            catch { return def; }
        }

        private int ReadIniInt(string section, string key, int def)
        {
            string val = ReadIni(section, key, "");
            return int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) ? r : def;
        }

        private double ReadIniDouble(string section, string key, double def)
        {
            string val = ReadIni(section, key, "").Replace(',', '.');
            return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double r) ? r : def;
        }

        private bool ReadIniBool(string section, string key, bool def)
        {
            string val = ReadIni(section, key, "");
            if (string.IsNullOrEmpty(val)) return def;
            if (val == "1") return true;
            if (val == "0") return false;
            return bool.TryParse(val, out bool r) ? r : def;
        }

        private void WriteIni(string section, string key, string value)
        {
            try { _uc?.Writekey(section, key, value); } catch { }
        }
    }
}
