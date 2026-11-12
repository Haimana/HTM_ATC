using System;
using System.IO;
using System.Globalization;
using System.Text;
using Plugins.Core;

namespace Plugins.Persistence
{
    public class ConfigStore
    {
        private readonly string _configPath;
        private ATCConfig _config;
        private readonly object _lock = new object();

        public ATCConfig Config
        {
            get
            {
                lock (_lock)
                {
                    return _config;
                }
            }
        }

        public bool ConfigExists => File.Exists(_configPath);

        public ConfigStore(string uccncPath, string profileName)
        {
            string profilesFolder = Path.Combine(uccncPath, "Profiles");
            if (!Directory.Exists(profilesFolder))
            {
                Directory.CreateDirectory(profilesFolder);
            }
            
            _configPath = Path.Combine(profilesFolder, $"HTM_ATC_Config_{profileName}.json");
            _config = ATCConfig.CreateDefault();
        }

        public ConfigStore(string configPath)
        {
            _configPath = configPath;
            _config = ATCConfig.CreateDefault();
        }

        public bool Load()
        {
            lock (_lock)
            {
                if (!File.Exists(_configPath))
                {
                    return false;
                }

                try
                {
                    string json = File.ReadAllText(_configPath, Encoding.UTF8);
                    _config = DeserializeConfig(json);
                    return true;
                }
                catch (Exception)
                {
                    _config = ATCConfig.CreateDefault();
                    return false;
                }
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    _config.LastModified = DateTime.UtcNow;
                    string json = SerializeConfig(_config);
                    
                    string directory = Path.GetDirectoryName(_configPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    File.WriteAllText(_configPath, json, Encoding.UTF8);
                }
                catch (Exception)
                {
                }
            }
        }

        public void UpdateConfig(Action<ATCConfig> updateAction)
        {
            lock (_lock)
            {
                updateAction(_config);
                Save();
            }
        }

        private string SerializeConfig(ATCConfig config)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"version\": {config.Version},");
            sb.AppendLine($"  \"lastModified\": \"{config.LastModified:O}\",");
            
