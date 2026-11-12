using System;
using System.Globalization;

namespace Plugins.Core
{
    public enum ToolChangeState
    {
        Idle,
        ValidateRequest,
        StopSpindle,
        SavePosition,
        MoveSafeZ,
        DetermineSlots,
        DropToolMoveXY,
        DropVerifyEmpty,
        DropAirBlowOn,
        DropLowerToClamp,
        DropEnterFork,
        DropOpenDrawbar,
        DropRaiseToCheck,
        DropVerifyToolInSlot,
        DropAirBlowOff,
        DropRaiseToEmpty,
        DropCloseDrawbar,
        PickToolMoveXY,
        PickVerifyToolPresent,
        PickAirBlowOn,
        PickLowerToClamp,
        PickCloseDrawbar,
        PickRaiseToCheck,
        PickVerifySlotEmpty,
        PickExitFork,
        PickRaiseToSafe,
        MeasureIfNeeded,
        ApplyOffset,
        RestorePosition,
        Complete,
        ManualUnload,
        ManualLoad,
        Abort,
        Fault
    }

    public class ATCStateMachine
    {
        private readonly ATCConfig _config;
        private readonly Plugininterface.Entry _uc;
        private readonly Action<string> _log;
        private readonly Func<bool> _isSimulation;

        public ToolChangeState CurrentState { get; private set; } = ToolChangeState.Idle;
        public int CurrentTool { get; private set; }
        public int NewTool { get; private set; }
        public int OldSlot { get; private set; }
        public int NewSlot { get; private set; }
        public double OriginalX { get; private set; }
        public double OriginalY { get; private set; }
        public double OriginalZ { get; private set; }
        public int OriginalModalMode { get; private set; }
        public string LastError { get; private set; }
        public bool IsRunning => CurrentState != ToolChangeState.Idle;

        public ATCStateMachine(ATCConfig config, Plugininterface.Entry uc, Action<string> log, Func<bool> isSimulation)
        {
            _config = config;
            _uc = uc;
            _log = log ?? (s => { });
            _isSimulation = isSimulation ?? (() => false);
        }

        public string StartToolChange(int newTool)
        {
            if (IsRunning)
                return "Tool change already in progress";

            NewTool = newTool;
            CurrentState = ToolChangeState.ValidateRequest;
            LastError = null;

            return RunStateMachine();
        }

