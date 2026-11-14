using System;
using System.Collections.Generic;
using System.Linq;
using HTM_PRO.Config;

namespace HTM_PRO.Core
{
    public sealed class PinAssignmentEntry
    {
        public string Direction { get; set; } // "INPUT" or "OUTPUT"
        public int Port { get; set; }
        public int Pin { get; set; }
        public string FunctionName { get; set; }
        public bool Negate { get; set; }
        public bool IsSharedIntentional { get; set; }
    }

    public sealed class PinAuditReport
    {
        public ControllerHardwareSpec Hardware { get; set; }
        public List<PinAssignmentEntry> AssignedInputs { get; } = new List<PinAssignmentEntry>();
        public List<PinAssignmentEntry> AssignedOutputs { get; } = new List<PinAssignmentEntry>();
        public List<(int Port, int Pin)> FreeInputs { get; } = new List<(int Port, int Pin)>();
        public List<(int Port, int Pin)> FreeOutputs { get; } = new List<(int Port, int Pin)>();
        public List<string> FreeAxisChannels { get; } = new List<string>();
        public List<string> Conflicts { get; } = new List<string>();
        public List<string> InvalidPins { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public int UsedInputCount => AssignedInputs.Select(a => (a.Port, a.Pin)).Distinct().Count();
        public int UsedOutputCount => AssignedOutputs.Select(a => (a.Port, a.Pin)).Distinct().Count();
        public int FreeInputCount => FreeInputs.Count;
        public int FreeOutputCount => FreeOutputs.Count;

        public string GetAtcReadinessSummary()
        {
            // Typical ATC outputs: Drawbar (1) + Air Blow (1) = 2 outputs (plus Optional Magazine/Cover)
            bool hasOutputsForAtc = FreeOutputCount >= 2;
            bool hasFreeAxisForRotary = FreeAxisChannels.Count > 0;

            int usableForSlotSensors = Math.Max(0, FreeInputCount - 2); // subtract 1 Air Pressure + 1 Tool Setter or Spindle S1/S2
            string perSlotNote = FreeInputCount >= 22
                ? "Full Per-Slot Sensors up to 20 linear slots"
                : FreeInputCount >= 6
                    ? $"Limited Per-Slot Sensors (max ~{Math.Max(1, FreeInputCount - 2)} slots) OR use Single Active-Slot / Spindle S1+S2 (up to 20/32 slots)"
                    : FreeInputCount >= 2
                        ? "Single Active-Slot (1 pin) / Spindle Sensors (1-2 pins) / Software Tracking (0 pins)"
                        : "Software Tracking only (0 additional inputs available)";

            string rotaryNote = hasFreeAxisForRotary
                ? $"Free axis channel(s) [{string.Join(", ", FreeAxisChannels)}] available for Rotary Magazine (up to 32 slots)."
                : "No free axis channel left (all A/B/C channels used) -> Rotary ATC requires 1 free axis.";

            string outNote = hasOutputsForAtc
                ? $"{FreeOutputCount} free output(s) available for ATC solenoids."
                : $"WARNING: Only {FreeOutputCount} free output(s) left (recommend at least 2: Drawbar + Air Blow; consider Shared Enable Pin!).";

            return $"{outNote}\r\nInputs: {FreeInputCount} free -> {perSlotNote}\r\nAxes: {rotaryNote}";
        }
    }

