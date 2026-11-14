using System;
using System.Collections.Generic;
using System.Linq;

namespace HTM_PRO.Config
{
    /// <summary>
    /// Functional role assigned to each UCCNC axis channel (X, Y, Z, A, B, C).
    /// </summary>
    public enum AxisRole
    {
        PrimaryLinear,
        GantrySlave,
        RotaryWorkAxis,
        ReservedForAtcRotary,
        Unused
    }

    /// <summary>
    /// Strategy for wiring stepper/servo Enable signals across motors.
    /// </summary>
    public enum EnablePinStrategy
    {
        SharedSinglePin,
        IndividualPerAxis,
        ExternalNoPin
    }

    /// <summary>
    /// Strategy for wiring Home and Limit switches across machine axes.
    /// </summary>
    public enum LimitHomeWiringStrategy
    {
        HomeOnlyWithSoftLimits,
        SharedHomeAndLimitPerAxis,
        IndividualHomePlusSeriesLimitPin,
        SeparateHomeLimitMinusLimitPlus
    }

    /// <summary>
    /// Spindle speed and direction control mode in UCCNC.
    /// </summary>
    public enum SpindleControlMode
    {
        PwmAndRelay,
        StepDirAndRelay,
        RelayOrModbusOnly
    }

    /// <summary>
    /// Per-axis electrical, kinematic, homing, and soft-limit configuration.
    /// </summary>
    public class AxisConfig
    {
        public string AxisName { get; set; } = "X";
        public bool Enabled { get; set; }
        public AxisRole Role { get; set; } = AxisRole.Unused;
        public string MasterAxis { get; set; } = "None";

        // Motor Output Pins
        public int StepPort { get; set; }
        public int StepPin { get; set; }
        public bool StepNegate { get; set; }

        public int DirPort { get; set; }
        public int DirPin { get; set; }
        public bool DirNegate { get; set; }

        public int EnaPort { get; set; }
        public int EnaPin { get; set; }
        public bool EnaNegate { get; set; }

        // Homing & Limit Input Pins
        public int HomePort { get; set; }
        public int HomePin { get; set; }
        public bool HomeNegate { get; set; }

        public int LimitMinusPort { get; set; }
        public int LimitMinusPin { get; set; }
        public bool LimitMinusNegate { get; set; }

        public int LimitPlusPort { get; set; }
        public int LimitPlusPin { get; set; }
        public bool LimitPlusNegate { get; set; }

        // Kinematics & Travel
        public double StepsPerUnit { get; set; } = 200.0;
        public double MaxVelocity { get; set; } = 4000.0;
        public double Acceleration { get; set; } = 400.0;
        public double MaxTravel { get; set; } = 600.0;
        public double SoftLimitNegative { get; set; } = 0.0;
        public double SoftLimitPositive { get; set; } = 600.0;

        // Homing Motion
        public bool HomeDirectionPositive { get; set; } = false;
        public bool HomeAutoZero { get; set; } = true;
        public double HomingSpeedUp { get; set; } = 500.0;
        public double HomingSpeedDown { get; set; } = 150.0;
        public double HomeBackoff { get; set; } = 3.0;

        // Rotary Options (applicable to A, B, C)
        public bool IsRotary { get; set; } = false;
        public bool RotaryRollover { get; set; } = false;

        public AxisConfig()
        {
        }

        public AxisConfig(string name)
        {
            AxisName = name;
        }

        public bool IsRotaryChannel => AxisName == "A" || AxisName == "B" || AxisName == "C";
        public bool RequiresMotorOutput => Enabled || Role == AxisRole.GantrySlave || Role == AxisRole.ReservedForAtcRotary;

        /// <summary>
        /// Recomputes SoftLimitNegative and SoftLimitPositive from MaxTravel and HomeDirectionPositive
        /// assuming Home coordinate equals 0.
        /// </summary>
        public void RecalculateSoftLimitsFromTravel()
        {
            if (MaxTravel <= 0)
            {
                SoftLimitNegative = 0;
                SoftLimitPositive = 0;
                return;
            }

            if (IsRotary && RotaryRollover)
            {
                SoftLimitNegative = -360000;
                SoftLimitPositive = 360000;
                return;
            }

            if (HomeDirectionPositive)
            {
                SoftLimitPositive = 0.0;
                SoftLimitNegative = -Math.Abs(MaxTravel);
            }
            else
            {
                SoftLimitNegative = 0.0;
                SoftLimitPositive = Math.Abs(MaxTravel);
            }
        }
    }

