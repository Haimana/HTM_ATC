using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plugins.Core
{
    public class ATCConfig
    {
        public int Version { get; set; } = 2;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public RackConfig Rack { get; set; } = new RackConfig();
        public HeightsConfig Heights { get; set; } = new HeightsConfig();
        public FeedsAndTimingConfig FeedsAndTiming { get; set; } = new FeedsAndTimingConfig();
        public TCPConfig TCP { get; set; } = new TCPConfig();
        public IOConfig IO { get; set; } = new IOConfig();
        public SafetyConfig Safety { get; set; } = new SafetyConfig();
        public ProbeConfig Probe { get; set; } = new ProbeConfig();
        public List<SlotConfig> Slots { get; set; } = new List<SlotConfig>();
        public OptionsConfig Options { get; set; } = new OptionsConfig();
        public SimulationConfig Simulation { get; set; } = new SimulationConfig();

        public ATCConfig()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            Slots.Clear();
            for (int i = 1; i <= 10; i++)
            {
                Slots.Add(new SlotConfig { Index = i, Enabled = true, ToolNumber = 0 });
            }
        }

        public SlotConfig GetSlot(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > Slots.Count)
                return null;
            return Slots[slotNumber - 1];
        }

        public double GetSlotPosition(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > 10)
                return 0;
            return Rack.Slot1Position + (slotNumber - 1) * Rack.Pitch;
        }

        public int FindToolSlot(int toolNumber)
        {
            if (toolNumber <= 0) return 0;
            foreach (var slot in Slots)
            {
                if (slot.Enabled && slot.ToolNumber == toolNumber)
                    return slot.Index;
            }
            return 0;
        }

        public int FindFirstEmptySlot()
        {
            foreach (var slot in Slots)
            {
                if (slot.Enabled && slot.ToolNumber == 0)
                    return slot.Index;
            }
            return 0;
        }

        public static ATCConfig CreateDefault()
        {
            return new ATCConfig
            {
                Version = 2,
                LastModified = DateTime.UtcNow,
                Rack = new RackConfig
                {
                    Axis = "X",
                    SlotCount = 10,
                    Slot1Position = 50.0,
                    Slot1PositionSecondary = -120.0,
                    Pitch = 80.0
                },
                Heights = new HeightsConfig
                {
                    ZSafe = 0.0,
                    ZEmpty = -20.0,
                    ZApproach = -80.0,
                    ZClamp = -145.0,
                    ZCheck = -140.0
                },
                FeedsAndTiming = new FeedsAndTimingConfig
                {
                    FeedRate = 2000.0,
                    OperationDelay = 300,
                    SensorWaitTime = 5000
                },
                TCP = new TCPConfig
                {
                    X = 100.0,
                    Y = 50.0,
                    Z = 0.0
                },
                IO = new IOConfig
                {
                    Drawbar = new PinConfig { Port = 1, Pin = 14 },
                    AirBlow = new OptionalPinConfig { Enabled = true, Port = 1, Pin = 15 },
                    PressureSensor = new OptionalPinConfig { Enabled = true, Port = 1, Pin = 10 },
                    DustHood = new DustHoodConfig { Enabled = false, Port = 1, Pin = 16, RaiseBeforeTC = true, LowerAfterTC = true },
                    SpindleToolSensor = new ActiveLevelPinConfig { Enabled = false, Port = 1, Pin = 17, ActiveLow = true },
                    SlotSensors = new SlotSensorsConfig
                    {
                        Enabled = true,
                        Port = 5,
                        Pins = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                    }
                },
                Safety = SafetyConfig.CreateDefault(),
                Probe = ProbeConfig.CreateDefault(),
                Options = new OptionsConfig
                {
                    ApplyG43 = true,
                    ReturnToXY = true,
                    MeasureNonRH = false,
                    ShowATCPage = false,
                    AirBlowDuringTC = true
                },
                Simulation = new SimulationConfig
                {
                    Enabled = false,
                    SlotStates = new bool[] { false, false, false, false, false, false, false, false, false, false },
                    PressureOK = true,
                    OssdOK = true
                }
            };
        }
    }

    public class RackConfig
    {
        public string Axis { get; set; } = "X";
        public string AxisSecondary => Axis == "X" ? "Y" : "X";
        public int SlotCount { get; set; } = 10;
        public double Slot1Position { get; set; }
        public double Slot1PositionSecondary { get; set; }
        public double Pitch { get; set; } = 80.0;
    }

    public class HeightsConfig
    {
        public double ZSafe { get; set; }
        public double ZEmpty { get; set; }
        public double ZApproach { get; set; }
        public double ZClamp { get; set; }
        public double ZCheck { get; set; }
    }

    public class FeedsAndTimingConfig
    {
        public double FeedRate { get; set; } = 2000.0;
        public int OperationDelay { get; set; } = 300;
        public int SensorWaitTime { get; set; } = 5000;
    }

    public class TCPConfig
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class IOConfig
    {
        public PinConfig Drawbar { get; set; } = new PinConfig();
        public OptionalPinConfig AirBlow { get; set; } = new OptionalPinConfig();
        public OptionalPinConfig PressureSensor { get; set; } = new OptionalPinConfig();
        public DustHoodConfig DustHood { get; set; } = new DustHoodConfig();
        public ActiveLevelPinConfig SpindleToolSensor { get; set; } = new ActiveLevelPinConfig();
        public SlotSensorsConfig SlotSensors { get; set; } = new SlotSensorsConfig();
    }

    public class PinConfig
    {
        public int Port { get; set; } = 1;
        public int Pin { get; set; } = 1;
    }

    public class OptionalPinConfig : PinConfig
    {
        public bool Enabled { get; set; }
    }

    public class ActiveLevelPinConfig : OptionalPinConfig
    {
        public bool ActiveLow { get; set; } = true;
    }

    public class DustHoodConfig : OptionalPinConfig
    {
        public bool RaiseBeforeTC { get; set; } = true;
        public bool LowerAfterTC { get; set; } = true;
    }

    public class SafetyConfig
    {
        public bool Enabled { get; set; } = true;
        public List<OssdInputConfig> OssdInputs { get; set; } = new List<OssdInputConfig>();

        public static SafetyConfig CreateDefault()
        {
            return new SafetyConfig
            {
                Enabled = true,
                OssdInputs = new List<OssdInputConfig>
                {
                    new OssdInputConfig { Name = "E-Stop", LedNumber = 3, ActiveLow = true, Enabled = true },
                    new OssdInputConfig { Name = "X-Axis Driver", LedNumber = 4, ActiveLow = true, Enabled = true },
                    new OssdInputConfig { Name = "Z-Axis Driver", LedNumber = 5, ActiveLow = true, Enabled = true },
                    new OssdInputConfig { Name = "C-Axis Driver", LedNumber = 6, ActiveLow = true, Enabled = true },
                    new OssdInputConfig { Name = "Power Supply", LedNumber = 7, ActiveLow = true, Enabled = true }
                }
            };
        }
    }

    public class OssdInputConfig
    {
        public string Name { get; set; } = "";
        public int LedNumber { get; set; }
        public bool ActiveLow { get; set; } = true;
        public bool Enabled { get; set; } = true;
    }

    public class ProbeConfig
    {
        public bool UseUCCNCSettings { get; set; } = true;
        public double FastFeed { get; set; } = 500.0;
        public double SlowFeed { get; set; } = 50.0;
        public double Retract { get; set; } = 2.0;
        public bool DualCycle { get; set; } = true;
        public double MinZ { get; set; } = -200.0;

        public static ProbeConfig CreateDefault()
        {
            return new ProbeConfig
            {
                UseUCCNCSettings = true,
                FastFeed = 500.0,
                SlowFeed = 50.0,
                Retract = 2.0,
                DualCycle = true,
                MinZ = -200.0
            };
        }
    }

    public class SlotSensorsConfig
    {
        public bool Enabled { get; set; }
        public int Port { get; set; } = 5;
        public int[] Pins { get; set; } = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        public int GetPinForSlot(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > Pins.Length)
                return 0;
            return Pins[slotNumber - 1];
        }
    }

    public class SlotConfig
    {
        public int Index { get; set; }
        public bool Enabled { get; set; } = true;
        public int ToolNumber { get; set; }
    }

    public class OptionsConfig
    {
        public bool ApplyG43 { get; set; } = true;
        public bool ReturnToXY { get; set; } = true;
        public bool MeasureNonRH { get; set; }
        public bool ShowATCPage { get; set; }
        public bool AirBlowDuringTC { get; set; } = true;
    }

    public class SimulationConfig
    {
        public bool Enabled { get; set; }
        public bool[] SlotStates { get; set; } = new bool[10];
        public bool PressureOK { get; set; } = true;
        public bool OssdOK { get; set; } = true;
    }

    public static class HTMProfileImporter
    {
        public static ATCConfig ImportFromProfile(string profilePath)
        {
            var config = ATCConfig.CreateDefault();
            var lines = System.IO.File.ReadAllLines(profilePath);
            string currentSection = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    continue;
                }

                if (!trimmed.Contains("=")) continue;
                var parts = trimmed.Split(new[] { '=' }, 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (currentSection == "UserTextfields")
                {
                    ImportUserTextField(config, key, value);
                }
                else if (currentSection == "UserCheckboxes")
                {
                    ImportUserCheckbox(config, key, value);
                }
            }

            return config;
        }

        private static void ImportUserTextField(ATCConfig config, string key, string value)
        {
            if (!int.TryParse(key, out int fieldId)) return;

            switch (fieldId)
            {
                case 30000: config.Rack.Axis = value; break;
                case 30010: config.Rack.Slot1Position = ParseDouble(value); break;
                case 30011: config.Rack.Slot1PositionSecondary = ParseDouble(value); break;
                case 30012: config.Heights.ZClamp = ParseDouble(value); break;
                case 30013: config.Rack.Pitch = ParseDouble(value); break;
                case 30014: config.Heights.ZApproach = ParseDouble(value); break;
                case 30015: config.Heights.ZSafe = ParseDouble(value); break;
                case 30016: config.Heights.ZEmpty = ParseDouble(value); break;
                case 30017: config.FeedsAndTiming.FeedRate = ParseDouble(value); break;
                case 30018: config.FeedsAndTiming.OperationDelay = (int)ParseDouble(value); break;
                case 30019: config.FeedsAndTiming.SensorWaitTime = (int)ParseDouble(value); break;
                case 30020: config.TCP.X = ParseDouble(value); break;
                case 30021: config.TCP.Y = ParseDouble(value); break;
                case 30022: config.TCP.Z = ParseDouble(value); break;
                case 30023: config.Heights.ZCheck = ParseDouble(value); break;
                case 20650: config.IO.Drawbar.Port = (int)ParseDouble(value); break;
                case 20651: config.IO.Drawbar.Pin = (int)ParseDouble(value); break;
                case 20652: config.IO.AirBlow.Port = (int)ParseDouble(value); break;
                case 20653: config.IO.AirBlow.Pin = (int)ParseDouble(value); break;
                case 20654: config.IO.PressureSensor.Port = (int)ParseDouble(value); break;
                case 20655: config.IO.PressureSensor.Pin = (int)ParseDouble(value); break;
            }

            if (fieldId >= 20601 && fieldId <= 20610)
            {
                int slotIndex = fieldId - 20601;
                if (slotIndex < config.Slots.Count)
                {
                    config.Slots[slotIndex].Enabled = value == "1";
                }
            }

            if (fieldId >= 20501 && fieldId <= 20510)
            {
                int slotIndex = fieldId - 20501;
                if (slotIndex < config.Slots.Count)
                {
                    config.Slots[slotIndex].ToolNumber = (int)ParseDouble(value);
                }
            }
        }

        private static void ImportUserCheckbox(ATCConfig config, string key, string value)
        {
            if (!int.TryParse(key, out int fieldId)) return;
            bool isChecked = value.ToLowerInvariant() == "true";

            switch (fieldId)
            {
                case 30003: config.Options.MeasureNonRH = isChecked; break;
                case 30004: config.Options.ReturnToXY = isChecked; break;
                case 30005: config.Options.ApplyG43 = isChecked; break;
                case 30006: config.Options.ShowATCPage = isChecked; break;
            }
        }

        private static double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;
            if (double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return 0;
        }
    }
}