        public string RunStateMachine()
        {
            while (CurrentState != ToolChangeState.Idle)
            {
                var previousState = CurrentState;
                
                try
                {
                    switch (CurrentState)
                    {
                        case ToolChangeState.ValidateRequest:
                            ExecuteValidateRequest();
                            break;
                        case ToolChangeState.StopSpindle:
                            ExecuteStopSpindle();
                            break;
                        case ToolChangeState.SavePosition:
                            ExecuteSavePosition();
                            break;
                        case ToolChangeState.MoveSafeZ:
                            ExecuteMoveSafeZ();
                            break;
                        case ToolChangeState.DetermineSlots:
                            ExecuteDetermineSlots();
                            break;
                        case ToolChangeState.DropToolMoveXY:
                            ExecuteDropToolMoveXY();
                            break;
                        case ToolChangeState.DropVerifyEmpty:
                            ExecuteDropVerifyEmpty();
                            break;
                        case ToolChangeState.DropAirBlowOn:
                            ExecuteDropAirBlowOn();
                            break;
                        case ToolChangeState.DropLowerToClamp:
                            ExecuteDropLowerToClamp();
                            break;
                        case ToolChangeState.DropEnterFork:
                            ExecuteDropEnterFork();
                            break;
                        case ToolChangeState.DropOpenDrawbar:
                            ExecuteDropOpenDrawbar();
                            break;
                        case ToolChangeState.DropRaiseToCheck:
                            ExecuteDropRaiseToCheck();
                            break;
                        case ToolChangeState.DropVerifyToolInSlot:
                            ExecuteDropVerifyToolInSlot();
                            break;
                        case ToolChangeState.DropAirBlowOff:
                            ExecuteDropAirBlowOff();
                            break;
                        case ToolChangeState.DropRaiseToEmpty:
                            ExecuteDropRaiseToEmpty();
                            break;
                        case ToolChangeState.DropCloseDrawbar:
                            ExecuteDropCloseDrawbar();
                            break;
                        case ToolChangeState.PickToolMoveXY:
                            ExecutePickToolMoveXY();
                            break;
                        case ToolChangeState.PickVerifyToolPresent:
                            ExecutePickVerifyToolPresent();
                            break;
                        case ToolChangeState.PickAirBlowOn:
                            ExecutePickAirBlowOn();
                            break;
                        case ToolChangeState.PickLowerToClamp:
                            ExecutePickLowerToClamp();
                            break;
                        case ToolChangeState.PickCloseDrawbar:
                            ExecutePickCloseDrawbar();
                            break;
                        case ToolChangeState.PickRaiseToCheck:
                            ExecutePickRaiseToCheck();
                            break;
                        case ToolChangeState.PickVerifySlotEmpty:
                            ExecutePickVerifySlotEmpty();
                            break;
                        case ToolChangeState.PickExitFork:
                            ExecutePickExitFork();
                            break;
                        case ToolChangeState.PickRaiseToSafe:
                            ExecutePickRaiseToSafe();
                            break;
                        case ToolChangeState.MeasureIfNeeded:
                            ExecuteMeasureIfNeeded();
                            break;
                        case ToolChangeState.ApplyOffset:
                            ExecuteApplyOffset();
                            break;
                        case ToolChangeState.RestorePosition:
                            ExecuteRestorePosition();
                            break;
                        case ToolChangeState.Complete:
                            ExecuteComplete();
                            break;
                        case ToolChangeState.Abort:
                            ExecuteAbort();
                            break;
                        case ToolChangeState.Fault:
                            ExecuteFault();
                            break;
                        default:
                            CurrentState = ToolChangeState.Idle;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    _log($"State machine error in {previousState}: {ex.Message}");
                    CurrentState = ToolChangeState.Fault;
                }

                if (CheckMacroStopped() && CurrentState != ToolChangeState.Abort && CurrentState != ToolChangeState.Fault)
                {
                    CurrentState = ToolChangeState.Abort;
                }
            }

            return LastError;
        }

        private void ExecuteValidateRequest()
        {
            _log($"Validating tool change request: T{NewTool}");

            if (!CheckAxesHomed())
            {
                LastError = "One or more axes are NOT REFERENCED";
                CurrentState = ToolChangeState.Fault;
                return;
            }

            if (_config.IO.PressureSensor.Enabled && !CheckAirPressure())
            {
                LastError = "No Air Pressure";
                CurrentState = ToolChangeState.Fault;
                return;
            }

            CurrentTool = GetCurrentTool();

            if (NewTool == CurrentTool)
            {
                _log($"Requested tool T{NewTool} already loaded");
                CurrentState = ToolChangeState.ApplyOffset;
                return;
            }

            CurrentState = ToolChangeState.StopSpindle;
        }

        private void ExecuteStopSpindle()
        {
            _log("Stopping spindle...");
            
            ExecuteGCode("M5");
            ExecuteGCode("M9");

            int waitTimeMs = GetSpindleWaitTime() * 1000;
            int elapsed = 0;
            const int checkInterval = 100;

            while (elapsed < waitTimeMs)
            {
                double spindleSpeed = GetActualSpindleSpeed();
                if (spindleSpeed <= 0)
                {
                    _log("Spindle stopped");
                    CurrentState = ToolChangeState.SavePosition;
                    return;
                }
                Wait(checkInterval);
                elapsed += checkInterval;
            }

            LastError = "Spindle did not stop in time";
            CurrentState = ToolChangeState.Fault;
        }

        private void ExecuteSavePosition()
        {
            OriginalX = GetMachineX();
            OriginalY = GetMachineY();
            OriginalZ = GetMachineZ();
            _log($"Saved position: X{F(OriginalX)} Y{F(OriginalY)} Z{F(OriginalZ)}");
            CurrentState = ToolChangeState.MoveSafeZ;
        }

        private void ExecuteMoveSafeZ()
        {
            _log($"Moving to safe Z: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DetermineSlots;
        }

        private void ExecuteDetermineSlots()
        {
            OldSlot = 0;
            NewSlot = 0;

            if (CurrentTool > 0)
            {
                OldSlot = _config.FindToolSlot(CurrentTool);
                if (OldSlot == 0)
                {
                    OldSlot = _config.FindFirstEmptySlot();
                    _log($"Current tool T{CurrentTool} not in rack, will use empty slot {OldSlot}");
                }
                else
                {
                    _log($"Current tool T{CurrentTool} has slot {OldSlot}");
                }
            }

            if (NewTool > 0)
            {
                NewSlot = _config.FindToolSlot(NewTool);
                _log(NewSlot > 0 
                    ? $"New tool T{NewTool} found in slot {NewSlot}" 
                    : $"New tool T{NewTool} not in rack, manual load required");
            }

            if (CurrentTool > 0 && OldSlot > 0)
            {
                CurrentState = ToolChangeState.DropToolMoveXY;
            }
            else if (CurrentTool > 0 && OldSlot == 0)
            {
                CurrentState = ToolChangeState.ManualUnload;
            }
            else if (NewTool > 0)
            {
                CurrentState = ToolChangeState.PickToolMoveXY;
            }
            else
            {
                CurrentState = ToolChangeState.Complete;
            }
        }

        private void ExecuteDropToolMoveXY()
        {
            string axis = _config.Rack.Axis;
            double slotPos = _config.GetSlotPosition(OldSlot);
            
            _log($"Moving to drop position: {axis}{F(slotPos)}");
            ExecuteGCode($"G00 G53 {axis}{F(slotPos)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            string axis2 = _config.Rack.AxisSecondary;
            _log($"Moving to approach: {axis2}{F(_config.Heights.ZApproach)}");
            ExecuteGCode($"G00 G53 {axis2}{F(_config.Heights.ZApproach)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            CurrentState = ToolChangeState.DropVerifyEmpty;
        }

        private void ExecuteDropVerifyEmpty()
        {
            if (!VerifySlotState(OldSlot, false))
            {
                LastError = $"Slot {OldSlot} should be empty but sensor reports occupied";
                CurrentState = ToolChangeState.Fault;
                return;
            }
            CurrentState = ToolChangeState.DropAirBlowOn;
        }

        private void ExecuteDropAirBlowOn()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                _log("Air blow ON");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, true);
            }
            CurrentState = ToolChangeState.DropLowerToClamp;
        }

        private void ExecuteDropLowerToClamp()
        {
            _log($"Lowering to clamp Z: {F(_config.Heights.ZClamp)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZClamp)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DropEnterFork;
        }

        private void ExecuteDropEnterFork()
        {
            string axis2 = _config.Rack.AxisSecondary;
            double forkY = _config.Rack.Slot1PositionSecondary;
            
            _log($"Entering fork: {axis2}{F(forkY)}");
            ExecuteGCode($"G01 G53 {axis2}{F(forkY)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DropOpenDrawbar;
        }

        private void ExecuteDropOpenDrawbar()
        {
            _log("Opening drawbar");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, true);
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DropRaiseToCheck;
        }

        private void ExecuteDropRaiseToCheck()
        {
            if (Math.Abs(_config.Heights.ZCheck - _config.Heights.ZClamp) > 0.001)
            {
                _log($"Raising to check Z: {F(_config.Heights.ZCheck)}");
                ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZCheck)} F{F(_config.FeedsAndTiming.FeedRate)}");
                WaitForMotionComplete();
                Wait(_config.FeedsAndTiming.OperationDelay);
            }
            CurrentState = ToolChangeState.DropVerifyToolInSlot;
        }