    /// <summary>
    /// Complete Stage 1 Machine Configuration model persisted in UCCNC profile and JSON backup.
    /// </summary>
    public class MachineConfig
    {
        public bool Stage1Completed { get; set; } = false;
        public string ControllerType { get; set; } = "UC300ETH_5LPT";
        public bool IsImperial { get; set; } = false;
        public bool ReserveResourcesForAtc { get; set; } = true;
        public bool UseFreshSetupDefaults { get; set; } = true;

        // X, Y, Z, A, B, C in fixed order
        public List<AxisConfig> Axes { get; set; } = new List<AxisConfig>();

        // Pin & Wiring Strategies
        public EnablePinStrategy EnableStrategy { get; set; } = EnablePinStrategy.SharedSinglePin;
        public int SharedEnablePort { get; set; }
        public int SharedEnablePin { get; set; }
        public bool SharedEnableNegate { get; set; }

        public LimitHomeWiringStrategy LimitStrategy { get; set; } = LimitHomeWiringStrategy.HomeOnlyWithSoftLimits;
        public bool IndependentSlaveHome { get; set; } = true;
        public int SharedSeriesLimitPort { get; set; }
        public int SharedSeriesLimitPin { get; set; }
        public bool SharedSeriesLimitNegate { get; set; }

        // General Motion & Safety
        public bool EnableSoftLimits { get; set; } = true;
        public bool CheckHomeOnCycleStart { get; set; } = true;
        public double SafeZ { get; set; } = 25.0;
        public string[] HomeSequence { get; set; } = new[] { "Z", "X", "Y", "None", "None", "None" };

        // E-Stop Input
        public int EstopPort { get; set; }
        public int EstopPin { get; set; }
        public bool EstopNegate { get; set; } = true;

        // Spindle & Coolant Outputs
        public SpindleControlMode SpindleMode { get; set; } = SpindleControlMode.PwmAndRelay;
        public int SpindlePwmPort { get; set; }
        public int SpindlePwmPin { get; set; }
        public bool SpindlePwmNegate { get; set; }
        public int SpindlePwmFreq { get; set; } = 400;
        public double SpindleMinRpm { get; set; } = 6000;
        public double SpindleMaxRpm { get; set; } = 24000;

        public int M3RelayPort { get; set; }
        public int M3RelayPin { get; set; }
        public bool M3RelayNegate { get; set; }
        public bool UseM4Relay { get; set; } = false;
        public int M4RelayPort { get; set; }
        public int M4RelayPin { get; set; }
        public bool M4RelayNegate { get; set; }

        public bool UseMistM7 { get; set; } = false;
        public int M7MistPort { get; set; }
        public int M7MistPin { get; set; }
        public bool M7MistNegate { get; set; }

        public bool UseFloodM8 { get; set; } = false;
        public int M8FloodPort { get; set; }
        public int M8FloodPin { get; set; }
        public bool M8FloodNegate { get; set; }

        // Probes
        public int Probe1Port { get; set; }
        public int Probe1Pin { get; set; }
        public bool Probe1Negate { get; set; } = true;

        public bool UseProbe2 { get; set; } = false;
        public int Probe2Port { get; set; }
        public int Probe2Pin { get; set; }
        public bool Probe2Negate { get; set; } = false;

        public MachineConfig()
        {
            EnsureSixAxes();
        }

        public void EnsureSixAxes()
        {
            string[] names = { "X", "Y", "Z", "A", "B", "C" };
            var existing = Axes ?? new List<AxisConfig>();
            var normalized = new List<AxisConfig>(6);

            foreach (string name in names)
            {
                var found = existing.FirstOrDefault(a => string.Equals(a.AxisName, name, StringComparison.OrdinalIgnoreCase));
                normalized.Add(found ?? new AxisConfig(name));
            }

            Axes = normalized;
            if (HomeSequence == null || HomeSequence.Length != 6)
            {
                HomeSequence = new[] { "Z", "X", "Y", "None", "None", "None" };
            }
        }