            sb.AppendLine("  \"rack\": {");
            sb.AppendLine($"    \"axis\": \"{config.Rack.Axis}\",");
            sb.AppendLine($"    \"slotCount\": {config.Rack.SlotCount},");
            sb.AppendLine($"    \"slot1Position\": {F(config.Rack.Slot1Position)},");
            sb.AppendLine($"    \"slot1PositionSecondary\": {F(config.Rack.Slot1PositionSecondary)},");
            sb.AppendLine($"    \"pitch\": {F(config.Rack.Pitch)}");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"heights\": {");
            sb.AppendLine($"    \"zSafe\": {F(config.Heights.ZSafe)},");
            sb.AppendLine($"    \"zEmpty\": {F(config.Heights.ZEmpty)},");
            sb.AppendLine($"    \"zApproach\": {F(config.Heights.ZApproach)},");
            sb.AppendLine($"    \"zClamp\": {F(config.Heights.ZClamp)},");
            sb.AppendLine($"    \"zCheck\": {F(config.Heights.ZCheck)}");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"feedsAndTiming\": {");
            sb.AppendLine($"    \"feedRate\": {F(config.FeedsAndTiming.FeedRate)},");
            sb.AppendLine($"    \"operationDelay\": {config.FeedsAndTiming.OperationDelay},");
            sb.AppendLine($"    \"sensorWaitTime\": {config.FeedsAndTiming.SensorWaitTime}");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"tcp\": {");
            sb.AppendLine($"    \"x\": {F(config.TCP.X)},");
            sb.AppendLine($"    \"y\": {F(config.TCP.Y)},");
            sb.AppendLine($"    \"z\": {F(config.TCP.Z)}");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"io\": {");
            sb.AppendLine($"    \"drawbar\": {{ \"port\": {config.IO.Drawbar.Port}, \"pin\": {config.IO.Drawbar.Pin} }},");
            sb.AppendLine($"    \"airBlow\": {{ \"enabled\": {B(config.IO.AirBlow.Enabled)}, \"port\": {config.IO.AirBlow.Port}, \"pin\": {config.IO.AirBlow.Pin} }},");
            sb.AppendLine($"    \"pressureSensor\": {{ \"enabled\": {B(config.IO.PressureSensor.Enabled)}, \"port\": {config.IO.PressureSensor.Port}, \"pin\": {config.IO.PressureSensor.Pin} }},");
            sb.AppendLine("    \"slotSensors\": {");
            sb.AppendLine($"      \"enabled\": {B(config.IO.SlotSensors.Enabled)},");
            sb.AppendLine($"      \"port\": {config.IO.SlotSensors.Port},");
            sb.Append("      \"pins\": [");
            sb.Append(string.Join(", ", config.IO.SlotSensors.Pins));
            sb.AppendLine("]");
            sb.AppendLine("    }");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"slots\": [");
            for (int i = 0; i < config.Slots.Count; i++)
            {
                var slot = config.Slots[i];
                string comma = i < config.Slots.Count - 1 ? "," : "";
                sb.AppendLine($"    {{ \"index\": {slot.Index}, \"enabled\": {B(slot.Enabled)}, \"toolNumber\": {slot.ToolNumber} }}{comma}");
            }
            sb.AppendLine("  ],");
            
            sb.AppendLine("  \"options\": {");
            sb.AppendLine($"    \"applyG43\": {B(config.Options.ApplyG43)},");
            sb.AppendLine($"    \"returnToXY\": {B(config.Options.ReturnToXY)},");
            sb.AppendLine($"    \"measureNonRH\": {B(config.Options.MeasureNonRH)},");
            sb.AppendLine($"    \"showATCPage\": {B(config.Options.ShowATCPage)},");
            sb.AppendLine($"    \"airBlowDuringTC\": {B(config.Options.AirBlowDuringTC)}");
            sb.AppendLine("  },");
            
            sb.AppendLine("  \"simulation\": {");
            sb.AppendLine($"    \"enabled\": {B(config.Simulation.Enabled)},");
            sb.Append("    \"slotStates\": [");
            sb.Append(string.Join(", ", Array.ConvertAll(config.Simulation.SlotStates, b => B(b))));
            sb.AppendLine("],");
            sb.AppendLine($"    \"pressureOK\": {B(config.Simulation.PressureOK)},");
            sb.AppendLine($"    \"ossdOK\": {B(config.Simulation.OssdOK)}");
            sb.AppendLine("  }");
            
            sb.AppendLine("}");
            return sb.ToString();
        }

        private ATCConfig DeserializeConfig(string json)
        {
            var config = ATCConfig.CreateDefault();
            
            config.Rack.Axis = ExtractString(json, "axis") ?? "X";
            config.Rack.SlotCount = ExtractInt(json, "slotCount", 10);
            config.Rack.Slot1Position = ExtractDouble(json, "slot1Position");
            config.Rack.Slot1PositionSecondary = ExtractDouble(json, "slot1PositionSecondary");
            config.Rack.Pitch = ExtractDouble(json, "pitch", 80);
            
            config.Heights.ZSafe = ExtractDouble(json, "zSafe");
            config.Heights.ZEmpty = ExtractDouble(json, "zEmpty");
            config.Heights.ZApproach = ExtractDouble(json, "zApproach");
            config.Heights.ZClamp = ExtractDouble(json, "zClamp");
            config.Heights.ZCheck = ExtractDouble(json, "zCheck");
            
            config.FeedsAndTiming.FeedRate = ExtractDouble(json, "feedRate", 2000);
            config.FeedsAndTiming.OperationDelay = ExtractInt(json, "operationDelay", 300);
            config.FeedsAndTiming.SensorWaitTime = ExtractInt(json, "sensorWaitTime", 5000);
            
            config.TCP.X = ExtractDouble(json, "\"tcp\".*?\"x\":\\s*([\\d.-]+)", 0, true);
            config.TCP.Y = ExtractDouble(json, "\"tcp\".*?\"y\":\\s*([\\d.-]+)", 0, true);
            config.TCP.Z = ExtractDouble(json, "\"tcp\".*?\"z\":\\s*([\\d.-]+)", 0, true);
            
            config.IO.Drawbar.Port = ExtractInt(json, "\"drawbar\".*?\"port\":\\s*(\\d+)", 1, true);
            config.IO.Drawbar.Pin = ExtractInt(json, "\"drawbar\".*?\"pin\":\\s*(\\d+)", 1, true);
            
            config.IO.AirBlow.Enabled = ExtractBool(json, "\"airBlow\".*?\"enabled\":\\s*(true|false)", false, true);
            config.IO.AirBlow.Port = ExtractInt(json, "\"airBlow\".*?\"port\":\\s*(\\d+)", 1, true);
            config.IO.AirBlow.Pin = ExtractInt(json, "\"airBlow\".*?\"pin\":\\s*(\\d+)", 1, true);
            
            config.IO.PressureSensor.Enabled = ExtractBool(json, "\"pressureSensor\".*?\"enabled\":\\s*(true|false)", false, true);
            config.IO.PressureSensor.Port = ExtractInt(json, "\"pressureSensor\".*?\"port\":\\s*(\\d+)", 1, true);
            config.IO.PressureSensor.Pin = ExtractInt(json, "\"pressureSensor\".*?\"pin\":\\s*(\\d+)", 1, true);
            
            config.IO.SlotSensors.Enabled = ExtractBool(json, "\"slotSensors\".*?\"enabled\":\\s*(true|false)", true, true);
            config.IO.SlotSensors.Port = ExtractInt(json, "\"slotSensors\".*?\"port\":\\s*(\\d+)", 5, true);
            
            config.Options.ApplyG43 = ExtractBool(json, "\"applyG43\":\\s*(true|false)", true, true);
            config.Options.ReturnToXY = ExtractBool(json, "\"returnToXY\":\\s*(true|false)", true, true);
            config.Options.MeasureNonRH = ExtractBool(json, "\"measureNonRH\":\\s*(true|false)", false, true);
            config.Options.ShowATCPage = ExtractBool(json, "\"showATCPage\":\\s*(true|false)", false, true);
            config.Options.AirBlowDuringTC = ExtractBool(json, "\"airBlowDuringTC\":\\s*(true|false)", true, true);
            
            config.Simulation.Enabled = ExtractBool(json, "\"simulation\".*?\"enabled\":\\s*(true|false)", false, true);
            config.Simulation.PressureOK = ExtractBool(json, "\"pressureOK\":\\s*(true|false)", true, true);
            config.Simulation.OssdOK = ExtractBool(json, "\"ossdOK\":\\s*(true|false)", true, true);
            
            var slotsMatch = System.Text.RegularExpressions.Regex.Matches(json, 
                "\"index\":\\s*(\\d+),\\s*\"enabled\":\\s*(true|false),\\s*\"toolNumber\":\\s*(\\d+)");
            foreach (System.Text.RegularExpressions.Match m in slotsMatch)
            {
                int index = int.Parse(m.Groups[1].Value);
                bool enabled = m.Groups[2].Value == "true";
                int toolNum = int.Parse(m.Groups[3].Value);
                
                if (index >= 1 && index <= 10)
                {
                    config.Slots[index - 1].Enabled = enabled;
                    config.Slots[index - 1].ToolNumber = toolNum;
                }
            }
            
            return config;
        }

        private string ExtractString(string json, string key)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, 
                $"\"{key}\":\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        private double ExtractDouble(string json, string key, double defaultValue = 0)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, 
                $"\"{key}\":\\s*([\\d.-]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, 
                NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val;
            return defaultValue;
        }

        private double ExtractDouble(string json, string pattern, double defaultValue, bool isPattern)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, pattern, 
                System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success && double.TryParse(match.Groups[1].Value, 
                NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val;
            return defaultValue;
        }

        private int ExtractInt(string json, string key, int defaultValue = 0)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, 
                $"\"{key}\":\\s*(\\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int val))
                return val;
            return defaultValue;
        }

        private int ExtractInt(string json, string pattern, int defaultValue, bool isPattern)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, pattern, 
                System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int val))
                return val;
            return defaultValue;
        }

        private bool ExtractBool(string json, string pattern, bool defaultValue, bool isPattern)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, pattern, 
                System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success)
                return match.Groups[1].Value == "true";
            return defaultValue;
        }

        private static string F(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }

        private static string B(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