        private void ExecuteDropVerifyToolInSlot()
        {
            if (!VerifySlotState(OldSlot, true))
            {
                LastError = $"Slot {OldSlot} should have tool but sensor reports empty";
                CurrentState = ToolChangeState.Fault;
                return;
            }
            CurrentState = ToolChangeState.DropAirBlowOff;
        }

        private void ExecuteDropAirBlowOff()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                _log("Air blow OFF");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
            CurrentState = ToolChangeState.DropRaiseToEmpty;
        }

        private void ExecuteDropRaiseToEmpty()
        {
            _log($"Raising to empty Z: {F(_config.Heights.ZEmpty)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZEmpty)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DropCloseDrawbar;
        }

        private void ExecuteDropCloseDrawbar()
        {
            _log("Closing drawbar");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);

            var slot = _config.GetSlot(OldSlot);
            if (slot != null)
            {
                slot.ToolNumber = CurrentTool;
            }
            SetCurrentTool(0);
            
            _log($"Tool T{CurrentTool} dropped in slot {OldSlot}");
            CurrentTool = 0;

            CurrentState = NewTool > 0 ? ToolChangeState.PickToolMoveXY : ToolChangeState.Complete;
        }

        private void ExecutePickToolMoveXY()
        {
            if (NewSlot == 0)
            {
                CurrentState = ToolChangeState.ManualLoad;
                return;
            }

            _log($"Raising to empty Z for pickup: {F(_config.Heights.ZEmpty)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZEmpty)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            string axis = _config.Rack.Axis;
            double slotPos = _config.GetSlotPosition(NewSlot);
            
            _log($"Moving to pick position: {axis}{F(slotPos)}");
            ExecuteGCode($"G00 G53 {axis}{F(slotPos)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            string axis2 = _config.Rack.AxisSecondary;
            double forkY = _config.Rack.Slot1PositionSecondary;
            _log($"Moving to slot: {axis2}{F(forkY)}");
            ExecuteGCode($"G00 G53 {axis2}{F(forkY)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            CurrentState = ToolChangeState.PickVerifyToolPresent;
        }

        private void ExecutePickVerifyToolPresent()
        {
            if (!VerifySlotState(NewSlot, true))
            {
                LastError = $"Slot {NewSlot} should have tool T{NewTool} but sensor reports empty";
                CurrentState = ToolChangeState.Fault;
                return;
            }
            CurrentState = ToolChangeState.PickAirBlowOn;
        }

        private void ExecutePickAirBlowOn()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                _log("Air blow ON");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, true);
            }
            
            _log("Opening drawbar for pickup");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, true);
            
