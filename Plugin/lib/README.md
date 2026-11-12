# Reference Assemblies

This folder should contain the UCCNC reference assemblies needed to compile the HTM_ATC plugin.

## Required Files

Copy the following files from your UCCNC 1.2118 installation:

1. **Plugininterface.dll** - Plugin API interface
   - Source: `C:\UCCNC\2118\Plugininterface.dll`

**DO NOT** copy UCCNC.exe or UC100.dll - these are not needed for compilation and should not be committed to version control.

## Setup Instructions

### Windows (Visual Studio)
1. Copy `Plugininterface.dll` to this `lib/` folder
2. Open `Plugin/src/HTM_ATC.sln` in Visual Studio
3. Build the solution

### Windows (Command Line)
```cmd
copy "C:\UCCNC\2118\Plugininterface.dll" lib\
cd src
msbuild HTM_ATC.csproj /p:Configuration=Release
```

### Linux (Mono) - For Development Only
```bash
# Copy Plugininterface.dll from Windows UCCNC installation
cp /path/to/Plugininterface.dll lib/

# Build with Mono
cd src
msbuild HTM_ATC.csproj /p:Configuration=Release
```

### Linux (dotnet SDK) - For Development Only
```bash
# Requires .NET SDK with Framework targeting pack
cp /path/to/Plugininterface.dll lib/
cd src
dotnet build HTM_ATC.csproj -c Release
```

## Output

The compiled DLL will be placed in:
- Debug: `src/bin/Debug/HTM_ATC.dll`
- Release: `src/bin/Release/HTM_ATC.dll`

## Installation

1. Copy `HTM_ATC.dll` to `C:\UCCNC\2118\Plugins\`
2. Start UCCNC
3. Go to Configure → General Settings → Plugin configuration
4. Enable "HTM_ATC" plugin
5. Restart UCCNC
6. Access via Plugins menu → HTM_ATC

## Testing in Demo Mode

1. Start UCCNC in Demo mode (no controller connected)
2. The plugin will detect Demo mode (LED 218)
3. Use the Simulation tab to:
   - Enable simulation mode
   - Set simulated sensor states
   - Test the full tool change cycle without hardware
