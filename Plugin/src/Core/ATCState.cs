using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Plugins.Forms;

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
        DropMoveToSlotXY,
        DropLowerToApproach,
        DropEnterFork,
        DropLowerToClamp,
        DropOpenDrawbar,
        DropRaiseToCheck,
        DropVerifyToolInSlot,
        DropAirBlowOff,
        DropRaiseToEmpty,
        DropCloseDrawbar,
        DropExitFork,
        PickMoveToSlotXY,
        PickLowerToApproach,
        PickEnterFork,
        PickVerifyToolPresent,
        PickAirBlowOn,
        PickOpenDrawbar,
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
        ManualUnloadMoveToTCP,
        ManualUnloadDialog,
        ManualLoadMoveToTCP,
        ManualLoadDialog,
        Abort,
        Fault
    }

    public class ATCStateMachine
    {
        private readonly ATCConfig _config;
        private readonly Plugininterface.Entry _uc;
        private readonly ToolTableManager _toolTable;
        
        private readonly object _stateLock = new object();

        public ToolChangeState CurrentState { get; private set; } = ToolChangeState.Idle;
        public int CurrentTool { get; private set; }
        public int NewTool { get; private set; }
        public int OldSlot { get; private set; }
        public int NewSlot { get; private set; }
        public double OriginalX { get; private set; }
        public double OriginalY { get; private set; }
        public double OriginalZ { get; private set; }
        public string LastError { get; private set; }
        public bool IsRunning => CurrentState != ToolChangeState.Idle;
        
        private volatile bool _stopRequested;
        private bool _assignedNewSlotToCurrent;

        public ATCStateMachine(ATCConfig config, Plugininterface.Entry uc)
        {
            _config = config;
            _uc = uc;
            _toolTable = new ToolTableManager(uc);
        }
        
        public ToolTableManager ToolTable => _toolTable;

        private void Log(string message)
        {
            PluginLogger.LogToolChange(message);
        }

        private bool IsSimulationMode()
        {
            return _config.Simulation.Enabled;
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

        public string StartToolChange(int newTool)
        {
            lock (_stateLock)
            {
                if (IsRunning)
                    return "Tool change already in progress";

                NewTool = newTool;
                CurrentState = ToolChangeState.ValidateRequest;
                LastError = null;
                _stopRequested = false;
                _assignedNewSlotToCurrent = false;
            }

            return RunStateMachine();
        }

        private string RunStateMachine()
        {
            while (CurrentState != ToolChangeState.Idle)
            {
                var previousState = CurrentState;
                
                if (CheckStopRequested())
                {
                    CurrentState = ToolChangeState.Abort;
                }
                
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
                        case ToolChangeState.DropMoveToSlotXY:
                            ExecuteDropMoveToSlotXY();
                            break;
                        case ToolChangeState.DropLowerToApproach:
                            ExecuteDropLowerToApproach();
                            break;
                        case ToolChangeState.DropEnterFork:
                            ExecuteDropEnterFork();
                            break;
                        case ToolChangeState.DropLowerToClamp:
                            ExecuteDropLowerToClamp();
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
                        case ToolChangeState.DropExitFork:
                            ExecuteDropExitFork();
                            break;
                        case ToolChangeState.PickMoveToSlotXY:
                            ExecutePickMoveToSlotXY();
                            break;
                        case ToolChangeState.PickLowerToApproach:
                            ExecutePickLowerToApproach();
                            break;
                        case ToolChangeState.PickEnterFork:
                            ExecutePickEnterFork();
                            break;
                        case ToolChangeState.PickVerifyToolPresent:
                            ExecutePickVerifyToolPresent();
                            break;
                        case ToolChangeState.PickAirBlowOn:
                            ExecutePickAirBlowOn();
                            break;
                        case ToolChangeState.PickOpenDrawbar:
                            ExecutePickOpenDrawbar();
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
                        case ToolChangeState.ManualUnloadMoveToTCP:
                            ExecuteManualUnloadMoveToTCP();
                            break;
                        case ToolChangeState.ManualUnloadDialog:
                            ExecuteManualUnloadDialog();
                            break;
                        case ToolChangeState.ManualLoadMoveToTCP:
                            ExecuteManualLoadMoveToTCP();
                            break;
                        case ToolChangeState.ManualLoadDialog:
                            ExecuteManualLoadDialog();
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
                    Log($"State machine error in {previousState}: {ex.Message}");
                    CurrentState = ToolChangeState.Fault;
                }
            }

            return LastError;
        }

        private void ExecuteValidateRequest()
        {
            Log($"Validating tool change request: T{NewTool}");

            bool demoMode = IsDemoMode();
            
            if (!demoMode && !CheckAxesHomed())
            {
                LastError = "One or more axes are NOT REFERENCED";
                CurrentState = ToolChangeState.Fault;
                return;
            }
            
            if (demoMode)
            {
                Log("Demo mode: skipping homing check");
            }

            if (_config.IO.PressureSensor.Enabled && !IsSimulationMode() && !demoMode)
            {
                if (!CheckAirPressure())
                {
                    LastError = "No Air Pressure";
                    CurrentState = ToolChangeState.Fault;
                    return;
                }
            }
            else if (_config.IO.PressureSensor.Enabled)
            {
                Log("Demo/Simulation: skipping pressure check");
            }

            CurrentTool = GetCurrentTool();

            if (NewTool == CurrentTool)
            {
                Log($"Requested tool T{NewTool} already loaded");
                CurrentState = ToolChangeState.ApplyOffset;
                return;
            }

            CurrentState = ToolChangeState.StopSpindle;
        }

        private void ExecuteStopSpindle()
        {
            Log("Stopping spindle (M5, M9)");
            
            ExecuteGCode("M5");
            ExecuteGCode("M9");

            if (!IsSimulationMode())
            {
                int waitTimeMs = GetSpindleWaitTime() * 1000;
                int elapsed = 0;
                const int checkInterval = 100;

                while (elapsed < waitTimeMs)
                {
                    if (CheckStopRequested())
                    {
                        CurrentState = ToolChangeState.Abort;
                        return;
                    }
                    
                    double spindleSpeed = GetActualSpindleSpeed();
                    if (spindleSpeed <= 0)
                    {
                        Log("Spindle stopped");
                        break;
                    }
                    Thread.Sleep(checkInterval);
                    elapsed += checkInterval;
                }
                
                if (elapsed >= waitTimeMs)
                {
                    Log("Warning: Spindle stop timeout, continuing anyway");
                }
            }
            else
            {
                Log("[SIM] Spindle stop simulated");
            }
            
            CurrentState = ToolChangeState.SavePosition;
        }

        private void ExecuteSavePosition()
        {
            OriginalX = GetMachineX();
            OriginalY = GetMachineY();
            OriginalZ = GetMachineZ();
            Log($"Saved position: X{F(OriginalX)} Y{F(OriginalY)} Z{F(OriginalZ)}");
            CurrentState = ToolChangeState.MoveSafeZ;
        }

        private void ExecuteMoveSafeZ()
        {
            Log($"Moving to safe Z: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DetermineSlots;
        }

        private void ExecuteDetermineSlots()
        {
            OldSlot = 0;
            NewSlot = 0;
            _assignedNewSlotToCurrent = false;

            Log($"[T{Thread.CurrentThread.ManagedThreadId}] DetermineSlots: CurrentTool=T{CurrentTool}, NewTool=T{NewTool}");

            if (CurrentTool > 0)
            {
                OldSlot = _toolTable.FindToolSlot(CurrentTool);
                
                if (OldSlot > 0 && _config.GetSlot(OldSlot)?.Enabled == true)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] Current tool T{CurrentTool} has own enabled slot {OldSlot}");
                }
                else if (OldSlot > 0 && _config.GetSlot(OldSlot)?.Enabled != true)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] Current tool T{CurrentTool} slot {OldSlot} is disabled, finding free slot");
                    OldSlot = 0;
                }
                
                if (OldSlot == 0)
                {
                    int freeSlot = _config.FindFirstEmptySlot();
                    if (freeSlot > 0)
                    {
                        OldSlot = freeSlot;
                        _assignedNewSlotToCurrent = true;
                        Log($"[T{Thread.CurrentThread.ManagedThreadId}] Current tool T{CurrentTool} assigned to first free slot {OldSlot}");
                    }
                    else
                    {
                        Log($"[T{Thread.CurrentThread.ManagedThreadId}] No free slot for current tool T{CurrentTool}, manual unload required");
                    }
                }
            }

            if (NewTool > 0)
            {
                NewSlot = _toolTable.FindToolSlot(NewTool);
                if (NewSlot > 0 && _config.GetSlot(NewSlot)?.Enabled == true)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] New tool T{NewTool} found in enabled slot {NewSlot}");
                }
                else if (NewSlot > 0 && _config.GetSlot(NewSlot)?.Enabled != true)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] New tool T{NewTool} slot {NewSlot} is disabled, manual load required");
                    NewSlot = 0;
                }
                else
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] New tool T{NewTool} not in rack, manual load required");
                }
            }

            if (CurrentTool > 0 && OldSlot > 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Decision: DROP current tool T{CurrentTool} to slot {OldSlot}");
                CurrentState = ToolChangeState.DropMoveToSlotXY;
            }
            else if (CurrentTool > 0 && OldSlot == 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Decision: MANUAL UNLOAD current tool T{CurrentTool} (no slot available)");
                CurrentState = ToolChangeState.ManualUnloadMoveToTCP;
            }
            else if (NewTool > 0 && NewSlot > 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Decision: PICK new tool T{NewTool} from slot {NewSlot}");
                CurrentState = ToolChangeState.PickMoveToSlotXY;
            }
            else if (NewTool > 0 && NewSlot == 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Decision: MANUAL LOAD new tool T{NewTool} (not in rack)");
                CurrentState = ToolChangeState.ManualLoadMoveToTCP;
            }
            else
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Decision: COMPLETE (nothing to do)");
                CurrentState = ToolChangeState.Complete;
            }
        }

        private void ExecuteDropMoveToSlotXY()
        {
            string rackAxis = _config.Rack.Axis;
            string secondaryAxis = _config.Rack.AxisSecondary;
            double slotPos = _config.GetSlotPosition(OldSlot);
            double approachPos = _config.Rack.ForkApproachPosition;
            
            Log($"Moving to slot {OldSlot}: {rackAxis}{F(slotPos)}, approach {secondaryAxis}{F(approachPos)}");
            ExecuteGCode($"G00 G53 {rackAxis}{F(slotPos)} {secondaryAxis}{F(approachPos)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            CurrentState = ToolChangeState.DropLowerToApproach;
        }

        private void ExecuteDropLowerToApproach()
        {
            Log($"Lowering to approach Z: {F(_config.Heights.ZApproach)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZApproach)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            if (_config.IO.SlotSensors.Enabled)
            {
                bool slotOccupied = CheckSlotSensor(OldSlot);
                if (slotOccupied)
                {
                    LastError = $"Slot {OldSlot} should be empty but sensor reports occupied";
                    CurrentState = ToolChangeState.Fault;
                    return;
                }
            }
            
            CurrentState = ToolChangeState.DropEnterFork;
        }

        private void ExecuteDropEnterFork()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                Log("Air blow ON");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, true);
            }
            
            string secondaryAxis = _config.Rack.AxisSecondary;
            double forkPos = _config.Rack.Slot1PositionSecondary;
            
            Log($"Entering fork: {secondaryAxis}{F(forkPos)}");
            ExecuteGCode($"G01 G53 {secondaryAxis}{F(forkPos)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.DropLowerToClamp;
        }

        private void ExecuteDropLowerToClamp()
        {
            Log($"Lowering to clamp Z: {F(_config.Heights.ZClamp)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZClamp)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.DropOpenDrawbar;
        }

        private void ExecuteDropOpenDrawbar()
        {
            Log("Opening drawbar");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, true);
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            if (IsSimulationMode())
            {
                Log("[SIM] Marking slot as occupied (tool dropped)");
                if (OldSlot >= 1 && OldSlot <= 10)
                {
                    _config.Simulation.SlotStates[OldSlot - 1] = true;
                }
            }
            
            CurrentState = ToolChangeState.DropRaiseToCheck;
        }

        private void ExecuteDropRaiseToCheck()
        {
            if (Math.Abs(_config.Heights.ZCheck - _config.Heights.ZClamp) > 0.001)
            {
                Log($"Raising to check Z: {F(_config.Heights.ZCheck)}");
                ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZCheck)} F{F(_config.FeedsAndTiming.FeedRate)}");
                WaitForMotionComplete();
                Wait(_config.FeedsAndTiming.OperationDelay);
            }
            CurrentState = ToolChangeState.DropVerifyToolInSlot;
        }

        private void ExecuteDropVerifyToolInSlot()
        {
            if (_config.IO.SlotSensors.Enabled)
            {
                bool slotOccupied = VerifySlotStateWithRetry(OldSlot, true);
                if (!slotOccupied)
                {
                    LastError = $"Slot {OldSlot} should have tool but sensor reports empty";
                    CurrentState = ToolChangeState.Fault;
                    return;
                }
                Log($"Slot {OldSlot} verified: tool present");
            }
            CurrentState = ToolChangeState.DropAirBlowOff;
        }

        private void ExecuteDropAirBlowOff()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                Log("Air blow OFF");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
            CurrentState = ToolChangeState.DropRaiseToEmpty;
        }

        private void ExecuteDropRaiseToEmpty()
        {
            Log($"Raising to empty Z: {F(_config.Heights.ZEmpty)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZEmpty)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.DropCloseDrawbar;
        }

        private void ExecuteDropCloseDrawbar()
        {
            Log("Closing drawbar");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);

            var slot = _config.GetSlot(OldSlot);
            if (slot != null)
            {
                slot.ToolNumber = CurrentTool;
            }
            
            if (_assignedNewSlotToCurrent)
            {
                _toolTable.WriteToolSlot(CurrentTool, OldSlot);
                _toolTable.SaveToolTable();
                Log($"Assigned slot {OldSlot} to tool T{CurrentTool} in tooltable");
            }
            
            SetCurrentTool(0);
            
            Log($"Tool T{CurrentTool} dropped in slot {OldSlot}");
            CurrentTool = 0;

            CurrentState = ToolChangeState.DropExitFork;
        }

        private void ExecuteDropExitFork()
        {
            string secondaryAxis = _config.Rack.AxisSecondary;
            double approachPos = _config.Rack.ForkApproachPosition;
            
            Log($"Exiting fork: {secondaryAxis}{F(approachPos)}");
            ExecuteGCode($"G01 G53 {secondaryAxis}{F(approachPos)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            if (NewTool > 0 && NewSlot > 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] After drop: proceed to pick T{NewTool} from slot {NewSlot}");
                CurrentState = ToolChangeState.PickMoveToSlotXY;
            }
            else if (NewTool > 0 && NewSlot == 0)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] After drop: proceed to manual load T{NewTool}");
                CurrentState = ToolChangeState.ManualLoadMoveToTCP;
            }
            else
            {
                CurrentState = ToolChangeState.Complete;
            }
        }

        private void ExecutePickMoveToSlotXY()
        {
            if (NewSlot == 0)
            {
                CurrentState = ToolChangeState.ManualLoadMoveToTCP;
                return;
            }

            Log($"Moving to safe Z for pickup: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            string rackAxis = _config.Rack.Axis;
            string secondaryAxis = _config.Rack.AxisSecondary;
            double slotPos = _config.GetSlotPosition(NewSlot);
            double approachPos = _config.Rack.ForkApproachPosition;
            
            Log($"Moving to slot {NewSlot}: {rackAxis}{F(slotPos)}, approach {secondaryAxis}{F(approachPos)}");
            ExecuteGCode($"G00 G53 {rackAxis}{F(slotPos)} {secondaryAxis}{F(approachPos)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            CurrentState = ToolChangeState.PickLowerToApproach;
        }

        private void ExecutePickLowerToApproach()
        {
            Log($"Lowering to approach Z: {F(_config.Heights.ZApproach)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZApproach)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.PickEnterFork;
        }

        private void ExecutePickEnterFork()
        {
            string secondaryAxis = _config.Rack.AxisSecondary;
            double forkPos = _config.Rack.Slot1PositionSecondary;
            
            Log($"Entering fork: {secondaryAxis}{F(forkPos)}");
            ExecuteGCode($"G01 G53 {secondaryAxis}{F(forkPos)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);

            CurrentState = ToolChangeState.PickVerifyToolPresent;
        }

        private void ExecutePickVerifyToolPresent()
        {
            if (_config.IO.SlotSensors.Enabled)
            {
                bool slotOccupied = VerifySlotStateWithRetry(NewSlot, true);
                if (!slotOccupied)
                {
                    LastError = $"Slot {NewSlot} should have tool T{NewTool} but sensor reports empty";
                    CurrentState = ToolChangeState.Fault;
                    return;
                }
                Log($"Slot {NewSlot} verified: tool present");
            }
            CurrentState = ToolChangeState.PickAirBlowOn;
        }

        private void ExecutePickAirBlowOn()
        {
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                Log("Air blow ON");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, true);
            }
            CurrentState = ToolChangeState.PickOpenDrawbar;
        }

        private void ExecutePickOpenDrawbar()
        {
            Log("Opening drawbar for pickup");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, true);
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.PickLowerToClamp;
        }

        private void ExecutePickLowerToClamp()
        {
            Log($"Lowering to clamp Z: {F(_config.Heights.ZClamp)}");
            ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZClamp)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickCloseDrawbar;
        }

        private void ExecutePickCloseDrawbar()
        {
            Log("Closing drawbar to grab tool");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);
            
            if (IsSimulationMode())
            {
                Log("[SIM] Marking slot as empty (tool picked)");
                if (NewSlot >= 1 && NewSlot <= 10)
                {
                    _config.Simulation.SlotStates[NewSlot - 1] = false;
                }
            }
            
            if (_config.IO.AirBlow.Enabled && _config.Options.AirBlowDuringTC)
            {
                Log("Air blow OFF");
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
            
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickRaiseToCheck;
        }

        private void ExecutePickRaiseToCheck()
        {
            if (Math.Abs(_config.Heights.ZCheck - _config.Heights.ZClamp) > 0.001)
            {
                Log($"Raising to check Z: {F(_config.Heights.ZCheck)}");
                ExecuteGCode($"G01 G53 Z{F(_config.Heights.ZCheck)} F{F(_config.FeedsAndTiming.FeedRate)}");
                WaitForMotionComplete();
                Wait(_config.FeedsAndTiming.OperationDelay);
            }
            CurrentState = ToolChangeState.PickVerifySlotEmpty;
        }

        private void ExecutePickVerifySlotEmpty()
        {
            if (_config.IO.SlotSensors.Enabled)
            {
                bool slotEmpty = VerifySlotStateWithRetry(NewSlot, false);
                if (!slotEmpty)
                {
                    LastError = $"Slot {NewSlot} should be empty after pickup but sensor reports occupied";
                    CurrentState = ToolChangeState.Fault;
                    return;
                }
                Log($"Slot {NewSlot} verified: empty");
            }
            CurrentState = ToolChangeState.PickExitFork;
        }

        private void ExecutePickExitFork()
        {
            string secondaryAxis = _config.Rack.AxisSecondary;
            double approachPos = _config.Rack.ForkApproachPosition;
            
            Log($"Exiting fork: {secondaryAxis}{F(approachPos)}");
            ExecuteGCode($"G01 G53 {secondaryAxis}{F(approachPos)} F{F(_config.FeedsAndTiming.FeedRate)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            CurrentState = ToolChangeState.PickRaiseToSafe;
        }

        private void ExecutePickRaiseToSafe()
        {
            Log($"Raising to safe Z: {F(_config.Heights.ZSafe)}");
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
            
            Log($"Tool T{NewTool} picked from slot {NewSlot}");
            
            CurrentState = ToolChangeState.MeasureIfNeeded;
        }

        private void ExecuteMeasureIfNeeded()
        {
            if (!_config.Options.MeasureNonRH || NewTool <= 0)
            {
                CurrentState = ToolChangeState.ApplyOffset;
                return;
            }
            
            var toolInfo = _toolTable.ReadToolInfo(NewTool);
            if (toolInfo == null || toolInfo.Type == "RH")
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] Tool T{NewTool} is RH or info unavailable, skipping measurement");
                CurrentState = ToolChangeState.ApplyOffset;
                return;
            }
            
            Log($"[T{Thread.CurrentThread.ManagedThreadId}] Tool T{NewTool} is NRH, measuring tool length...");
            AddStatusMessage($"Measuring tool T{NewTool} length...");
            
            if (!PerformToolMeasurement())
            {
                CurrentState = ToolChangeState.ApplyOffset;
                return;
            }
            
            CurrentState = ToolChangeState.ApplyOffset;
        }
        
        private bool PerformToolMeasurement()
        {
            try
            {
                double fastFeed = _config.Probe.FastFeed;
                double slowFeed = _config.Probe.SlowFeed;
                double retract = _config.Probe.Retract;
                bool dualCycle = _config.Probe.DualCycle;
                double minZ = _config.Probe.MinZ;
                
                if (_config.Probe.UseUCCNCSettings)
                {
                    fastFeed = GetFieldDouble(2709, 500);
                    slowFeed = GetFieldDouble(2710, 50);
                    retract = GetFieldDouble(2706, 2);
                    dualCycle = GetButton(853);
                    minZ = GetFieldDouble(41, -200);
                }
                
                Log($"Probe settings: FastFeed={fastFeed}, SlowFeed={slowFeed}, Retract={retract}, DualCycle={dualCycle}, MinZ={minZ}");
                
                if (IsSimulationMode())
                {
                    Log("[SIM] Tool measurement simulated - no actual probe");
                    AddStatusMessage($"[SIM] Tool T{NewTool} measured (simulated)");
                    return true;
                }
                
                Log($"Moving to safe Z: {F(_config.Heights.ZSafe)}");
                ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
                WaitForMotionComplete();
                
                Log($"Moving to TCP for measurement: X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
                ExecuteGCode($"G00 G53 X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
                WaitForMotionComplete();
                
                Log($"Probing with G31 to Z{F(minZ)} F{F(fastFeed)}");
                ExecuteGCode($"G31 G53 Z{F(minZ)} F{F(fastFeed)}");
                WaitForMotionComplete();
                
                if (!GetLED(244))
                {
                    Log("WARNING: First probe did not trigger (LED 244 not set)");
                    AddStatusMessage($"Tool T{NewTool} probe failed - sensor not triggered");
                    return false;
                }
                
                double probedZ = GetMachineZ();
                Log($"First probe hit at Z{F(probedZ)}");
                
                if (dualCycle)
                {
                    Log($"Retracting {retract}mm for second probe");
                    ExecuteGCode($"G00 G91 Z{F(retract)}");
                    WaitForMotionComplete();
                    ExecuteGCode("G90");
                    
                    Log($"Second probe at slow feed F{F(slowFeed)}");
                    ExecuteGCode($"G31 G53 Z{F(minZ)} F{F(slowFeed)}");
                    WaitForMotionComplete();
                    
                    if (!GetLED(244))
                    {
                        Log("WARNING: Second probe did not trigger");
                        AddStatusMessage($"Tool T{NewTool} slow probe failed");
                        return false;
                    }
                    
                    probedZ = GetMachineZ();
                    Log($"Second probe hit at Z{F(probedZ)}");
                }
                
                double toolLength = Math.Abs(probedZ - _config.Heights.ZSafe);
                Log($"Calculated tool length: {F(toolLength)}");
                
                _toolTable.WriteToolInfo(NewTool, 
                    _toolTable.ReadToolInfo(NewTool)?.Diameter ?? 0,
                    toolLength,
                    "NRH");
                _toolTable.SaveToolTable();
                
                Log($"Tool T{NewTool} length {F(toolLength)} written to tooltable");
                AddStatusMessage($"Tool T{NewTool} measured: L={toolLength:F3}");
                
                Log($"Raising to safe Z after measurement");
                ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
                WaitForMotionComplete();
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"Tool measurement error: {ex.Message}");
                AddStatusMessage($"Tool T{NewTool} measurement error: {ex.Message}");
                return false;
            }
        }
        
        private double GetFieldDouble(int fieldNum, double defaultValue)
        {
            try
            {
                string val = _uc.Getfield(true, fieldNum);
                if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    return d;
            }
            catch { }
            return defaultValue;
        }
        
        private bool GetButton(int buttonNum)
        {
            try
            {
                return _uc.Getbuttonstate(buttonNum);
            }
            catch { }
            return false;
        }
        
        private void AddStatusMessage(string message)
        {
            try
            {
                _uc.AddStatusmessage(message);
            }
            catch { }
        }

        private void ExecuteApplyOffset()
        {
            if (_config.Options.ApplyG43 && NewTool > 0)
            {
                Log($"Applying tool offset: G43 H{NewTool}");
                ExecuteGCode($"G43 H{NewTool}");
            }
            else
            {
                Log("Canceling tool offset: G49");
                ExecuteGCode("G49");
            }
            CurrentState = ToolChangeState.RestorePosition;
        }

        private void ExecuteRestorePosition()
        {
            if (_config.Options.ReturnToXY)
            {
                Log($"Returning to original XY: X{F(OriginalX)} Y{F(OriginalY)}");
                ExecuteGCode($"G00 G53 X{F(OriginalX)} Y{F(OriginalY)}");
                WaitForMotionComplete();
            }
            CurrentState = ToolChangeState.Complete;
        }

        private void ExecuteComplete()
        {
            Log($"Tool change complete: T{NewTool}");
            CurrentState = ToolChangeState.Idle;
        }

        private void ExecuteManualUnloadMoveToTCP()
        {
            Log($"[T{Thread.CurrentThread.ManagedThreadId}] Manual unload: no slot for T{CurrentTool}, moving to TCP");
            
            Log($"Moving to safe Z: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            if (CheckStopRequested())
            {
                CurrentState = ToolChangeState.Abort;
                return;
            }
            
            Log($"Moving to TCP: X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
            ExecuteGCode($"G00 G53 X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
            WaitForMotionComplete();
            
            Log($"Moving TCP Z: {F(_config.TCP.Z)}");
            ExecuteGCode($"G00 G53 Z{F(_config.TCP.Z)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.ManualUnloadDialog;
        }

        private void ExecuteManualUnloadDialog()
        {
            Log($"[T{Thread.CurrentThread.ManagedThreadId}] Showing manual removal dialog for T{CurrentTool}");
            
            DialogResult result = ToolChangeDialogs.ShowManualRemoveDialog(CurrentTool);
            
            if (result == DialogResult.OK)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] User confirmed manual removal of T{CurrentTool}");
                SetCurrentTool(0);
                CurrentTool = 0;
                
                if (NewTool > 0 && NewSlot > 0)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] Proceeding to pick T{NewTool} from slot {NewSlot}");
                    CurrentState = ToolChangeState.PickMoveToSlotXY;
                }
                else if (NewTool > 0 && NewSlot == 0)
                {
                    Log($"[T{Thread.CurrentThread.ManagedThreadId}] Proceeding to manual load T{NewTool}");
                    CurrentState = ToolChangeState.ManualLoadMoveToTCP;
                }
                else
                {
                    CurrentState = ToolChangeState.Complete;
                }
            }
            else
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] User cancelled manual removal");
                LastError = "ATC - TC Aborted";
                PlayErrorSound();
                CurrentState = ToolChangeState.Fault;
            }
        }

        private void ExecuteManualLoadMoveToTCP()
        {
            Log($"[T{Thread.CurrentThread.ManagedThreadId}] Manual load: T{NewTool} not in rack, moving to TCP");
            
            Log($"Moving to safe Z: {F(_config.Heights.ZSafe)}");
            ExecuteGCode($"G00 G53 Z{F(_config.Heights.ZSafe)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            if (CheckStopRequested())
            {
                CurrentState = ToolChangeState.Abort;
                return;
            }
            
            Log($"Moving to TCP: X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
            ExecuteGCode($"G00 G53 X{F(_config.TCP.X)} Y{F(_config.TCP.Y)}");
            WaitForMotionComplete();
            
            Log($"Moving TCP Z: {F(_config.TCP.Z)}");
            ExecuteGCode($"G00 G53 Z{F(_config.TCP.Z)}");
            WaitForMotionComplete();
            Wait(_config.FeedsAndTiming.OperationDelay);
            
            CurrentState = ToolChangeState.ManualLoadDialog;
        }

        private void ExecuteManualLoadDialog()
        {
            Log($"[T{Thread.CurrentThread.ManagedThreadId}] Showing manual load dialog for T{NewTool}");
            
            DialogResult result = ToolChangeDialogs.ShowManualLoadDialog(NewTool);
            
            if (result == DialogResult.OK)
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] User confirmed manual load of T{NewTool}");
                SetCurrentTool(NewTool);
                CurrentTool = NewTool;
                CurrentState = ToolChangeState.MeasureIfNeeded;
            }
            else
            {
                Log($"[T{Thread.CurrentThread.ManagedThreadId}] User cancelled manual load");
                LastError = $"Tool T{NewTool} not loaded";
                PlayErrorSound();
                CurrentState = ToolChangeState.Fault;
            }
        }

        private void PlayErrorSound()
        {
            try
            {
                string soundPath = System.IO.Path.Combine(
                    System.Windows.Forms.Application.StartupPath,
                    @"Flashscreen\SOUND\error.wav");
                if (System.IO.File.Exists(soundPath))
                {
                    var player = new System.Media.SoundPlayer(soundPath);
                    player.Play();
                }
            }
            catch { }
        }

        private void ExecuteAbort()
        {
            Log("Tool change ABORTED by user");
            SafetyShutdown();
            CurrentState = ToolChangeState.Idle;
        }

        private void ExecuteFault()
        {
            Log($"Tool change FAULT: {LastError}");
            SafetyShutdown();
            
            if (!IsSimulationMode() && !IsDemoMode())
            {
                try
                {
                    _uc.Stop();
                }
                catch { }
            }
            else
            {
                Log("Demo/Simulation: exec.Stop() skipped");
            }
            
            CurrentState = ToolChangeState.Idle;
        }

        private void SafetyShutdown()
        {
            Log("Safety shutdown: closing outputs");
            SetOutput(_config.IO.Drawbar.Port, _config.IO.Drawbar.Pin, false);
            if (_config.IO.AirBlow.Enabled)
            {
                SetOutput(_config.IO.AirBlow.Port, _config.IO.AirBlow.Pin, false);
            }
        }

        private bool CheckSlotSensor(int slotNumber)
        {
            if (IsSimulationMode())
            {
                if (slotNumber >= 1 && slotNumber <= 10)
                {
                    return _config.Simulation.SlotStates[slotNumber - 1];
                }
                return false;
            }
            
            int pin = _config.IO.SlotSensors.GetPinForSlot(slotNumber);
            int ledNum = GetLEDForPin(_config.IO.SlotSensors.Port, pin);
            bool sensorOn = GetLED(ledNum);
            return !sensorOn;
        }

        private bool VerifySlotStateWithRetry(int slotNumber, bool expectedOccupied)
        {
            if (!_config.IO.SlotSensors.Enabled)
                return true;

            if (IsSimulationMode())
            {
                if (slotNumber >= 1 && slotNumber <= 10)
                {
                    bool simState = _config.Simulation.SlotStates[slotNumber - 1];
                    Log($"[SIM] Slot {slotNumber} state: {(simState ? "occupied" : "empty")}, expected: {(expectedOccupied ? "occupied" : "empty")}");
                    return simState == expectedOccupied;
                }
                return true;
            }

            int pin = _config.IO.SlotSensors.GetPinForSlot(slotNumber);
            int ledNum = GetLEDForPin(_config.IO.SlotSensors.Port, pin);
            
            int elapsed = 0;
            int debounceRequired = 500;
            int debounceCount = 0;
            
            while (elapsed < _config.FeedsAndTiming.SensorWaitTime)
            {
                if (CheckStopRequested())
                    return false;
                
                bool sensorOn = GetLED(ledNum);
                bool slotOccupied = !sensorOn;
                
                if (slotOccupied == expectedOccupied)
                {
                    debounceCount += 50;
                    if (debounceCount >= debounceRequired)
                        return true;
                }
                else
                {
                    debounceCount = 0;
                }
                
                Thread.Sleep(50);
                elapsed += 50;
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
            return GetLED(56) && GetLED(57) && GetLED(58);
        }

        private bool CheckAirPressure()
        {
            int ledNum = GetLEDForPin(_config.IO.PressureSensor.Port, _config.IO.PressureSensor.Pin);
            return !GetLED(ledNum);
        }

        private bool CheckStopRequested()
        {
            if (_stopRequested)
                return true;
            
            if (IsSimulationMode())
                return false;
            
            try
            {
                return _uc.Ismacrostopped();
            }
            catch
            {
                return false;
            }
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
            if (IsSimulationMode()) return 0;
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
            if (IsSimulationMode()) return 0;
            try { return _uc.GetXmachpos(); }
            catch { return 0; }
        }

        private double GetMachineY()
        {
            if (IsSimulationMode()) return 0;
            try { return _uc.GetYmachpos(); }
            catch { return 0; }
        }

        private double GetMachineZ()
        {
            if (IsSimulationMode()) return 0;
            try { return _uc.GetZmachpos(); }
            catch { return 0; }
        }

        private bool GetLED(int ledNum)
        {
            if (IsSimulationMode()) return false;
            try { return _uc.GetLED(ledNum); }
            catch { return false; }
        }

        private void SetOutput(int port, int pin, bool state)
        {
            if (IsSimulationMode())
            {
                Log($"[SIM] Output Port{port} Pin{pin} = {(state ? "ON" : "OFF")}");
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
            if (IsSimulationMode())
            {
                Log($"[SIM] G-code: {code}");
                return;
            }
            try { _uc.Code(code); }
            catch { }
        }

        private void WaitForMotionComplete()
        {
            if (IsSimulationMode()) return;
            
            int timeout = 30000;
            int elapsed = 0;
            
            try
            {
                while (_uc.IsMoving() && elapsed < timeout)
                {
                    if (CheckStopRequested())
                        return;
                    Thread.Sleep(20);
                    elapsed += 20;
                }
            }
            catch { }
            
            if (elapsed >= timeout)
            {
                Log("Warning: Motion timeout");
            }
        }

        private void Wait(int ms)
        {
            if (ms <= 0) return;
            
            int elapsed = 0;
            while (elapsed < ms)
            {
                if (CheckStopRequested())
                    return;
                int sleepTime = Math.Min(50, ms - elapsed);
                Thread.Sleep(sleepTime);
                elapsed += sleepTime;
            }
        }

        public void RequestStop()
        {
            _stopRequested = true;
        }
        
        public ToolChangeSnapshot GetSnapshot()
        {
            lock (_stateLock)
            {
                return new ToolChangeSnapshot
                {
                    State = CurrentState,
                    CurrentTool = CurrentTool,
                    NewTool = NewTool,
                    OldSlot = OldSlot,
                    NewSlot = NewSlot,
                    LastError = LastError,
                    IsRunning = IsRunning
                };
            }
        }
    }
    
    public class ToolChangeSnapshot
    {
        public ToolChangeState State { get; set; }
        public int CurrentTool { get; set; }
        public int NewTool { get; set; }
        public int OldSlot { get; set; }
        public int NewSlot { get; set; }
        public string LastError { get; set; }
        public bool IsRunning { get; set; }
    }
}
