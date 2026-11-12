# HTM_ATC Plugin Specification

## Overview

The HTM_ATC Plugin is a standalone UCCNC plugin (C# DLL, .NET Framework 4.x, WinForms) for controlling a 10-slot linear automatic tool changer. It requires NO screenset modifications and provides its own UI window accessible from UCCNC's Plugins menu.

**Target Environment:**
- UCCNC 1.2118
- Controller: UC300ETH-UB1
- Testing: UCCNC Demo mode (works under Wine/Bottles on Linux)
- Profile: Stock Default profile with Default2019 screenset

---

## Architecture

### Plugin Structure

```
Plugin/
├── src/
│   ├── HTM_ATC.csproj           # .NET Framework 4.x project
│   ├── UCCNCplugin.cs           # Plugin entry point (required class name)
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Forms/
│   │   ├── MainForm.cs          # Main plugin window (tabs)
│   │   ├── SetupWizard.cs       # First-run setup wizard
│   │   └── DiagnosticsPanel.cs  # Live I/O diagnostics
│   ├── Core/
│   │   ├── ATCState.cs          # State machine for tool change
│   │   ├── ATCConfig.cs         # Configuration model
│   │   ├── RackManager.cs       # Slot/tool management
│   │   └── SafetyMonitor.cs     # Safety checks (OSSD, pressure)
│   ├── Persistence/
│   │   └── ConfigStore.cs       # JSON config next to profile
│   ├── Simulation/
│   │   └── SimulatedIO.cs       # Fake sensors for Demo mode
│   └── Resources/
│       └── Strings.resx         # Localizable strings (EN base, RO later)
├── lib/
│   └── README.md                # Instructions for reference DLLs
└── docs/
    └── SPEC.md                  # This file
```

### Plugin Interface

The plugin implements the standard UCCNC plugin interface:
- `UCCNCplugin` class in `Plugins` namespace
- `Init_event()` - Initialize with Plugininterface.Entry
- `Getproperties_event()` - Return plugin metadata
- `Startup_event()` - Called when plugin starts, show window if configured
- `Showup_event()` - Called from Plugins menu, show/focus window
- `Shutdown_event()` - Cleanup on UCCNC exit
- `Loop_event()` - Called at 25Hz for sensor polling and state sync
- `Configure_event()` - Show configuration (launches wizard)
- `Informplugin_event()` - Receive M6.txt calls
- `Buttonpress_event()` - Monitor button presses if needed

### Threading Model

**Critical:** UCCNC calls `Loop_event()` on its own thread (~25Hz), while the plugin UI
runs on a separate WinForms UI thread. Direct cross-thread UI access can cause deadlocks.

**Solution implemented:**
1. `Loop_event()` sets a flag (`_updatePending`) and returns immediately
2. A WinForms `Timer` on the UI thread polls this flag and performs UI updates
3. All UI updates happen on the UI thread via the timer tick handler
4. Guards prevent re-entry and detect closing state

This avoids the hang that occurred in v1.0.0 when the wizard finished saving config
while `Loop_event` was trying to access form controls.

### File Logging

The plugin logs key events to `{UCCNC_Path}/Profiles/HTM_ATC_{profile}.log`:
- Plugin startup/shutdown
- Wizard finish and config save
- Form open/close
- Loop exceptions
- M6 tool change requests

Log format: `[YYYY-MM-DD HH:mm:ss.fff] message`

---

## Configuration Model (ATCConfig)

### Rack Geometry
| Field | Type | Description | Range | HTM Field |
|-------|------|-------------|-------|-----------|
| RackAxis | string | Primary axis for rack (X or Y) | "X"/"Y" | 30000 |
| RackAxisSecondary | string | Secondary axis | "Y"/"X" | 30001 |
| SlotCount | int | Number of slots | 1-10 | Fixed: 10 |
| Slot1Position | double | Machine coord of slot 1 on rack axis | mm | 30010 |
| Slot1PositionSecondary | double | Machine coord on secondary axis | mm | 30011 |
| SlotPitch | double | Distance between slot centers | mm | 30013 |

### Z Heights (Machine Coordinates)
| Field | Type | Description | HTM Field |
|-------|------|-------------|-----------|
| ZSafe | double | Safe Z for travel (above everything) | 30015 |
| ZEmpty | double | Z for empty spindle travel | 30016 |
| ZClamp | double | Z for tool engage (in fork) | 30012 |
| ZCheck | double | Z for sensor verification | 30023 |
| ZApproach | double | Y position before entering fork | 30014 |

### Feed Rates
| Field | Type | Description | HTM Field |
|-------|------|-------------|-----------|
| TCFeedRate | double | Feed rate for tool change moves | 30017 |
| Delay | int | Delay between operations (ms) | 30018 |
| SensorWaitTime | int | Max wait for sensor state (ms) | 30019 |

### Tool Change Position (TCP)
| Field | Type | Description | HTM Field |
|-------|------|-------------|-----------|
| TCPx | double | X position for manual tool change | 30020 |
| TCPy | double | Y position for manual tool change | 30021 |
| TCPz | double | Z position for manual tool change | 30022 |

### I/O Mapping
| Field | Type | Description | HTM Field |
|-------|------|-------------|-----------|
| DrawbarPort | int | Output port for drawbar | 20650 |
| DrawbarPin | int | Output pin for drawbar | 20651 |
| AirBlowPort | int | Output port for air blow | 20652 |
| AirBlowPin | int | Output pin for air blow | 20653 |
| AirBlowEnabled | bool | Enable air blow output | - |
| PressureSensorPort | int | Input port for air pressure | 20654 |
| PressureSensorPin | int | Input pin for air pressure | 20655 |
| PressureSensorEnabled | bool | Enable pressure monitoring | - |
| DustHoodPort | int | Output port for dust hood | - |
| DustHoodPin | int | Output pin for dust hood | - |
| DustHoodEnabled | bool | Enable dust hood control | - |
| DustHoodRaiseBeforeTC | bool | Raise hood before tool change | - |
| DustHoodLowerAfterTC | bool | Lower hood after tool change | - |
| SpindleToolSensorPort | int | Input port for spindle tool sensor | - |
| SpindleToolSensorPin | int | Input pin for spindle tool sensor | - |
| SpindleToolSensorEnabled | bool | Enable spindle tool presence check | - |
| SpindleToolSensorActiveLow | bool | Sensor is active low | - |

### Slot Sensors (Port 5, Pins 2-11 by default)
| Field | Type | Description |
|-------|------|-------------|
| SlotSensorPort | int | Input port for all slot sensors |
| SlotSensorPins | int[10] | Pin numbers for slots 1-10 |
| SlotSensorsEnabled | bool | Enable slot sensor checking |

### Safety Configuration (OSSD Inputs)
| Field | Type | Description |
|-------|------|-------------|
| Safety.Enabled | bool | Enable safety monitoring |
| Safety.OssdInputs[] | array | List of OSSD input configurations |
| OssdInputs[n].Name | string | Descriptive name (e.g., "E-Stop") |
| OssdInputs[n].LedNumber | int | UCCNC LED number to monitor |
| OssdInputs[n].ActiveLow | bool | Fault when LED is ON (default: true) |
| OssdInputs[n].Enabled | bool | Include in monitoring |

Default OSSD inputs for UC300ETH-UB1:
- LED 3: E-Stop
- LED 4: X-Axis Driver
- LED 5: Z-Axis Driver
- LED 6: C-Axis Driver
- LED 7: Power Supply

### Tool Length Probe Settings
| Field | Type | Description | UCCNC Field |
|-------|------|-------------|-------------|
| Probe.UseUCCNCSettings | bool | Read from UCCNC at probe time | - |
| Probe.FastFeed | double | Fast probe feed (mm/min) | 2709 |
| Probe.SlowFeed | double | Slow probe feed (mm/min) | 2710 |
| Probe.Retract | double | Retract between touches (mm) | 2706 |
| Probe.DualCycle | bool | Use fast-then-slow cycle | Button 853 |
| Probe.MinZ | double | Minimum Z limit | 41 |

When `UseUCCNCSettings` is true, probe values are read live from UCCNC fields at probe time.
When false, the plugin's own saved values are used.

### Options
| Field | Type | Description | HTM Field |
|-------|------|-------------|-----------|
| ShowATCPageDuringTC | bool | Switch to ATC tab during tool change | 30006 |
| ApplyG43 | bool | Apply G43 Hxx after tool change | 30005 |
| ReturnToXY | bool | Return to original XY after TC | 30004 |
| MeasureNonRH | bool | Measure non-reference-height tools | 30003 |
| AirBlowDuringTC | bool | Enable air blow during TC | 20203 |

### Per-Slot Configuration
| Field | Type | Description |
|-------|------|-------------|
| Slots[n].Enabled | bool | Slot is active/usable |
| Slots[n].ToolNumber | int | Tool currently in slot (0=empty) |

---

## State Machine: M6 Tool Change

### States

```
IDLE
  │
  ├─[M6 Tn]─> VALIDATE_REQUEST
  │
VALIDATE_REQUEST
  │ - Check axes homed (LEDs 56,57,58)
  │ - Check air pressure (if sensors enabled)
  │ - Check OSSD inputs
  │ - Validate tool numbers
  │
  ├─[fail]──> FAULT
  ├─[T==current && applyG43]──> APPLY_OFFSET ─> IDLE
  └─[OK]──> STOP_SPINDLE
  
STOP_SPINDLE
  │ - exec.Stopspin()
  │ - exec.Code("M09")
  │ - Wait for spindle to stop (verify AS3.Getfield(870) == 0)
  │ - Timeout from field 2761 (spindle wait time)
  │
  ├─[timeout]──> FAULT
  └─[stopped]──> SAVE_POSITION
  
SAVE_POSITION
  │ - Record original X, Y, Z machine positions
  │ - Record modal state (G90/G91, etc.)
  │ - Record feed rate
  │
  └─> MOVE_SAFE_Z

MOVE_SAFE_Z
  │ - G00 G53 Z[ZSafe]  // ALWAYS before any XY move
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> DETERMINE_SLOTS

DETERMINE_SLOTS
  │ - Find oldSlot: where currentTool is stored (if any)
  │ - Find newSlot: where newTool is stored (if any)
  │ - If currentTool>0 && no oldSlot: find first free slot
  │
  ├─[currentTool>0 && hasSlot]──> DROP_TOOL_MOVE_XY
  ├─[currentTool>0 && noSlot]──> MANUAL_UNLOAD
  └─[currentTool==0]──> PICK_TOOL_MOVE_XY

DROP_TOOL_MOVE_XY
  │ - G00 G53 [RackAxis][slot position]
  │ - G00 G53 [SecondaryAxis][ZApproach]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> DROP_VERIFY_EMPTY

DROP_VERIFY_EMPTY
  │ - CheckSlot(oldSlot, expectedOccupied=false)
  │ - Wait with debounce (1000ms stable)
  │
  ├─[timeout/mismatch]──> FAULT_WITH_DIALOG (Abort/Retry/Ignore)
  └─[OK]──> DROP_AIRBLOW_ON

DROP_AIRBLOW_ON
  │ - If airBlowEnabled: exec.Setoutpin(airPort, airPin)
  │
  └─> DROP_LOWER_TO_CLAMP

DROP_LOWER_TO_CLAMP
  │ - G01 G53 Z[ZClamp] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> DROP_ENTER_FORK

DROP_ENTER_FORK
  │ - G01 G53 [SecondaryAxis][SlotY] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> DROP_OPEN_DRAWBAR

DROP_OPEN_DRAWBAR
  │ - exec.Setoutpin(drawbarPort, drawbarPin)
  │ - exec.Wait(delay)
  │
  └─> DROP_RAISE_TO_CHECK

DROP_RAISE_TO_CHECK
  │ - If ZCheck != ZClamp: G01 G53 Z[ZCheck] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> DROP_VERIFY_TOOL_IN_SLOT

DROP_VERIFY_TOOL_IN_SLOT
  │ - CheckSlot(oldSlot, expectedOccupied=true)
  │
  ├─[fail]──> FAULT
  └─[OK]──> DROP_AIRBLOW_OFF

DROP_AIRBLOW_OFF
  │ - If airBlowEnabled: exec.Clroutpin(airPort, airPin)
  │
  └─> DROP_RAISE_TO_EMPTY

DROP_RAISE_TO_EMPTY
  │ - G01 G53 Z[ZEmpty] F[TCFeedRate]  // empty spindle, lower clearance OK
  │ - while(exec.IsMoving()){}
  │
  └─> DROP_CLOSE_DRAWBAR

DROP_CLOSE_DRAWBAR
  │ - exec.Clroutpin(drawbarPort, drawbarPin)
  │ - Update state: Slots[oldSlot].ToolNumber = currentTool
  │ - exec.Setcurrenttool(0)
  │ - Persist state immediately
  │
  └─> [newTool>0?] PICK_TOOL_MOVE_XY : COMPLETE

PICK_TOOL_MOVE_XY
  │ - If newSlot==0: goto MANUAL_LOAD
  │ - G00 G53 [RackAxis][newSlot position]
  │ - G00 G53 [SecondaryAxis][SlotY]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> PICK_VERIFY_TOOL_PRESENT

PICK_VERIFY_TOOL_PRESENT
  │ - CheckSlot(newSlot, expectedOccupied=true)
  │
  ├─[fail]──> FAULT
  └─[OK]──> PICK_AIRBLOW_ON

PICK_AIRBLOW_ON
  │ - If airBlowEnabled: exec.Setoutpin(airPort, airPin)
  │ - exec.Setoutpin(drawbarPort, drawbarPin)  // Open before lowering
  │
  └─> PICK_LOWER_TO_CLAMP

PICK_LOWER_TO_CLAMP
  │ - G01 G53 Z[ZClamp] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  ├─[Ismacrostopped]──> ABORT
  └─[OK]──> PICK_CLOSE_DRAWBAR

PICK_CLOSE_DRAWBAR
  │ - exec.Clroutpin(drawbarPort, drawbarPin)  // Grab tool
  │ - If airBlowEnabled: exec.Clroutpin(airPort, airPin)
  │ - exec.Wait(delay)
  │
  └─> PICK_RAISE_TO_CHECK

PICK_RAISE_TO_CHECK
  │ - If ZCheck != ZClamp: G01 G53 Z[ZCheck] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  └─> PICK_VERIFY_SLOT_EMPTY

PICK_VERIFY_SLOT_EMPTY
  │ - CheckSlot(newSlot, expectedOccupied=false)
  │
  ├─[fail]──> FAULT
  └─[OK]──> PICK_EXIT_FORK

PICK_EXIT_FORK
  │ - G01 G53 [SecondaryAxis][ZApproach] F[TCFeedRate]
  │ - while(exec.IsMoving()){}
  │
  └─> PICK_RAISE_TO_SAFE

PICK_RAISE_TO_SAFE
  │ - G00 G53 Z[ZSafe]
  │ - while(exec.IsMoving()){}
  │ - Update state: Slots[newSlot].ToolNumber = 0
  │ - exec.Setcurrenttool(newTool)
  │ - Persist state
  │
  └─> MEASURE_IF_NEEDED

MEASURE_IF_NEEDED
  │ - Read tool type from tooltable
  │ - If measureNonRH && type != "RH": run measurement
  │   - Move to TCP
  │   - G31 probing with verification (LED 244 = ProbedOK)
  │   - Calculate and store length
  │
  └─> APPLY_OFFSET

APPLY_OFFSET
  │ - If applyG43: exec.Code("G43 H" + newTool)
  │ - Else: exec.Code("G49")
  │
  └─> RESTORE_POSITION

RESTORE_POSITION
  │ - If returnToXY: G00 G53 X[origX] Y[origY]
  │ - Restore modal state (G90/G91)
  │ - Restore feed rate
  │
  └─> COMPLETE

COMPLETE
  │ - Log success
  │
  └─> IDLE

MANUAL_UNLOAD
  │ - Move to TCP
  │ - Show dialog: "Remove tool T{n} from spindle"
  │ - Wait for OK/Cancel
  │
  ├─[Cancel]──> ABORT
  └─[OK]──> exec.Setcurrenttool(0), continue to PICK_TOOL_MOVE_XY

MANUAL_LOAD
  │ - Move to TCP
  │ - Show dialog: "Load tool T{n} into spindle"
  │ - Wait for OK/Cancel
  │
  ├─[Cancel]──> ABORT
  └─[OK]──> exec.Setcurrenttool(newTool), continue to APPLY_OFFSET

ABORT
  │ - Called when Ismacrostopped() returns true
  │ - exec.Clroutpin(airPort, airPin)
  │ - exec.Clroutpin(drawbarPort, drawbarPin)
  │ - Log "ATC - TC Aborted!"
  │
  └─> IDLE

FAULT
  │ - exec.Clroutpin(airPort, airPin)
  │ - exec.Clroutpin(drawbarPort, drawbarPin)
  │ - Attempt to move to ZSafe if possible
  │ - exec.Stop()  // Actually stop the machine
  │ - Play error sound
  │ - Log specific fault message
  │
  └─> IDLE
```

### Critical Safety Rules

1. **Always Z Safe First**: Before ANY XY movement, move to ZSafe in machine coords (G53)
2. **Drawbar Closed on Exit**: Any abort/fault must close drawbar before returning
3. **Air Off on Exit**: Any abort/fault must turn off air blow
4. **Verify Sensor States**: Every slot operation must verify expected sensor state
5. **G31 Hit Verification**: Check LED 244 (ProbedOK) after probing
6. **Machine Coordinates Only**: All positioning uses G53 machine coordinates
7. **InvariantCulture**: All number parsing/formatting uses InvariantCulture
8. **Spindle Verified Stopped**: Wait for actual speed (field 870) to reach 0
9. **exec.Stop() on Fault**: Safety faults call exec.Stop(), not just return

---

## Setup Wizard

### Overview

The Setup Wizard provides a guided first-run experience for configuring the ATC. It:
- Launches automatically when no saved config exists
- Can be re-launched from the main plugin window ("Run Setup Wizard" button)
- Pre-fills current values when re-running
- Optionally imports existing HTM_ATC profile values
- Provides live testing capabilities at relevant steps

### Wizard Steps (17 total)

1. Welcome & Import
2. Rack Orientation
3. Slot 1 Position (shows axis labels based on orientation, machine coordinates G53)
4. Slot Spacing
5. Z Heights
6. Drawbar Output
7. Air Blow (optional)
8. Dust Hood (optional, NEW)
9. Pressure Sensor (optional)
10. Spindle Tool Sensor (optional, NEW)
11. Slot Sensors
12. Safety Inputs (OSSD configuration, NEW)
13. Tool Length Probe (hybrid UCCNC/plugin settings, NEW)
14. Enable Slots
15. Speeds & Timing
16. Options
17. Summary

---

#### Step 1: Welcome & Import
**Title:** "HTM ATC Setup Wizard"

**Content:**
- Welcome message explaining what the wizard configures
- Option to import existing HTM_ATC profile settings
  - Browse for profile file (.pro)
  - Parse UserTextfields section for known field IDs
- Checkbox: "Start fresh with default values"

**Validation:** None required

---

#### Step 2: Rack Orientation
**Title:** "Tool Rack Orientation"

**Content:**
- Explanation: "Select the axis along which your tool rack is aligned. Most linear racks have tools arranged in a row along X or Y."
- Radio buttons: X-axis / Y-axis
- Diagram placeholder showing rack orientations
- Secondary axis is auto-calculated

**Validation:** Selection required

**Live Test:** None

---

#### Step 3: Slot 1 Position
**Title:** "Slot 1 Position"

**Content:**
- Explanation: "Position the spindle directly above the center of Slot 1. This is where the tool holder cone aligns with the fork."
- Machine position displays (X, Y)
- "Jog" buttons to open jog panel
- "Read Current Position" button
- Manual entry fields for X and Y

**Validation:**
- Values must be within machine limits

**Live Test:** None (jogging is test)

---

#### Step 4: Slot Pitch
**Title:** "Slot Spacing (Pitch)"

**Content:**
- Explanation: "Enter the center-to-center distance between adjacent slots. You can measure this or jog to Slot 2 and let the wizard calculate it."
- Manual entry field (mm)
- "Jog to Slot 2 and Calculate" button
- Calculated value display: "(Slot2 - Slot1) = XX mm"
- Typical values: 60-100mm

**Validation:**
- Pitch > 0
- Pitch < 200mm (sanity check)

---

#### Step 5: Z Heights
**Title:** "Z Axis Heights"

**Content:**
- Explanation: "Configure the Z heights for tool change operations. All values are in MACHINE coordinates (G53)."

**Fields (with individual explanations):**

1. **Z Safe** (30015)
   - "The highest Z position, used for safe travel. Spindle clears all obstacles including tools in rack."
   - "Read Current Z" button

2. **Z Empty** (30016)
   - "Z height for traveling with empty spindle. Can be lower than ZSafe since no tool is loaded."
   - "Read Current Z" button

3. **Z Approach / Clear Y** (30014)
   - "Secondary axis position when approaching/leaving the fork. Must clear the fork fingers."
   - "Read Current Y" button

4. **Z Clamp** (30012)
   - "Z height where the tool is fully seated in the fork for clamping/releasing."
   - "Read Current Z" button

5. **Z Check** (30023)
   - "Z height for sensor verification. If same as Z Clamp, sensor checked at clamp position."
   - "Read Current Z" button or "Same as Z Clamp" checkbox

**Validation:**
- ZSafe > ZEmpty
- ZSafe > ZClamp
- ZClamp < ZCheck <= ZSafe (if different)

**Diagram:** Cross-section showing Z heights relative to tool and fork

---

#### Step 6: Drawbar Output
**Title:** "Drawbar Pneumatic Valve"

**Content:**
- Explanation: "Configure the output that controls the drawbar pneumatic valve. When activated, the drawbar opens to release the tool."
- Port selector (1-5)
- Pin selector (1-17)
- Current state indicator (LED)

**Validation:**
- Port 1-5, Pin 1-17
- Warn if pin is used for other functions

**Live Test:**
- "Test Drawbar" button with confirmation dialog
- "⚠️ This will open the drawbar. Ensure no tool is in spindle or hold the tool!"
- Toggle on/off with visual feedback
- Auto-off after 3 seconds

---

#### Step 7: Air Blow Output
**Title:** "Air Blow (Optional)"

**Content:**
- Explanation: "Configure the air blow output for cleaning tool holders during changes. This is optional."
- Enable checkbox
- Port selector (1-5)
- Pin selector (1-17)

**Validation:**
- If enabled: Port 1-5, Pin 1-17
- Cannot be same as drawbar

**Live Test:**
- "Test Air Blow" button
- Toggle on/off with visual feedback

---

#### Step 8: Dust Hood (NEW)
**Title:** "Dust Hood Control (Optional)"

**Content:**
- Explanation: "Configure an output to control a dust hood that can be raised/lowered during tool changes."
- Enable checkbox
- Port selector (1-5)
- Pin selector (1-17)
- Checkbox: "Raise dust hood before tool change"
- Checkbox: "Lower dust hood after tool change"

**Validation:**
- If enabled: Port 1-5, Pin 1-17

**Live Test:**
- "Test Dust Hood" button to toggle

---

#### Step 9: Air Pressure Sensor
**Title:** "Air Pressure Sensor (Optional)"

**Content:**
- Explanation: "Configure an input to detect low air pressure. Tool changes will be blocked if pressure is low."
- Enable checkbox
- Port selector (1-5)
- Pin selector (1-17)
- Active state selector (High = OK / Low = OK)

**Validation:**
- If enabled: Port 1-5, Pin 1-17

**Live Test:**
- Live LED showing current input state
- "Current state: OK / LOW PRESSURE"

---

#### Step 10: Spindle Tool Sensor (NEW)
**Title:** "Spindle Tool Presence Sensor (Optional)"

**Content:**
- Explanation: "Configure an optional sensor to detect if a tool is present in the spindle. Used for verification during tool changes."
- Enable checkbox
- Port selector (1-5)
- Pin selector (1-17)
- Active level selector: "Active Low (sensor ON = tool present)"

**Validation:**
- If enabled: Port 1-5, Pin 1-17

**Live Test:**
- Live display showing current state
- "Current state: TOOL PRESENT / NO TOOL"

---

#### Step 11: Slot Sensors
**Title:** "Slot Presence Sensors"

**Content:**
- Explanation: "Configure inputs that detect whether each slot has a tool present. These sensors improve safety by verifying tool pickup/dropoff."
- Enable checkbox for slot sensor system
- Port for slot sensors (typically Port 5)
- Pin mapping table (Slot 1-10 → Pin)
- Default: Port 5, Pins 2-11

**Validation:**
- If enabled: All pins must be unique
- All pins must be in valid range

**Live Test:**
- Live display showing all 10 sensor states
- "Place/remove tools to verify sensors respond correctly"

---

#### Step 12: Safety Inputs (OSSD) (NEW)
**Title:** "Safety Input Configuration"

**Content:**
- Explanation: "Configure OSSD (Output Signal Switching Device) safety inputs. These monitor critical safety signals and stop the machine if a fault is detected."
- Master enable checkbox: "Enable Safety Monitoring"
- Table with 5 configurable inputs:
  - Enable checkbox
  - Name field (e.g., "E-Stop", "X-Axis Driver")
  - LED number (UCCNC LED to monitor)
  - Active Low checkbox (fault when LED is ON)

**Default values for UC300ETH-UB1:**
- LED 3: E-Stop
- LED 4: X-Axis Driver
- LED 5: Z-Axis Driver
- LED 6: C-Axis Driver
- LED 7: Power Supply

**Validation:**
- At least one input should be enabled if safety is enabled

---

#### Step 13: Tool Length Probe (NEW)
**Title:** "Tool Length Probe Settings"

**Content:**
- Explanation: "Configure tool length probing parameters. The plugin can use UCCNC's probe settings or its own values."
- Checkbox: "Use UCCNC probe settings" (reads fields 2709, 2710, 2706, button 853, field 41 at probe time)
- When unchecked, show editable fields:
  - Fast feed (mm/min) - pre-filled from UCCNC field 2709
  - Slow feed (mm/min) - pre-filled from UCCNC field 2710
  - Retract distance (mm) - pre-filled from UCCNC field 2706
  - Dual cycle checkbox - pre-filled from UCCNC button 853
  - Min Z limit - pre-filled from UCCNC field 41

**Validation:**
- If manual: feed rates > 0, retract > 0

---

#### Step 14: Enable Slots
**Title:** "Active Slots"

**Content:**
- Explanation: "Select which slots are physically present and usable in your rack."
- Checkbox grid for Slots 1-10
- Visual rack diagram with slot numbers

**Validation:**
- At least one slot must be enabled

---

#### Step 15: Feed Rates & Timing
**Title:** "Speeds and Timing"

**Content:**
- Explanation: "Configure motion speeds and timing delays."

**Fields:**

1. **Tool Change Feed Rate** (30017)
   - "Speed for tool change movements (mm/min)"
   - Typical: 1000-3000 mm/min

2. **Operation Delay** (30018)
   - "Delay between operations (milliseconds)"
   - Typical: 200-500 ms

3. **Sensor Wait Time** (30019)
   - "Maximum time to wait for sensor state change (milliseconds)"
   - Typical: 3000-5000 ms

**Validation:**
- Feed rate > 0
- Delays >= 0

---

#### Step 16: Options
**Title:** "Tool Change Options"

**Content:**
- Explanation: "Configure optional behaviors during tool changes."

**Checkboxes:**

1. **Apply G43 after tool change** (30005)
   - "Automatically apply tool length compensation (G43 Hxx) after loading a tool"

2. **Return to XY position** (30004)
   - "Return to the original XY position after completing the tool change"

3. **Measure non-RH tools** (30003)
   - "Automatically measure tools not marked as 'Reference Height' in tooltable"

4. **Show ATC page during TC** (30006)
   - "Switch to ATC display page during tool change operations"

5. **Air blow during tool change**
   - "Use air blow during tool change operations"

---

#### Step 17: Summary
**Title:** "Configuration Summary"

**Content:**
- Read-only summary of all settings organized by category
- Scrollable panel with all values
- "Warnings" section if any issues detected:
  - Unused slots with sensors enabled
  - Potentially dangerous Z height configurations
  - Missing optional features

**Actions:**
- "Back" - Return to edit any step
- "Save & Close" - Save configuration and exit wizard
- "Save & Test" - Save and perform a simulated dry-run

---

### Wizard Implementation Notes

1. **Navigation:**
   - Back/Next/Cancel buttons
   - Cancel shows confirmation: "Discard all changes?"
   - Back preserves entered values
   - Progress indicator (Step X of 14)

2. **Validation:**
   - Red border on invalid fields
   - Next button disabled until valid
   - Error messages in consistent location

3. **Position Reading:**
   - Use exec.GetXmachpos(), etc.
   - Show all 3 decimals
   - Use InvariantCulture for display/parse

4. **Live Tests:**
   - Require explicit user confirmation
   - Show clear status indicators
   - Auto-disable after timeout
   - Log all test actions

5. **Localization:**
   - All user-visible strings in Resources/Strings.resx
   - Wizard step titles, explanations, button labels
   - Error messages
   - Prepared for Romanian translation

---

## Persistence Format

Configuration is stored as JSON in the UCCNC Profiles folder:

**Location:** `{UCCNC_Path}/Profiles/HTM_ATC_Config_{profileName}.json`

For example, if UCCNC is installed at `C:\UCCNC\2118\` and the active profile is `ATCdev.pro`:
```
C:\UCCNC\2118\Profiles\HTM_ATC_Config_ATCdev.json
```

**Schema:**
```json
{
  "version": 1,
  "lastModified": "2024-11-12T10:30:00Z",
  "rack": {
    "axis": "X",
    "slotCount": 10,
    "slot1X": 50.000,
    "slot1Y": -120.000,
    "pitch": 80.000
  },
  "heights": {
    "zSafe": 0.000,
    "zEmpty": -20.000,
    "zApproach": -80.000,
    "zClamp": -145.000,
    "zCheck": -140.000
  },
  "feedsAndTiming": {
    "feedRate": 2000.0,
    "operationDelay": 300,
    "sensorWaitTime": 5000
  },
  "tcp": {
    "x": 100.000,
    "y": 50.000,
    "z": 0.000
  },
  "io": {
    "drawbar": { "port": 1, "pin": 14 },
    "airBlow": { "enabled": true, "port": 1, "pin": 15 },
    "pressureSensor": { "enabled": true, "port": 1, "pin": 10 },
    "slotSensors": {
      "enabled": true,
      "port": 5,
      "pins": [2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
    }
  },
  "slots": [
    { "index": 1, "enabled": true, "toolNumber": 1 },
    { "index": 2, "enabled": true, "toolNumber": 0 },
    { "index": 3, "enabled": true, "toolNumber": 5 },
    ...
  ],
  "options": {
    "applyG43": true,
    "returnToXY": true,
    "measureNonRH": false,
    "showATCPage": false,
    "airBlowDuringTC": true
  },
  "simulation": {
    "enabled": false,
    "slotStates": [true, false, true, false, false, false, false, false, false, false]
  }
}
```

**Save triggers:**
- Immediately on any configuration change (wizard, manual edit)
- On slot/tool state changes during operation
- NOT deferred to form close

---

## UI Layout

### Main Window Tabs

#### Tab 1: Status
- Current tool display (large)
- Active tool location in rack (visual)
- Rack visualization with slot states
- Quick actions: Go to TCP, Manual drawbar test
- Status messages area

#### Tab 2: Tool Rack
- Visual representation of 10-slot rack
- Per-slot display:
  - Slot number
  - Tool number (editable)
  - Tool diameter (from tooltable)
  - Tool length (from tooltable)
  - Enabled/disabled toggle
  - Sensor state LED
  - Active tool indicator
- Assign tool button
- Clear slot button

#### Tab 3: Setup
- Grouped settings panels matching wizard steps
- Editable fields with validation
- "Run Setup Wizard" button
- "Import from HTM_ATC Profile" button
- "Export Settings" button
- Save button (though saves are immediate)

#### Tab 4: Diagnostics
- Live I/O display:
  - Input LEDs (Port 1-5, relevant pins)
  - Output states
  - Drawbar state
  - Air blow state
  - Pressure sensor state
  - All slot sensors
- Manual output toggle buttons (with confirmation)
- Position DROs (X, Y, Z machine coords)
- Spindle state

#### Tab 5: Simulation
- Enable/disable simulation mode
- Simulated slot states (checkboxes)
- Simulated pressure OK
- Simulated OSSD OK
- "Run Test Cycle" button
- Log output area

---

## Loop Callback Responsibilities

The Loop_event() runs at 25Hz and replaces M20100/M20101 macroloops:

1. **Sensor Polling:**
   - Read all slot sensor inputs
   - Update UI indicators
   - Debounce sensor changes

2. **Safety Monitoring:**
   - Check OSSD inputs (if configured)
   - Check air pressure (if configured)
   - On fault: exec.Stop() + alarm message

3. **State Synchronization:**
   - Verify tool-in-spindle matches expected
   - Update current tool display
   - Sync with UCCNC's tool number

4. **Simulation Mode:**
   - When enabled, return simulated sensor states
   - Allow full cycle testing in Demo mode

---

## M6.txt Macro

The only required profile modification - a minimal M6.txt:

```csharp
// M6.txt - HTM_ATC Plugin Tool Change
// Place in {UCCNC_Path}/Profile/Macro_{ProfileName}/M6.txt

// Get requested tool
int newTool = exec.Getnewtool();

// Call the HTM_ATC plugin to handle tool change
object result = exec.Informplugin("HTM_ATC.dll", "M6:" + newTool);

// Plugin handles everything including errors
// Return value indicates success (null=ok, string=error)
if (result is string)
{
    string error = result as string;
    if (!string.IsNullOrEmpty(error))
    {
        exec.AddStatusmessage("ATC Error: " + error);
    }
}
```

The plugin may install this automatically with user confirmation and backup of existing M6.txt.

---

## Known Bugs to Avoid (from Macro Review)

1. ✅ **CheckSlot leaves outputs ON** → Always close drawbar/air on any exit path
2. ✅ **Air check bound to button 20202** → Config flag, not screenset button
3. ✅ **No safe-Z before XY when spindle empty** → Always ZSafe before any XY
4. ✅ **Back-to-XY from empty Z** → Raise to ZSafe first
5. ✅ **CheckSlot depends on macroloop** → Plugin polls directly, no dependency
6. ✅ **G31 hit never verified** → Check LED 244 (ProbedOK)
7. ✅ **G31 target in work coords** → Use G53 machine coords
8. ✅ **Measures T0 when measureNRH** → Skip if newTool == 0
9. ✅ **Decimal-comma culture** → InvariantCulture everywhere
10. ✅ **Spindle wait = 0** → Actually wait and timeout
11. ✅ **Modal state/feed not restored** → Save and restore modal state
12. ✅ **G43 without length** → Verify length exists before G43
13. ✅ **Setup only saved on close** → Save immediately on change
14. ✅ **OSSD only shows messages** → Call exec.Stop() on OSSD fault

---

## Open Questions for Owner

1. **OSSD Input Mapping**: The current macros monitor LEDs 3-7 for E-Stop, driver faults, PSU. Are these the correct inputs for your UC300ETH-UB1? Should the plugin allow configuring custom OSSD inputs?

2. **Tool Length Sensor**: The current M20331 uses fields 2709, 2710 (probe feedrates) and 2706 (retract). Should these be configurable in the plugin or reuse UCCNC's probing settings?

3. **Dust Hood Control**: The current setup mentions a dust hood output. Should this be integrated into the plugin?

4. **Spindle Tool Sensor**: Is there a sensor to directly detect tool presence in spindle (beyond drawbar position)?

5. **Warm-up/Cool-down**: Should the plugin handle spindle warm-up sequences or leave that to separate macros?

6. **Network Errors**: The UC300ETH can have network timeouts. Should the plugin implement retry logic for commands?

7. **Multiple Profiles**: Should the plugin support different configs per UCCNC profile, or a single global config?

8. **Slot Position Override**: Should individual slots support custom X/Y positions, or is uniform pitch sufficient?

9. **Tool Magazine Type**: Is this always a linear horizontal rack, or could it be vertical or carousel in future?

10. **Language Priority**: Should Romanian be the default language, with English as fallback, or vice versa?

---

## Migration from HTM_ATC Profile

The plugin can import existing settings from an HTM_ATC profile by parsing:

**Profile .pro file sections:**
- `[UserTextfields]` - Fields 30000-30024, 20650-20655
- `[UserCheckboxes]` - Checkboxes 30002-30006
- `[Tooltablevalues]` - Tool slot assignments

**Import Process:**
1. User selects .pro file
2. Plugin reads INI-style sections
3. Maps known field IDs to config properties
4. Shows preview of imported values
5. User confirms or adjusts
6. Saves to new JSON format

---

## Build Requirements

- .NET Framework 4.0 or 4.8
- Reference assemblies (NOT committed to repo):
  - Plugininterface.dll (from UCCNC installation)
- Build works with:
  - Visual Studio 2019+
  - MSBuild on Windows
  - Mono/MSBuild on Linux (for development)
  - dotnet build with appropriate targeting

See `Plugin/lib/README.md` for setup instructions.
