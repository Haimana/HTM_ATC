using System;
using Plugins.Core;

namespace Plugins.Simulation
{
    public class SimulatedIO
    {
        private readonly ATCConfig _config;
        private readonly bool[] _outputStates = new bool[100];
        
        public bool DrawbarOpen { get; private set; }
        public bool AirBlowOn { get; private set; }
        
        public SimulatedIO(ATCConfig config)
        {
            _config = config;
        }

        public bool IsSimulationEnabled => _config.Simulation.Enabled;

        public bool GetSlotSensorState(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > 10)
                return false;
            return _config.Simulation.SlotStates[slotNumber - 1];
        }

        public void SetSlotSensorState(int slotNumber, bool hasTool)
        {
            if (slotNumber >= 1 && slotNumber <= 10)
            {
                _config.Simulation.SlotStates[slotNumber - 1] = hasTool;
            }
        }

        public bool GetPressureSensorOK()
        {
            return _config.Simulation.PressureOK;
        }

        public void SetPressureSensorOK(bool ok)
        {
            _config.Simulation.PressureOK = ok;
        }

        public bool GetOssdOK()
        {
            return _config.Simulation.OssdOK;
        }

        public void SetOssdOK(bool ok)
        {
            _config.Simulation.OssdOK = ok;
        }

        public void SetOutput(int port, int pin, bool state)
        {
            int index = (port - 1) * 17 + pin;
            if (index >= 0 && index < _outputStates.Length)
            {
                _outputStates[index] = state;
            }

            if (port == _config.IO.Drawbar.Port && pin == _config.IO.Drawbar.Pin)
            {
                DrawbarOpen = state;
            }
            else if (port == _config.IO.AirBlow.Port && pin == _config.IO.AirBlow.Pin)
            {
                AirBlowOn = state;
            }
        }

        public bool GetOutput(int port, int pin)
        {
            int index = (port - 1) * 17 + pin;
            if (index >= 0 && index < _outputStates.Length)
            {
                return _outputStates[index];
            }
            return false;
        }

        public void Reset()
        {
            Array.Clear(_outputStates, 0, _outputStates.Length);
            DrawbarOpen = false;
            AirBlowOn = false;
        }

        public void SimulateToolPickup(int slotNumber)
        {
            if (slotNumber >= 1 && slotNumber <= 10)
            {
                _config.Simulation.SlotStates[slotNumber - 1] = false;
            }
        }

        public void SimulateToolDrop(int slotNumber)
        {
            if (slotNumber >= 1 && slotNumber <= 10)
            {
                _config.Simulation.SlotStates[slotNumber - 1] = true;
            }
        }
    }
}