        public AxisConfig GetAxis(string name)
        {
            EnsureSixAxes();
            return Axes.First(a => string.Equals(a.AxisName, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Applies clean baseline kinematic and travel defaults appropriate for Metric (mm) or Imperial (inch).
        /// </summary>
        public void ApplyUnitBaselineDefaults(bool isImperial)
        {
            IsImperial = isImperial;
            EnsureSixAxes();

            var x = GetAxis("X");
            var y = GetAxis("Y");
            var z = GetAxis("Z");
            var a = GetAxis("A");
            var b = GetAxis("B");
            var c = GetAxis("C");

            x.Enabled = true; x.Role = AxisRole.PrimaryLinear; x.MasterAxis = "None";
            y.Enabled = true; y.Role = AxisRole.PrimaryLinear; y.MasterAxis = "None";
            z.Enabled = true; z.Role = AxisRole.PrimaryLinear; z.MasterAxis = "None";

            // Default 3-axis setup: keep A/B/C unused until user configures gantry slave or rotary
            foreach (var rot in new[] { a, b, c })
            {
                if (rot.Role == AxisRole.Unused)
                {
                    rot.Enabled = false;
                }
            }

            x.HomeDirectionPositive = false;
            y.HomeDirectionPositive = false;
            z.HomeDirectionPositive = true; // Typical router Z homes UP at top of travel (Z=0)

            if (isImperial)
            {
                SafeZ = 1.0;
                ApplyLinearDefaults(x, 5080.0, 200.0, 25.0, 24.0, 20.0, 5.0, 0.12);
                ApplyLinearDefaults(y, 5080.0, 200.0, 25.0, 36.0, 20.0, 5.0, 0.12);
                ApplyLinearDefaults(z, 5080.0, 120.0, 20.0, 8.0, 15.0, 4.0, 0.12);
            }
            else
            {
                SafeZ = 25.0;
                ApplyLinearDefaults(x, 200.0, 5000.0, 500.0, 600.0, 500.0, 150.0, 3.0);
                ApplyLinearDefaults(y, 200.0, 5000.0, 500.0, 900.0, 500.0, 150.0, 3.0);
                ApplyLinearDefaults(z, 200.0, 3000.0, 400.0, 200.0, 400.0, 100.0, 3.0);
            }

            ApplyRotaryDefaults(a);
            ApplyRotaryDefaults(b);
            ApplyRotaryDefaults(c);

            HomeSequence = new[] { "Z", "X", "Y", "None", "None", "None" };
        }

        private static void ApplyLinearDefaults(
            AxisConfig axis,
            double stepsPerUnit,
            double vel,
            double accel,
            double travel,
            double homeUp,
            double homeDown,
            double backoff)
        {
            axis.StepsPerUnit = stepsPerUnit;
            axis.MaxVelocity = vel;
            axis.Acceleration = accel;
            axis.MaxTravel = travel;
            axis.HomingSpeedUp = homeUp;
            axis.HomingSpeedDown = homeDown;
            axis.HomeBackoff = backoff;
            axis.RecalculateSoftLimitsFromTravel();
        }

        private static void ApplyRotaryDefaults(AxisConfig axis)
        {
            axis.IsRotary = true;
            axis.StepsPerUnit = 88.8889; // 32000 pulses/rev / 360 deg
            axis.MaxVelocity = 3600.0;
            axis.Acceleration = 1800.0;
            axis.MaxTravel = 360.0;
            axis.HomingSpeedUp = 600.0;
            axis.HomingSpeedDown = 180.0;
            axis.HomeBackoff = 5.0;
        }

        /// <summary>
        /// Synchronizes gantry slave axis motion defaults from its configured master axis.
        /// </summary>
        public void SyncSlaveAxisParameters()
        {
            EnsureSixAxes();
            foreach (var ax in Axes)
            {
                if (ax.Role == AxisRole.GantrySlave && !string.IsNullOrEmpty(ax.MasterAxis) && ax.MasterAxis != "None")
                {
                    var master = GetAxis(ax.MasterAxis);
                    ax.Enabled = master.Enabled;
                    ax.IsRotary = false;
                    ax.StepsPerUnit = master.StepsPerUnit;
                    ax.MaxVelocity = master.MaxVelocity;
                    ax.Acceleration = master.Acceleration;
                    ax.MaxTravel = master.MaxTravel;
                    ax.HomeDirectionPositive = master.HomeDirectionPositive;
                    ax.HomeAutoZero = master.HomeAutoZero;
                    ax.HomingSpeedUp = master.HomingSpeedUp;
                    ax.HomingSpeedDown = master.HomingSpeedDown;
                    ax.HomeBackoff = master.HomeBackoff;
                    ax.SoftLimitNegative = master.SoftLimitNegative;
                    ax.SoftLimitPositive = master.SoftLimitPositive;
                }
            }
        }
    }
}