    /// <summary>
    /// Handles intelligent hardware pin auto-assignment, conflict detection,
    /// and Stage 2 (ATC) resource capacity analysis.
    /// </summary>
    public static class PinAllocator
    {
        /// <summary>
        /// Auto-assigns valid Output pins (Step, Dir, Enable, Spindle PWM, Relays M3/M4/M7/M8)
        /// in preferred port order for the active controller hardware.
        /// </summary>
        public static void AutoAssignOutputs(MachineConfig cfg, ControllerHardwareSpec spec)
        {
            if (cfg == null || spec == null) return;
            cfg.EnsureSixAxes();

            var pool = new Queue<(int Port, int Pin)>(spec.EnumerateAllOutputs(preferredOrder: true));
            (int Port, int Pin) NextOut() => pool.Count > 0 ? pool.Dequeue() : (0, 0);

            // Clear all existing motor/spindle outputs first so allocation is deterministic
            foreach (var ax in cfg.Axes)
            {
                ax.StepPort = 0; ax.StepPin = 0;
                ax.DirPort = 0; ax.DirPin = 0;
                ax.EnaPort = 0; ax.EnaPin = 0;
            }
            cfg.SharedEnablePort = 0; cfg.SharedEnablePin = 0;
            cfg.SpindlePwmPort = 0; cfg.SpindlePwmPin = 0;
            cfg.M3RelayPort = 0; cfg.M3RelayPin = 0;
            cfg.M4RelayPort = 0; cfg.M4RelayPin = 0;
            cfg.M7MistPort = 0; cfg.M7MistPin = 0;
            cfg.M8FloodPort = 0; cfg.M8FloodPin = 0;

            // 1. Assign Step and Dir for each active/slave axis
            foreach (var ax in cfg.Axes.Where(a => a.Enabled))
            {
                var step = NextOut();
                var dir = NextOut();
                ax.StepPort = step.Port;
                ax.StepPin = step.Pin;
                ax.DirPort = dir.Port;
                ax.DirPin = dir.Pin;
            }

            // 2. Assign Enable outputs according to chosen strategy
            if (cfg.EnableStrategy == EnablePinStrategy.SharedSinglePin)
            {
                var ena = NextOut();
                cfg.SharedEnablePort = ena.Port;
                cfg.SharedEnablePin = ena.Pin;
                foreach (var ax in cfg.Axes.Where(a => a.Enabled))
                {
                    ax.EnaPort = ena.Port;
                    ax.EnaPin = ena.Pin;
                    ax.EnaNegate = cfg.SharedEnableNegate;
                }
            }
            else if (cfg.EnableStrategy == EnablePinStrategy.IndividualPerAxis)
            {
                foreach (var ax in cfg.Axes.Where(a => a.Enabled))
                {
                    var ena = NextOut();
                    ax.EnaPort = ena.Port;
                    ax.EnaPin = ena.Pin;
                }
            }

            // 3. Assign Spindle PWM + Relays
            if (cfg.SpindleMode == SpindleControlMode.PwmAndRelay || cfg.SpindleMode == SpindleControlMode.StepDirAndRelay)
            {
                var pwm = NextOut();
                cfg.SpindlePwmPort = pwm.Port;
                cfg.SpindlePwmPin = pwm.Pin;
            }

            var m3 = NextOut();
            cfg.M3RelayPort = m3.Port;
            cfg.M3RelayPin = m3.Pin;

            if (cfg.UseM4Relay)
            {
                var m4 = NextOut();
                cfg.M4RelayPort = m4.Port;
                cfg.M4RelayPin = m4.Pin;
            }
            if (cfg.UseMistM7)
            {
                var m7 = NextOut();
                cfg.M7MistPort = m7.Port;
                cfg.M7MistPin = m7.Pin;
            }
            if (cfg.UseFloodM8)
            {
                var m8 = NextOut();
                cfg.M8FloodPort = m8.Port;
                cfg.M8FloodPin = m8.Pin;
            }
        }

        /// <summary>
        /// Applies the Shared Single Enable Pin across all enabled axes when SharedSinglePin is selected.
        /// </summary>
        public static void SyncSharedEnablePin(MachineConfig cfg)
        {
            if (cfg == null || cfg.EnableStrategy != EnablePinStrategy.SharedSinglePin) return;
            foreach (var ax in cfg.Axes.Where(a => a.Enabled))
            {
                ax.EnaPort = cfg.SharedEnablePort;
                ax.EnaPin = cfg.SharedEnablePin;
                ax.EnaNegate = cfg.SharedEnableNegate;
            }
        }

