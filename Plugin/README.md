# HTM_ATC Plugin

A standalone UCCNC plugin for controlling a 10-slot linear automatic tool changer. This plugin requires NO screenset modifications and provides its own WinForms UI.

## Features

- **No screenset modifications required** - works with stock Default2019 screenset
- **Setup Wizard** - guided first-run configuration with live testing
- **Rack Management** - visual slot status with tool assignments
- **Safety Monitoring** - OSSD inputs, air pressure, sensor verification
- **Simulation Mode** - full cycle testing in UCCNC Demo mode
- **State Machine M6** - robust tool change with proper error handling
- **JSON Configuration** - settings stored next to profile, not in screenset

## Target Environment

- UCCNC 1.2118
- Controller: UC300ETH-UB1
- Testing: UCCNC Demo mode (works under Wine/Bottles on Linux)

## Build Instructions

### Prerequisites

1. Copy `Plugininterface.dll` from your UCCNC installation to `Plugin/lib/`:
   ```
   cp "C:\UCCNC\2118\Plugininterface.dll" Plugin/lib/
   ```

### Windows (Visual Studio)

1. Open `Plugin/src/HTM_ATC.csproj` in Visual Studio 2019+
2. Build → Build Solution (or press F6)
3. Output: `Plugin/src/bin/Release/HTM_ATC.dll`

### Windows (Command Line)

```cmd
cd Plugin\src
msbuild HTM_ATC.csproj /p:Configuration=Release
```

### Linux (Mono) - Development Only

```bash
cd Plugin/src
msbuild HTM_ATC.csproj /p:Configuration=Release
```

## Installation

1. Copy `HTM_ATC.dll` to `C:\UCCNC\2118\Plugins\`
2. Start UCCNC
3. Go to **Configure → General Settings → Plugin configuration**
4. Check the box next to "HTM_ATC" to enable it
5. Restart UCCNC
6. Access via **Plugins menu → HTM_ATC**

## M6.txt Setup

The plugin needs a small M6.txt macro to be called during tool changes. Copy this to your profile's macro folder:

**Location:** `C:\UCCNC\2118\Profile\Macro_YourProfile\M6.txt`

```csharp
// M6.txt - HTM_ATC Plugin Tool Change
int newTool = exec.Getnewtool();
object result = exec.Informplugin("HTM_ATC.dll", "M6:" + newTool);
if (result is string)
{
    string error = result as string;
    if (!string.IsNullOrEmpty(error))
    {
        exec.AddStatusmessage("ATC Error: " + error);
    }
}
```

## Testing in Demo Mode

1. Start UCCNC without a controller (Demo mode - LED 218 is ON)
2. Open the plugin via Plugins menu → HTM_ATC
3. Go to the **Simulation** tab
4. Check "Enable Simulation Mode"
5. Set simulated slot states (which slots have tools)
6. Click "Run Test Tool Change Cycle"
7. Check the **Status** tab log for the state machine output

## First Run Setup

When no configuration exists, the **Setup Wizard** launches automatically:

1. **Welcome** - import existing HTM_ATC profile or start fresh
2. **Rack Orientation** - X or Y axis alignment
3. **Slot 1 Position** - jog to slot 1 and read position
4. **Slot Pitch** - distance between slots
5. **Z Heights** - safe, empty, approach, clamp, check heights
6. **Drawbar Output** - port/pin with live test
7. **Air Blow** - optional output configuration
8. **Pressure Sensor** - optional input configuration
9. **Slot Sensors** - pin mapping for presence detection
10. **Enable Slots** - which slots are physically present
11. **Feeds & Timing** - speeds and delays
12. **Tool Change Position** - manual load/unload position
13. **Options** - G43, return to XY, measure non-RH tools
14. **Summary** - review all settings before saving

## Configuration Storage

Settings are stored as JSON in the UCCNC Profiles folder:
```
C:\UCCNC\2118\Profiles\HTM_ATC_Config_YourProfile.json
```

For example, if your profile is `ATCdev.pro`, the config file will be:
```
C:\UCCNC\2118\Profiles\HTM_ATC_Config_ATCdev.json
```

## Migrating from HTM_ATC Screenset

The wizard can import settings from an existing HTM_ATC profile:

1. In the wizard, click "Import from HTM_ATC Profile..."
2. Select your `.pro` file
3. Review imported values
4. Complete the wizard

## Known Improvements over Original Macros

- Always moves to Safe Z before any XY movement
- Verifies spindle actually stopped (not just wait time)
- G31 probing uses machine coordinates (G53)
- Checks LED 244 (ProbedOK) after probing
- Uses InvariantCulture for all number parsing
- Safety faults call exec.Stop() not just show messages
- Settings saved immediately, not on form close
- Modal state and feed rate restored after tool change

## Documentation

See `Plugin/docs/SPEC.md` for:
- Complete architecture
- M6 state machine diagram
- I/O model
- Configuration format
- Known bugs avoided
- Open questions for owner

## License

Same as HTM_ATC project.
