using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace HTM_PRO.Core
{
    /// <summary>
    /// Supported UCCNC motion controller hardware models.
    /// </summary>
    public enum ControllerModel
    {
        UC100,
        UC400ETH,
        AXBB,
        UC300_5LPT,
        UC300ETH_5LPT,
        UC300ETH_UB1,
        UC300_M44,
        UC300ETH_M44,
        UC300ETH_M45,
        UC300_5441,
        UC300ETH_5441
    }

    /// <summary>
    /// Physical Input and Output pin capabilities for a specific controller port.
    /// </summary>
    public sealed class PortSpec
    {
        public int PortNumber { get; }
        public IReadOnlyList<int> InputPins { get; }
        public IReadOnlyList<int> OutputPins { get; }

        public PortSpec(int portNumber, int[] inputPins, int[] outputPins)
        {
            PortNumber = portNumber;
            InputPins = Array.AsReadOnly(inputPins ?? Array.Empty<int>());
            OutputPins = Array.AsReadOnly(outputPins ?? Array.Empty<int>());
        }
    }

    /// <summary>
    /// Complete hardware I/O specification for a UCCNC motion controller.
    /// </summary>
    public sealed class ControllerHardwareSpec
    {
        public ControllerModel Model { get; }
        public string DisplayName { get; }
        public string Notes { get; }
        public IReadOnlyDictionary<int, PortSpec> Ports { get; }
        public IReadOnlyList<int> PreferredOutputPortOrder { get; }
        public IReadOnlyList<int> PreferredInputPortOrder { get; }

        public int TotalInputs => Ports.Values.Sum(p => p.InputPins.Count);
        public int TotalOutputs => Ports.Values.Sum(p => p.OutputPins.Count);
        public int MaxPortNumber => Ports.Count > 0 ? Ports.Keys.Max() : 1;

        public ControllerHardwareSpec(
            ControllerModel model,
            string displayName,
            string notes,
            IEnumerable<PortSpec> ports,
            int[] preferredOutputPortOrder,
            int[] preferredInputPortOrder)
        {
            Model = model;
            DisplayName = displayName;
            Notes = notes;
            Ports = ports.ToDictionary(p => p.PortNumber);
            PreferredOutputPortOrder = Array.AsReadOnly(preferredOutputPortOrder);
            PreferredInputPortOrder = Array.AsReadOnly(preferredInputPortOrder);
        }

        public bool IsValidInputPin(int port, int pin)
        {
            if (port == 0 && pin == 0) return true;
            return Ports.TryGetValue(port, out var p) && p.InputPins.Contains(pin);
        }

        public bool IsValidOutputPin(int port, int pin)
        {
            if (port == 0 && pin == 0) return true;
            return Ports.TryGetValue(port, out var p) && p.OutputPins.Contains(pin);
        }

        public IEnumerable<(int Port, int Pin)> EnumerateAllInputs(bool preferredOrder = true)
        {
            var portOrder = preferredOrder
                ? PreferredInputPortOrder
                : Ports.Keys.OrderBy(k => k).ToArray();

            foreach (int port in portOrder)
            {
                if (!Ports.TryGetValue(port, out var spec)) continue;
                foreach (int pin in spec.InputPins)
                {
                    yield return (port, pin);
                }
            }
        }

        public IEnumerable<(int Port, int Pin)> EnumerateAllOutputs(bool preferredOrder = true)
        {
            var portOrder = preferredOrder
                ? PreferredOutputPortOrder
                : Ports.Keys.OrderBy(k => k).ToArray();

            foreach (int port in portOrder)
            {
                if (!Ports.TryGetValue(port, out var spec)) continue;
                foreach (int pin in spec.OutputPins)
                {
                    yield return (port, pin);
                }
            }
        }
    }

    /// <summary>
    /// Exact hardware pinout database for all UCCNC-supported motion controllers,
    /// derived from UCCNC's Default2019.ssf Layer 36 pinout mapping and Pinout documentation.
    /// </summary>
    public static class HardwareCatalog
    {
        private static readonly int[] StdLptInputs = { 10, 11, 12, 13, 15 };
        private static readonly int[] StdLptOutputs = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 14, 16, 17 };
        private static readonly int[] ExtInputs13 = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15 };
        private static readonly int[] ExtOutputs4 = { 1, 14, 16, 17 };
        private static readonly int[] AllPins17 = Enumerable.Range(1, 17).ToArray();

        private static readonly Dictionary<ControllerModel, ControllerHardwareSpec> Specs =
            new Dictionary<ControllerModel, ControllerHardwareSpec>
            {
                [ControllerModel.UC100] = new ControllerHardwareSpec(
                    ControllerModel.UC100,
                    "UC100 (USB - 1 LPT Port)",
                    "5 Inputs, 12 Outputs. Suitable for 3-4 axes; ATC must use 0-1 input modes (Software Tracking).",
                    new[] { new PortSpec(1, StdLptInputs, StdLptOutputs) },
                    new[] { 1 },
                    new[] { 1 }),

                [ControllerModel.UC400ETH] = new ControllerHardwareSpec(
                    ControllerModel.UC400ETH,
                    "UC400ETH (Ethernet - 2 LPT Ports)",
                    "10 Inputs, 24 Outputs. Good for 3-5 axes; supports ATC via Spindle Sensors / Single Active-Slot Sensor.",
                    new[]
                    {
                        new PortSpec(1, StdLptInputs, StdLptOutputs),
                        new PortSpec(2, StdLptInputs, StdLptOutputs)
                    },
                    new[] { 1, 2 },
                    new[] { 1, 2 }),

                [ControllerModel.AXBB] = new ControllerHardwareSpec(
                    ControllerModel.AXBB,
                    "AXBB-E (Ethernet Integrated Controller)",
                    "11 Inputs (6x24V isolated on P2 + 5xLPT on P3), 29 Outputs. Dedicated step/dir on Port 1.",
                    new[]
                    {
                        new PortSpec(1, Array.Empty<int>(), AllPins17),
                        new PortSpec(2, new[] { 1, 2, 3, 4, 5, 6 }, Array.Empty<int>()),
                        new PortSpec(3, StdLptInputs, StdLptOutputs)
                    },
                    new[] { 1, 3 },
                    new[] { 2, 3 }),

                [ControllerModel.UC300_5LPT] = new ControllerHardwareSpec(
                    ControllerModel.UC300_5LPT,
                    "UC300-5LPT (USB - 5 Ports)",
                    "49 Inputs, 36 Outputs. Ports 2 & 3 provide 12 outputs each (ideal for motors); Ports 1, 4, 5 provide 13 inputs each.",
                    new[]
                    {
                        new PortSpec(1, ExtInputs13, ExtOutputs4),
                        new PortSpec(2, StdLptInputs, StdLptOutputs),
                        new PortSpec(3, StdLptInputs, StdLptOutputs),
                        new PortSpec(4, ExtInputs13, ExtOutputs4),
                        new PortSpec(5, ExtInputs13, ExtOutputs4)
                    },
                    new[] { 2, 3, 1, 4, 5 },
                    new[] { 1, 4, 5, 2, 3 }),

                [ControllerModel.UC300ETH_5LPT] = new ControllerHardwareSpec(
                    ControllerModel.UC300ETH_5LPT,
                    "UC300ETH-5LPT (Ethernet - 5 Ports)",
                    "49 Inputs, 36 Outputs. Ports 2 & 3 provide 12 outputs each (ideal for motors); Ports 1, 4, 5 provide 13 inputs each.",
                    new[]
                    {
                        new PortSpec(1, ExtInputs13, ExtOutputs4),
                        new PortSpec(2, StdLptInputs, StdLptOutputs),
                        new PortSpec(3, StdLptInputs, StdLptOutputs),
                        new PortSpec(4, ExtInputs13, ExtOutputs4),
                        new PortSpec(5, ExtInputs13, ExtOutputs4)
                    },
                    new[] { 2, 3, 1, 4, 5 },
                    new[] { 1, 4, 5, 2, 3 }),

                [ControllerModel.UC300ETH_UB1] = new ControllerHardwareSpec(
                    ControllerModel.UC300ETH_UB1,
                    "UC300ETH-UB1 (CNCroom UB1 Breakout)",
                    "49 Inputs, 36 Outputs (5LPT layout on UB1 industrial 24V board).",
                    new[]
                    {
                        new PortSpec(1, ExtInputs13, ExtOutputs4),
                        new PortSpec(2, StdLptInputs, StdLptOutputs),
                        new PortSpec(3, StdLptInputs, StdLptOutputs),
                        new PortSpec(4, ExtInputs13, ExtOutputs4),
                        new PortSpec(5, ExtInputs13, ExtOutputs4)
                    },
                    new[] { 2, 3, 1, 4, 5 },
                    new[] { 1, 4, 5, 2, 3 }),

                [ControllerModel.UC300_M44] = new ControllerHardwareSpec(
                    ControllerModel.UC300_M44,
                    "UC300-M44 (USB - M44 Motherboard)",
                    "52 Inputs, 33 Outputs. Port 1 & 5 for primary outputs; Port 4 is all 17 inputs.",
                    new[]
                    {
                        new PortSpec(1, StdLptInputs, StdLptOutputs),
                        new PortSpec(2, ExtInputs13, ExtOutputs4),
                        new PortSpec(3, ExtInputs13, ExtOutputs4),
                        new PortSpec(4, AllPins17, Array.Empty<int>()),
                        new PortSpec(5, new[] { 1, 2, 3, 4 }, Enumerable.Range(5, 13).ToArray())
                    },
                    new[] { 1, 5, 2, 3 },
                    new[] { 4, 2, 3, 1, 5 }),

                [ControllerModel.UC300ETH_M44] = new ControllerHardwareSpec(
                    ControllerModel.UC300ETH_M44,
                    "UC300ETH-M44 (Ethernet - M44 Motherboard)",
                    "52 Inputs, 33 Outputs. Port 1 & 5 for primary outputs; Port 4 is all 17 inputs.",
                    new[]
                    {
                        new PortSpec(1, StdLptInputs, StdLptOutputs),
                        new PortSpec(2, ExtInputs13, ExtOutputs4),
                        new PortSpec(3, ExtInputs13, ExtOutputs4),
                        new PortSpec(4, AllPins17, Array.Empty<int>()),
                        new PortSpec(5, new[] { 1, 2, 3, 4 }, Enumerable.Range(5, 13).ToArray())
                    },
                    new[] { 1, 5, 2, 3 },
                    new[] { 4, 2, 3, 1, 5 }),

                [ControllerModel.UC300ETH_M45] = new ControllerHardwareSpec(
                    ControllerModel.UC300ETH_M45,
                    "UC300ETH-M45 (Ethernet - M45 Motherboard)",
                    "49 Inputs, 36 Outputs. Ports 1 & 4 have 12 outputs each; Ports 2, 3, 5 have 13 inputs each.",
                    new[]
                    {
                        new PortSpec(1, StdLptInputs, StdLptOutputs),
                        new PortSpec(2, ExtInputs13, ExtOutputs4),
                        new PortSpec(3, ExtInputs13, ExtOutputs4),
                        new PortSpec(4, StdLptInputs, StdLptOutputs),
                        new PortSpec(5, ExtInputs13, ExtOutputs4)
                    },
                    new[] { 1, 4, 2, 3, 5 },
                    new[] { 2, 3, 5, 1, 4 }),

                [ControllerModel.UC300_5441] = new ControllerHardwareSpec(
                    ControllerModel.UC300_5441,
                    "UC300-5441 (USB - Hi/Low Motherboard)",
                    "54 Inputs (Ports 1-3 all inputs), 31 Outputs (Ports 4-5).",
                    new[]
                    {
                        new PortSpec(1, AllPins17, Array.Empty<int>()),
                        new PortSpec(2, AllPins17, Array.Empty<int>()),
                        new PortSpec(3, AllPins17, Array.Empty<int>()),
                        new PortSpec(4, new[] { 1, 2, 3 }, Enumerable.Range(4, 14).ToArray()),
                        new PortSpec(5, Array.Empty<int>(), AllPins17)
                    },
                    new[] { 5, 4 },
                    new[] { 1, 2, 3, 4 }),

                [ControllerModel.UC300ETH_5441] = new ControllerHardwareSpec(
                    ControllerModel.UC300ETH_5441,
                    "UC300ETH-5441 (Ethernet - Hi/Low Motherboard)",
                    "54 Inputs (Ports 1-3 all inputs), 31 Outputs (Ports 4-5).",
                    new[]
                    {
                        new PortSpec(1, AllPins17, Array.Empty<int>()),
                        new PortSpec(2, AllPins17, Array.Empty<int>()),
                        new PortSpec(3, AllPins17, Array.Empty<int>()),
                        new PortSpec(4, new[] { 1, 2, 3 }, Enumerable.Range(4, 14).ToArray()),
                        new PortSpec(5, Array.Empty<int>(), AllPins17)
                    },
                    new[] { 5, 4 },
                    new[] { 1, 2, 3, 4 })
            };

        public static IReadOnlyCollection<ControllerHardwareSpec> GetAllSpecs() => Specs.Values;

        public static ControllerHardwareSpec GetSpec(ControllerModel model)
        {
            return Specs.TryGetValue(model, out var spec) ? spec : Specs[ControllerModel.UC300ETH_5LPT];
        }

        public static ControllerModel ParseModel(string rawName, ControllerModel fallback = ControllerModel.UC300ETH_5LPT)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return fallback;

            string norm = rawName.Trim();
            if (norm.StartsWith("Demo_", StringComparison.OrdinalIgnoreCase))
            {
                norm = norm.Substring(5);
            }

            if (norm.Equals("mode", StringComparison.OrdinalIgnoreCase))
            {
                return fallback;
            }

            if (norm.IndexOf("5441", StringComparison.OrdinalIgnoreCase) >= 0 ||
                norm.Equals("UC300_Hi", StringComparison.OrdinalIgnoreCase) ||
                norm.Equals("UC300_Low", StringComparison.OrdinalIgnoreCase) ||
                norm.Equals("UC300ETH_Hi", StringComparison.OrdinalIgnoreCase) ||
                norm.Equals("UC300ETH_Low", StringComparison.OrdinalIgnoreCase))
            {
                return norm.IndexOf("ETH", StringComparison.OrdinalIgnoreCase) >= 0
                    ? ControllerModel.UC300ETH_5441
                    : ControllerModel.UC300_5441;
            }

            if (Enum.TryParse<ControllerModel>(norm, true, out var parsed))
            {
                return parsed;
            }

            return fallback;
        }

        /// <summary>
        /// Inspects UCCNC's Form1 userselecteddevtype field to detect the active hardware or demo controller.
        /// </summary>
        public static (ControllerModel Model, bool IsDemoMode, string RawDeviceType) DetectActiveController(Form mainForm)
        {
            string raw = "Demo_mode";
            bool isDemo = true;

            try
            {
                if (mainForm != null)
                {
                    FieldInfo devTypeField = mainForm.GetType().GetField(
                        "userselecteddevtype",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    object val = devTypeField?.GetValue(mainForm);
                    if (val != null)
                    {
                        raw = val.ToString();
                        isDemo = raw.StartsWith("Demo", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch { }

            ControllerModel model = ParseModel(raw, ControllerModel.UC300ETH_5LPT);
            return (model, isDemo, raw);
        }
    }
}
