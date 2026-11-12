using System;
using System.Collections.Generic;

namespace Plugins.Core
{
    public class SafetyFault
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; }
    }

    public class SafetyMonitor
    {
        private readonly ATCConfig _config;
        private readonly Plugininterface.Entry _uc;
        private readonly Action<string> _log;
        private readonly Func<bool> _isSimulation;

        private readonly Dictionary<string, bool> _previousOssdStates = new Dictionary<string, bool>();
        private bool _previousPressureState = true;

        public event Action<SafetyFault> OnFaultDetected;
        public event Action<SafetyFault> OnFaultCleared;

        public List<SafetyFault> ActiveFaults { get; } = new List<SafetyFault>();

        public SafetyMonitor(ATCConfig config, Plugininterface.Entry uc, Action<string> log, Func<bool> isSimulation)
        {
            _config = config;
            _uc = uc;
            _log = log ?? (s => { });
            _isSimulation = isSimulation ?? (() => false);

            InitializeOssdStates();
        }

        private void InitializeOssdStates()
        {
            _previousOssdStates.Clear();
            if (_config.Safety?.OssdInputs != null)
            {
                foreach (var input in _config.Safety.OssdInputs)
                {
                    if (input.Enabled)
                    {
                        _previousOssdStates[input.Name] = false;
                    }
                }
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

        public void CheckSafety()
        {
            if (!_config.Safety.Enabled)
                return;

            bool simMode = _isSimulation();
            bool demoMode = IsDemoMode();

            if (simMode || demoMode)
            {
                return;
            }

            CheckOssdInputs();
            CheckAirPressure();
        }

        private void CheckOssdInputs()
        {
            if (_config.Safety?.OssdInputs == null)
                return;

            foreach (var input in _config.Safety.OssdInputs)
            {
                if (!input.Enabled)
                    continue;

                bool rawState = GetLED(input.LedNumber);
                bool faultActive = input.ActiveLow ? rawState : !rawState;

                string key = input.Name;
                bool previousFault = false;
                _previousOssdStates.TryGetValue(key, out previousFault);

                if (faultActive != previousFault)
                {
                    _previousOssdStates[key] = faultActive;

                    string faultCode = $"OSSD_{input.LedNumber}";
                    if (faultActive)
                    {
                        string message = $"{input.Name} Fault";
                        ReportFault(faultCode, message);
                        _log($"SAFETY FAULT: {message}");
                        StopMachine();
                    }
                    else
                    {
                        ClearFault(faultCode);
                        _log($"{input.Name} - OK");
                    }
                }
            }
        }

        private void CheckAirPressure()
        {
            if (!_config.IO.PressureSensor.Enabled)
                return;

            int ledNum = GetLEDForPin(_config.IO.PressureSensor.Port, _config.IO.PressureSensor.Pin);
            bool sensorTriggered = GetLED(ledNum);
            bool pressureLow = sensorTriggered;

            if (pressureLow != !_previousPressureState)
            {
                _previousPressureState = !pressureLow;

                if (pressureLow)
                {
                    ReportFault("AIR_PRESSURE", "Low Air Pressure Detected");
                    _log("SAFETY WARNING: Low air pressure");
                }
                else
                {
                    ClearFault("AIR_PRESSURE");
                    _log("Air pressure restored");
                }
            }
        }

        private void ReportFault(string code, string message)
        {
            var existing = ActiveFaults.Find(f => f.Code == code);
            if (existing != null)
            {
                existing.IsActive = true;
                existing.Timestamp = DateTime.Now;
                return;
            }

            var fault = new SafetyFault
            {
                Code = code,
                Message = message,
                Timestamp = DateTime.Now,
                IsActive = true
            };

            ActiveFaults.Add(fault);
            OnFaultDetected?.Invoke(fault);
        }

        private void ClearFault(string code)
        {
            var fault = ActiveFaults.Find(f => f.Code == code);
            if (fault != null)
            {
                fault.IsActive = false;
                ActiveFaults.Remove(fault);
                OnFaultCleared?.Invoke(fault);
            }
        }

        private void StopMachine()
        {
            if (_isSimulation() || IsDemoMode())
            {
                PluginLogger.Log("Demo/Simulation: exec.Stop() skipped for safety fault");
                return;
            }
            
            try
            {
                _uc.Stop();
            }
            catch { }
        }

        private bool GetLED(int ledNum)
        {
            try
            {
                return _uc.GetLED(ledNum);
            }
            catch
            {
                return false;
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

        public bool HasActiveFaults()
        {
            return ActiveFaults.Count > 0;
        }

        public bool IsAxisHomed(int axis)
        {
            if (_isSimulation() || IsDemoMode()) return true;
            int ledNum = 56 + axis;
            return GetLED(ledNum);
        }

        public bool AreAllAxesHomed()
        {
            return IsAxisHomed(0) && IsAxisHomed(1) && IsAxisHomed(2);
        }

        public bool CheckSpindleToolPresent()
        {
            if (!_config.IO.SpindleToolSensor.Enabled)
                return true;

            if (_isSimulation() || IsDemoMode())
                return true;

            int ledNum = GetLEDForPin(_config.IO.SpindleToolSensor.Port, _config.IO.SpindleToolSensor.Pin);
            bool rawState = GetLED(ledNum);
            return _config.IO.SpindleToolSensor.ActiveLow ? !rawState : rawState;
        }
    }
}
