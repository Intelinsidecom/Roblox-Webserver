# Multi-Version RCC Setup Guide

The Arbiter now supports running different RCC versions for different purposes (rendering, game servers, etc.).

## Directory Structure

Organize your RCC binaries by year in the `RCC` directory:

```
Arbiter/
├── RCC/
│   ├── 2016/
│   │   ├── RCCService.exe
│   │   ├── content/
│   │   ├── platformcontent/
│   │   └── ... (all RCC dependencies)
│   ├── 2017/
│   │   ├── RCCService.exe
│   │   └── ...
│   └── 2018/
│       ├── Bootstrapper.exe
│       └── ...
├── appsettings.json
└── ...
```

## Configuration

### Rendering RCC (Interactive Mode)

Edit `appsettings.json` to configure the rendering RCC:

```json
{
  "Rendering": {
    "Year": "2016",
    "Port": 64989,
    "Executable": "RCCService.exe",
    "Arguments": "-console",
    "AutoStart": true
  }
}
```

**Configuration Options:**

- **Year**: Directory name under `RCC/` (e.g., "2016", "2017")
- **Port**: Port number for the RCC service
- **Executable**: RCC executable filename (e.g., "RCCService.exe", "Bootstrapper.exe")
- **Arguments**: Command-line arguments (without `-port`, which is added automatically)
- **AutoStart**: If `true`, automatically starts RCC in interactive mode

### Example Configurations

**Using RCCService.exe:**
```json
"Rendering": {
  "Year": "2016",
  "Port": 64989,
  "Executable": "RCCService.exe",
  "Arguments": "-console",
  "AutoStart": true
}
```

**Using Bootstrapper.exe:**
```json
"Rendering": {
  "Year": "2018",
  "Port": 64990,
  "Executable": "Bootstrapper.exe",
  "Arguments": "",
  "AutoStart": true
}
```

**Using a specific RCC version:**
```json
"Rendering": {
  "Year": "2017",
  "Port": 64991,
  "Executable": "0.280.0.41501.exe",
  "Arguments": "-console",
  "AutoStart": true
}
```

## Usage

### Interactive Mode (with Auto-Start)

When `AutoStart` is `true`, the Arbiter will automatically launch the configured RCC:

```bash
dotnet run
```

Output:
```
=== RCC Service Arbiter ===

Starting RCC (2016)...
  Executable: C:\...\Arbiter\RCC\2016\RCCService.exe
  Arguments: -console -port 64989
  Working Dir: C:\...\Arbiter\RCC\2016
  Service URL: http://localhost:64989

RCC (2016) started with PID: 12345
RCC (2016) is running on port 64989

Interactive Mode - Connecting to RCC Service at: http://localhost:64989

Interactive Mode
Type Lua and press Enter to execute.
Type 'exit' to quit.
Shortcut: press 'o' to run Scripts/HelloWorld.lua if present.

Lua>
```

The RCC process will automatically shut down when you exit the Arbiter.

### Interactive Mode (Manual RCC)

Set `AutoStart` to `false` to manually manage RCC:

```json
"Rendering": {
  "AutoStart": false
}
```

Then start RCC manually and run the Arbiter:

```bash
# Terminal 1: Start RCC manually
cd RCC\2016
RCCService.exe -console -port 64989

# Terminal 2: Run Arbiter
dotnet run
```

### HTTP Server Mode

HTTP mode uses the `RCCService:ServiceUrl` configuration (not affected by Rendering config):

```bash
dotnet run -- --http
```

Or enable in config:
```json
"HttpServer": {
  "Enabled": true
}
```

## Future Expansion

The system is designed to support multiple RCC configurations:

```json
{
  "Rendering": {
    "Year": "2016",
    "Port": 64989,
    "Executable": "RCCService.exe",
    "Arguments": "-console",
    "AutoStart": true
  },
  "GameServer": {
    "Year": "2018",
    "Port": 53640,
    "Executable": "Bootstrapper.exe",
    "Arguments": "",
    "AutoStart": false
  },
  "Thumbnails": {
    "Year": "2016",
    "Port": 64988,
    "Executable": "RCCService.exe",
    "Arguments": "-console",
    "AutoStart": false
  }
}
```

## Troubleshooting

### Directory Not Found
```
RCC directory not found: C:\...\Arbiter\RCC\2016
Please create the directory structure: RCC/2016/ and place the RCC binaries there.
```

**Solution:** Create the directory and copy all RCC files:
```bash
mkdir RCC\2016
# Copy RCCService.exe and all dependencies to RCC\2016\
```

### Executable Not Found
```
RCC executable not found: C:\...\Arbiter\RCC\2016\RCCService.exe
Please place RCCService.exe in RCC/2016/
```

**Solution:** Ensure the executable name matches the configuration.

### Port Already in Use

If the port is already in use, change it in `appsettings.json`:
```json
"Rendering": {
  "Port": 64990
}
```

### RCC Exits Immediately
```
RCC process exited immediately with code: -1
```

**Solution:** Check that all RCC dependencies (DLLs, content folders) are present in the RCC directory.

## Migration from Old Setup

If you have RCC files in `Arbiter/RCC/` directly:

1. Create year directory: `mkdir RCC\2016`
2. Move files: `move RCC\*.* RCC\2016\`
3. Update `appsettings.json` with `"Year": "2016"`

## Benefits

- **Version Isolation**: Different RCC versions for different purposes
- **Easy Switching**: Change year in config to use different RCC
- **Auto-Management**: RCC starts/stops automatically in interactive mode
- **Future-Proof**: Ready for game server and thumbnail RCC configurations
