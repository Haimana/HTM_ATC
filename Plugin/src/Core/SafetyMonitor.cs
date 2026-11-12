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

        private readonly Dictionary<int, bool> _previousOssdStates = new Dictionary<int, bool>();
        private bool _previousPressureState = true;

        public event Action<SafetyFault> OnFaultDetected;
        public event Action<SafetyFault> OnFaultCleared;

        public List<SafetyFault> ActiveFaults { get; } = new List<SafetyFault>();

        private static readonly Dictionary<int, string> OssdMessages = new Dictionary<int, string>
        {
            { 3, "E-Stop Button Activated" },
            { 4, "X-Axis Driver Fault" },
            { 5, "Z-Axis Driver Fault" },
            { 6, "C-Axis Driver Fault" },
            { 7, "Power Supply Fault" }
        };

        private static readonly Dictionary<int, string> OssdClearMessages = new Dictionary<int, string>
        {
            { 3, "E-Stop Button De-Activated" },
            { 4, "X-Axis Driver Online" },
            { 5, "Z-Axis Driver Online" },
            { 6, "C-Axis Driver Online" },
            { 7, "Power Supply Restored" }
        };

        public SafetyMonitor(ATCConfig config, Plugininterface.Entry uc, Action<string> log, Func<bool> isSimulation)
        {
            _config = config;
            _uc = uc;
            _log = log ?? (s => { });
            _isSimulation = isSimulation ?? (() => false);

            foreach (var ledNum in OssdMessages.Keys)
            {
                _previousOssdStates[ledNum] = false;
            }
        }

        public void CheckSafety()
        {
            if (_isSimulation())
            {
                if (!_config.Simulation.OssdOK)
                {
                    ReportFault("SIM_OSSD", "Simulated OSSD fault");
                }
                if (_config.IO.PressureSensor.Enabled && !_config.Simulation.PressureOK)
                {
                    ReportFault("SIM_PRESSURE", "Simulated low air pressure");
                }
                return;
            }

            CheckOssdInputs();
            CheckAirPressure();
        }

        private void CheckOssdInputs()
        {
            foreach (var kvp in OssdMessages)
            {
                int ledNum = kvp.Key;
                bool currentState = GetLED(ledNum);
                bool previousState = _previousOssdStates[ledNum];

                if (currentState != previousState)
                {
                    _previousOssdStates[ledNum] = currentState;

                    if (currentState)
                    {
                        string faultCode = $"OSSD_{ledNum}";
                        string message = kvp.Value;
                        ReportFault(faultCode, message);

                        _log($"SAFETY FAULT: {message}");
                        StopMachine();
                    }
                    else
                    {
                        string faultCode = $"OSSD_{ledNum}";
                        ClearFault(faultCode);
                        
                        if (OssdClearMessages.TryGetValue(ledNum, out string clearMsg))
                        {
                            _log(clearMsg);
                        }
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
            if (_isSimulation()) return true;
            int ledNum = 56 + axis;
            return GetLED(ledNum);
        }

        public bool AreAllAxesHomed()
        {
            return IsAxisHomed(0) && IsAxisHomed(1) && IsAxisHomed(2);
        }
    }
}