        /// <summary>
        /// Auto-assigns valid Input pins (E-Stop, Probe 1, Probe 2, Home & Limit switches)
        /// according to LimitHomeWiringStrategy.
        /// </summary>
        public static void AutoAssignInputs(MachineConfig cfg, ControllerHardwareSpec spec)
        {
            if (cfg == null || cfg.Axes == null || spec == null) return;
            cfg.EnsureSixAxes();

            var pool = new Queue<(int Port, int Pin)>(spec.EnumerateAllInputs(preferredOrder: true));
            (int Port, int Pin) NextIn() => pool.Count > 0 ? pool.Dequeue() : (0, 0);

            foreach (var ax in cfg.Axes)
            {
                ax.HomePort = 0; ax.HomePin = 0;
                ax.LimitMinusPort = 0; ax.LimitMinusPin = 0;
                ax.LimitPlusPort = 0; ax.LimitPlusPin = 0;
            }
            cfg.SharedSeriesLimitPort = 0; cfg.SharedSeriesLimitPin = 0;

            // 1. Critical safety & probing inputs first
            var estop = NextIn();
            cfg.EstopPort = estop.Port;
            cfg.EstopPin = estop.Pin;

            var probe1 = NextIn();
            cfg.Probe1Port = probe1.Port;
            cfg.Probe1Pin = probe1.Pin;

            if (cfg.UseProbe2)
            {
                var probe2 = NextIn();
                cfg.Probe2Port = probe2.Port;
                cfg.Probe2Pin = probe2.Pin;
            }
            else
            {
                cfg.Probe2Port = 0;
                cfg.Probe2Pin = 0;
            }

            // 2. Home & Limit switches per active axis according to HomeGrouping
            var enabledAxes = cfg.Axes.Where(a => a.Enabled).ToList();

            if (cfg.HomeGrouping == HomePinGrouping.CommonMasterAndCommonSlave)
            {
                var masterHome = NextIn();
                foreach (var ax in enabledAxes.Where(a => a.Role != AxisRole.GantrySlave))
                {
                    ax.HomePort = masterHome.Port;
                    ax.HomePin = masterHome.Pin;
                    ApplySharedHomeAndLimitIfNeeded(cfg, ax, masterHome.Port, masterHome.Pin);
                }

                var enabledSlaves = enabledAxes.Where(a => a.Role == AxisRole.GantrySlave).ToList();
                if (cfg.IndependentSlaveHome && enabledSlaves.Count > 0)
                {
                    var slaveHome = NextIn();
                    foreach (var sl in enabledSlaves)
                    {
                        sl.HomePort = slaveHome.Port;
                        sl.HomePin = slaveHome.Pin;
                        ApplySharedHomeAndLimitIfNeeded(cfg, sl, slaveHome.Port, slaveHome.Pin);
                    }
                }
            }
            else if (cfg.HomeGrouping == HomePinGrouping.SingleCommonAllAxes)
            {
                var commonHome = NextIn();
                foreach (var ax in enabledAxes)
                {
                    if (ax.Role == AxisRole.GantrySlave && !cfg.IndependentSlaveHome)
                    {
                        continue;
                    }
                    ax.HomePort = commonHome.Port;
                    ax.HomePin = commonHome.Pin;
                    ApplySharedHomeAndLimitIfNeeded(cfg, ax, commonHome.Port, commonHome.Pin);
                }
            }
            else
            {
                foreach (var ax in enabledAxes)
                {
                    // Skip slave home if user explicitly disabled independent gantry squaring
                    if (ax.Role == AxisRole.GantrySlave && !cfg.IndependentSlaveHome)
                    {
                        continue;
                    }

                    var home = NextIn();
                    ax.HomePort = home.Port;
                    ax.HomePin = home.Pin;
                    ApplySharedHomeAndLimitIfNeeded(cfg, ax, home.Port, home.Pin);
                }
            }

            if (cfg.LimitStrategy == LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin)
            {
                var seriesLim = NextIn();
                cfg.SharedSeriesLimitPort = seriesLim.Port;
                cfg.SharedSeriesLimitPin = seriesLim.Pin;
                foreach (var ax in enabledAxes.Where(a => a.Role != AxisRole.GantrySlave))
                {
                    ax.LimitMinusPort = seriesLim.Port;
                    ax.LimitMinusPin = seriesLim.Pin;
                    ax.LimitMinusNegate = cfg.SharedSeriesLimitNegate;
                    ax.LimitPlusPort = seriesLim.Port;
                    ax.LimitPlusPin = seriesLim.Pin;
                    ax.LimitPlusNegate = cfg.SharedSeriesLimitNegate;
                }
            }
            else if (cfg.LimitStrategy == LimitHomeWiringStrategy.SeparateHomeLimitMinusLimitPlus)
            {
                foreach (var ax in enabledAxes.Where(a => a.Role != AxisRole.GantrySlave))
                {
                    var lm = NextIn();
                    var lp = NextIn();
                    ax.LimitMinusPort = lm.Port;
                    ax.LimitMinusPin = lm.Pin;
                    ax.LimitPlusPort = lp.Port;
                    ax.LimitPlusPin = lp.Pin;
                }
            }
        }

        /// <summary>
        /// Mirrors an axis's Home input to Limit+ or Limit- when SharedHomeAndLimitPerAxis is active.
        /// </summary>
        public static void ApplySharedHomeAndLimitIfNeeded(MachineConfig cfg, AxisConfig ax, int port, int pin)
        {
            if (cfg == null || ax == null || cfg.LimitStrategy != LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis) return;
            if (ax.HomeDirectionPositive)
            {
                ax.LimitPlusPort = port;
                ax.LimitPlusPin = pin;
                ax.LimitPlusNegate = ax.HomeNegate;
            }
            else
            {
                ax.LimitMinusPort = port;
                ax.LimitMinusPin = pin;
                ax.LimitMinusNegate = ax.HomeNegate;
            }
        }