            CurrentState = ToolChangeState.PickLowerToClamp;
        }

        private void ExecutePickLowerToClamp()
        {
            _log($"Lowering to clamp Z: {F(_config.Heights.ZClamp)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZClamp)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickCloseDrawbar;
        }

        private void ExecutePickCloseDrawbar()
        {
            _log("Closing drawbar to grab tool");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);
            
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                _log("Air blow OFF");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
            
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickRaiseToCheck;
        }

        private void ExecutePickRaiseToCheck()
        {
            if (Math.Abs(_config.Heights.ZCheck - _config.Heights.ZClamp) > 0.001)
            {
                _log($"Raising to check Z: {F(_config.Heights.ZCheck)}");
                ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZCheck)} F{F(_config.FeedsAndTiming.FeedRate)}");
                WaitForMotionComplete();
                Wait(_config.FeedsAndTiming.OperationDelay);
            }
            CurrentState = ToolChangeState.PickVerifySlotEmpty;
        }

        private void ExecutePickVerifySlotEmpty()
        {
            if (!VerifySlotState(NewSlot, false))
            {
                LastError = $"Slot {NewSlot} should be empty after pickup but sensor reports occupied";
                CurrentState = ToolChangeState.Fault;
                return;
            }
            CurrentState = ToolChangeState.PickExitFork;
        }

        private void ExecutePickExitFork()
        {
            string axis2 = _config.Rack.AxisSecondary;
            _log($"Exiting fork: {axis2}{F(_config.Heights.ZApproach)}");
            ExecuteGCode($"G01 G53 {axis2}{F(_config.Heights.ZApproach)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickRaiseToSafe;
        }

        private void ExecutePickRaiseToSafe()
        {
            _log($"Raising to safe Z: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            var slot = _config.GetSlot(NewSlot);
            if (slot != null)
            {
                slot.ToolNumber = 0;
            }
            SetCurrentTool(NewTool);
            CurrentTool = NewTool;
            
            _log($"Tool T{NewTool} picked from slot {NewSlot}");
            
            CurrentState = ToolChangeState.MeasureIfNeeded;
        }

        private void ExecuteMeasureIfNeeded()
        {
            CurrentState = ToolChangeState.ApplyOffset;
        }

        private void ExecuteApplyOffset()
        {
            if (_config.Options.ApplyG43 && NewTool > 0)
            {
                _log($"Applying tool offset: G43 H{NewTool}");
                ExecuteGCode($"G43 H{NewTool}");
            }
            else
            {
                _log("Canceling tool offset: G49");
                ExecuteGCode("G49");
            }
            CurrentState = ToolChangeState.RestorePosition;
        }

        private void ExecuteRestorePosition()
        {
            if (_config.Options.ReturnToXY)
            {
                _log($"Returning to original XY: X{F(OriginalX)} Y{F(OriginalY)}");
                ExecuteGCode($"G00 G53 X{F(OriginalX)} Y{F(OriginalY)}");
                WaitForMotionComplete();
            }
            CurrentState = ToolChangeState.Complete;
        }

        private void ExecuteComplete()
        {
            _log($"Tool change complete: T{NewTool}");
            CurrentState = ToolChangeState.Idle;
        }

        private void ExecuteAbort()
        {
            _log("Tool change ABORTED by user");
            SafetyShutdown();
            CurrentState = ToolChangeState.Idle;
        }

        private void ExecuteFault()
        {
            _log($"Tool change FAULT: {LastError}");
            SafetyShutdown();
            
            try
            {
                _uc.Stop();
            }
            catch { }
            
            CurrentState = ToolChangeState.Idle;
        }

        private void SafetyShutdown()
        {
            _log("Safety shutdown: closing outputs");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);
            if (_config.IO.AirBlow.Enabled)
            {
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
        }

        private bool VerifySlotState(int slotNumber, bool expectedOccupied)
        {
            if (!_config.IO.SlotSensors.Enabled)
                return true;

            if (_isSimulation())
            {
                bool simState = _config.Simulation.SlotStates[slotNumber - 1];
                return simState == expectedOccupied;
            }

            int pin = _config.IO.SlotSensors.GetPinForSlot(slotNumber);
            int ledNum = GetLEDForPin(_config.IO.SlotSensors.Port, pin);
            
            int elapsed = 0;
            int debounceRequired = 1000;
            int debounceCount = 0;
            
            while (elapsed < _config.FeedsAndTiming.SensorWaitTime)
            {
                bool sensorState = GetLED(ledNum);
                bool slotOccupied = !sensorState;
                
                if (slotOccupied == expectedOccupied)
                {
                    debounceCount += 100;
                    if (debounceCount >= debounceRequired)
                        return true;
                }
                else
                {
                    debounceCount = 0;
                }
                
                Wait(100);
                elapsed += 100;
            }
            
            return false;
        }

        private string F(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
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

        private bool CheckAxesHomed()
        {
            if (_isSimulation()) return true;
            return GetLED(56) && GetLED(57) && GetLED(58);
        }

        private bool CheckAirPressure()
        {
            if (_isSimulation()) return _config.Simulation.PressureOK;
            int ledNum = GetLEDForPin(_config.IO.PressureSensor.Port, _config.IO.PressureSensor.Pin);
            return !GetLED(ledNum);
        }

        private bool CheckMacroStopped()
        {
            if (_isSimulation()) return false;
            try { return _uc.Ismacrostopped(); }
            catch { return false; }
        }

        private int GetCurrentTool()
        {
            try { return _uc.Getcurrenttool(); }
            catch { return 0; }
        }

        private void SetCurrentTool(int tool)
        {
            try { _uc.Setcurrenttool(tool); }
            catch { }
        }

        private double GetActualSpindleSpeed()
        {
            if (_isSimulation()) return 0;
            try
            {
                string val = _uc.Getfield(true, 870);
                if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double speed))
                    return speed;
            }
            catch { }
            return 0;
        }

        private int GetSpindleWaitTime()
        {
            try
            {
                string val = _uc.Getfield(true, 2761);
                if (int.TryParse(val, out int time) && time > 0)
                    return time;
            }
            catch { }
            return 10;
        }

        private double GetMachineX()
        {
            if (_isSimulation()) return 0;
            try { return _uc.GetXmachpos(); }
            catch { return 0; }
        }

        private double GetMachineY()
        {
            if (_isSimulation()) return 0;
            try { return _uc.GetYmachpos(); }
            catch { return 0; }
        }

        private double GetMachineZ()
        {
            if (_isSimulation()) return 0;
            try { return _uc.GetZmachpos(); }
            catch { return 0; }
        }

        private bool GetLED(int ledNum)
        {
            if (_isSimulation()) return false;
            try { return _uc.GetLED(ledNum); }
            catch { return false; }
        }

        private void SetOutput(int port, int pin, bool state)
        {
            if (_isSimulation())
            {
                _log($"[SIM] Output Port{port} Pin{pin} = {(state ? "ON" : "OFF")}");
                return;
            }
            try
            {
                if (state)
                    _uc.Setoutpin(port, pin);
                else
                    _uc.Clroutpin(port, pin);
            }
            catch { }
        }

        private void ExecuteGCode(string code)
        {
            if (_isSimulation())
            {
                _log($"[SIM] G-code: {code}");
                return;
            }
            try { _uc.Code(code); }
            catch { }
        }

        private void WaitForMotionComplete()
        {
            if (_isSimulation()) return;
            try
            {
                while (_uc.IsMoving())
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch { }
        }

        private void Wait(int ms)
        {
            if (ms > 0)
                System.Threading.Thread.Sleep(ms);
        }
    }
}