        /// <summary>
        /// Audits every configured Input and Output pin against controller capabilities,
        /// checks for accidental collisions, and computes free pins/axes remaining for ATC.
        /// </summary>
        public static PinAuditReport ValidateAndAudit(MachineConfig cfg, ControllerHardwareSpec spec)
        {
            var report = new PinAuditReport { Hardware = spec };
            if (cfg == null || spec == null) return report;

            cfg.EnsureSixAxes();

            void AddOut(int port, int pin, string fn, bool neg, bool sharedIntentional = false)
            {
                if (port == 0 && pin == 0) return;
                report.AssignedOutputs.Add(new PinAssignmentEntry
                {
                    Direction = "OUTPUT",
                    Port = port,
                    Pin = pin,
                    FunctionName = fn,
                    Negate = neg,
                    IsSharedIntentional = sharedIntentional
                });
                if (!spec.IsValidOutputPin(port, pin))
                {
                    report.InvalidPins.Add($"OUTPUT {fn}: Port {port} Pin {pin} does NOT exist as an Output on {spec.DisplayName}.");
                }
            }

            void AddIn(int port, int pin, string fn, bool neg, bool sharedIntentional = false)
            {
                if (port == 0 && pin == 0) return;
                report.AssignedInputs.Add(new PinAssignmentEntry
                {
                    Direction = "INPUT",
                    Port = port,
                    Pin = pin,
                    FunctionName = fn,
                    Negate = neg,
                    IsSharedIntentional = sharedIntentional
                });
                if (!spec.IsValidInputPin(port, pin))
                {
                    report.InvalidPins.Add($"INPUT {fn}: Port {port} Pin {pin} does NOT exist as an Input on {spec.DisplayName}.");
                }
            }

            // Collect Outputs
            foreach (var ax in cfg.Axes.Where(a => a.Enabled))
            {
                AddOut(ax.StepPort, ax.StepPin, $"{ax.AxisName} Step", ax.StepNegate);
                AddOut(ax.DirPort, ax.DirPin, $"{ax.AxisName} Dir", ax.DirNegate);
                AddOut(ax.EnaPort, ax.EnaPin, $"{ax.AxisName} Enable", ax.EnaNegate,
                    sharedIntentional: cfg.EnableStrategy == EnablePinStrategy.SharedSinglePin);

                // Collect Inputs
                AddIn(ax.HomePort, ax.HomePin, $"{ax.AxisName} Home", ax.HomeNegate);
                AddIn(ax.LimitMinusPort, ax.LimitMinusPin, $"{ax.AxisName} Limit-", ax.LimitMinusNegate,
                    sharedIntentional: cfg.LimitStrategy == LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis ||
                                       cfg.LimitStrategy == LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin);
                AddIn(ax.LimitPlusPort, ax.LimitPlusPin, $"{ax.AxisName} Limit+", ax.LimitPlusNegate,
                    sharedIntentional: cfg.LimitStrategy == LimitHomeWiringStrategy.SharedHomeAndLimitPerAxis ||
                                       cfg.LimitStrategy == LimitHomeWiringStrategy.IndividualHomePlusSeriesLimitPin);

                if (ax.StepPort == 0 || ax.StepPin == 0 || ax.DirPort == 0 || ax.DirPin == 0)
                {
                    report.Warnings.Add($"Axis {ax.AxisName} is enabled but Step or Dir pin is 0 (unassigned).");
                }
            }

            if (cfg.SpindleMode == SpindleControlMode.PwmAndRelay || cfg.SpindleMode == SpindleControlMode.StepDirAndRelay)
            {
                AddOut(cfg.SpindlePwmPort, cfg.SpindlePwmPin, "Spindle PWM / Step", cfg.SpindlePwmNegate);
            }
            AddOut(cfg.M3RelayPort, cfg.M3RelayPin, "Spindle M3 Relay", cfg.M3RelayNegate);
            if (cfg.UseM4Relay) AddOut(cfg.M4RelayPort, cfg.M4RelayPin, "Spindle M4 Relay", cfg.M4RelayNegate);
            if (cfg.UseMistM7) AddOut(cfg.M7MistPort, cfg.M7MistPin, "Mist M7 Relay", cfg.M7MistNegate);
            if (cfg.UseFloodM8) AddOut(cfg.M8FloodPort, cfg.M8FloodPin, "Flood M8 Relay", cfg.M8FloodNegate);

            AddIn(cfg.EstopPort, cfg.EstopPin, "E-Stop", cfg.EstopNegate);
            AddIn(cfg.Probe1Port, cfg.Probe1Pin, "Probe 1 (Tool Setter)", cfg.Probe1Negate);
            if (cfg.UseProbe2) AddIn(cfg.Probe2Port, cfg.Probe2Pin, "Probe 2 (3D Touch Probe)", cfg.Probe2Negate);

            if (cfg.EstopPort == 0 || cfg.EstopPin == 0)
            {
                report.Warnings.Add("E-Stop input pin is unassigned (0/0). Highly recommended to assign before operating machine.");
            }

            // Detect unintended conflicts on Outputs
            foreach (var grp in report.AssignedOutputs.GroupBy(o => (o.Port, o.Pin)))
            {
                var items = grp.ToList();
                if (items.Count <= 1) continue;
                bool allIntentionalSharedEnable = items.All(i => i.FunctionName.EndsWith(" Enable") && i.IsSharedIntentional);
                if (!allIntentionalSharedEnable)
                {
                    report.Conflicts.Add($"OUTPUT Conflict on P{grp.Key.Port}-Pin{grp.Key.Pin}: {string.Join(", ", items.Select(i => i.FunctionName))}");
                }
            }

            // Detect unintended conflicts on Inputs
            foreach (var grp in report.AssignedInputs.GroupBy(i => (i.Port, i.Pin)))
            {
                var items = grp.ToList();
                if (items.Count <= 1) continue;

                bool allAxisSwitches = items.All(i =>
                    i.FunctionName.EndsWith(" Home", StringComparison.Ordinal) ||
                    i.FunctionName.EndsWith(" Limit-", StringComparison.Ordinal) ||
                    i.FunctionName.EndsWith(" Limit+", StringComparison.Ordinal));

                if (!allAxisSwitches)
                {
                    // E-Stop / Probe inputs must never share a pin with other inputs or limit/home buses
                    report.Conflicts.Add($"INPUT Conflict on P{grp.Key.Port}-Pin{grp.Key.Pin}: {string.Join(", ", items.Select(i => i.FunctionName))}");
                    continue;
                }

                if (cfg.HomeGrouping == HomePinGrouping.CommonMasterAndCommonSlave)
                {
                    bool slaveSharesMasterHome = cfg.Axes.Any(sl =>
                        sl.Enabled &&
                        sl.Role == AxisRole.GantrySlave &&
                        !string.IsNullOrEmpty(sl.MasterAxis) &&
                        sl.MasterAxis != "None" &&
                        sl.HomePort == grp.Key.Port &&
                        sl.HomePin == grp.Key.Pin &&
                        cfg.Axes.Any(m => m.Enabled &&
                            string.Equals(m.AxisName, sl.MasterAxis, StringComparison.OrdinalIgnoreCase) &&
                            m.HomePort == grp.Key.Port &&
                            m.HomePin == grp.Key.Pin));

                    if (slaveSharesMasterHome)
                    {
                        report.Conflicts.Add($"INPUT Conflict on P{grp.Key.Port}-Pin{grp.Key.Pin}: Slave Home shares pin with its Master ({string.Join(", ", items.Select(i => i.FunctionName))}).");
                    }
                }
            }

            // Compute free outputs & free inputs
            var usedOut = new HashSet<(int Port, int Pin)>(report.AssignedOutputs.Select(o => (o.Port, o.Pin)));
            foreach (var pp in spec.EnumerateAllOutputs(preferredOrder: false))
            {
                if (!usedOut.Contains(pp)) report.FreeOutputs.Add(pp);
            }

            var usedIn = new HashSet<(int Port, int Pin)>(report.AssignedInputs.Select(i => (i.Port, i.Pin)));
            foreach (var pp in spec.EnumerateAllInputs(preferredOrder: false))
            {
                if (!usedIn.Contains(pp)) report.FreeInputs.Add(pp);
            }

            // Compute free axis channels (A, B, C not used for active work/slave)
            foreach (var ax in cfg.Axes.Where(a => a.IsRotaryChannel))
            {
                if (!ax.Enabled || ax.Role == AxisRole.ReservedForAtcRotary || ax.Role == AxisRole.Unused)
                {
                    report.FreeAxisChannels.Add(ax.AxisName);
                }
            }

            return report;
        }
    }
}
