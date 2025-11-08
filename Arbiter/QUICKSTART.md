# RCC Arbiter - Quick Start Guide

## Prerequisites

1. **RCC Service Running**: You need a running RCC Service instance
2. **.NET 8.0 SDK**: Install from https://dotnet.microsoft.com/download

## Step 1: Start the RCC Service

From the RCC Service source directory:

```bash
cd F:\roblox-2016-source-code\RCCService
RCCService.exe -Console -port 53640
```

You should see:
```
Service starting...
Service Started on port 53640
```

## Step 2: Build the Arbiter

```bash
cd C:\Users\Intel\Documents\GitHub\RobloxWebserver\Arbiter\RCC
dotnet build
```

Or simply run:
```bash
run.bat
```

## Step 3: Run the Arbiter

```bash
dotnet run
```

## What Happens

The arbiter will:

1. **Connect** to the RCC Service at `http://localhost:53640`
2. **Test HelloWorld** - Verify basic connectivity
3. **Get Version** - Display RCC Service version
4. **Get Status** - Show service status
5. **Execute Lua Script** - Run a "Hello World" Lua script
6. **Enter Interactive Mode** - Allow you to type Lua scripts

## Example Session

```
=== RCC Service Arbiter ===

Connecting to RCC Service at: http://localhost:53640

--- Test 1: HelloWorld ---
HelloWorld Result: Hello World

--- Test 2: GetVersion ---
RCC Version: 1.0.0.0

--- Test 3: GetStatus ---
Version: 1.0.0.0
Environment Count: 0

--- Test 4: Execute Lua Script (Hello World) ---
Executing Lua script: print('Hello World from Lua!')
Job ID: 12345678-1234-1234-1234-123456789abc

Script executed successfully!
Returned 0 value(s):

--- Interactive Mode ---
Enter Lua scripts to execute (or 'exit' to quit):

Lua> print("Hello from interactive mode!")
(no return values)

Lua> return 1 + 1
Results:
  [0] (number) 2

Lua> return "test", 42, true
Results:
  [0] (string) "test"
  [1] (number) 42
  [2] (boolean) true

Lua> exit
```

## Executing Script Files

To execute a Lua script from a file, you can modify the Program.cs or use the interactive mode:

1. Create your Lua script in `Scripts/` folder
2. In interactive mode, paste the script content
3. Or modify Program.cs to load from file

## Common Issues

### "Connection Refused"
- Ensure RCC Service is running
- Check the port number (default: 53640)
- Verify firewall settings

### "SOAP Fault"
- Check Lua syntax
- Verify script doesn't exceed timeout
- Check RCC Service logs for details

### "Build Failed"
- Ensure .NET 8.0 SDK is installed
- Run `dotnet --version` to verify
- Check for missing NuGet packages

## Next Steps

1. **Explore Scripts**: Check the `Scripts/` folder for examples
2. **Modify Program.cs**: Customize the arbiter for your needs
3. **Read README.md**: Full documentation of all features
4. **Check RCC WSDL**: `F:\roblox-2016-source-code\RCCService\gSOAP\RCCService.wsdl`

## Advanced Usage

### Custom RCC URL
```bash
dotnet run http://your-server:port
```

### Job Management
```csharp
// Open a persistent job
var job = new Job { id = "MyJob", expirationInSeconds = 300, category = 0, cores = 1 };
var script = new ScriptExecution { name = "Init", script = "print('Initialized')" };
client.OpenJobEx(job, script);

// Execute in existing job
var execScript = new ScriptExecution { name = "Execute", script = "return 42" };
var results = client.ExecuteEx("MyJob", execScript);

// Close job
client.CloseJob("MyJob");
```

### Batch Processing
```csharp
// BatchJob creates and closes job automatically
var job = new Job { id = "Batch1", expirationInSeconds = 60, category = 0, cores = 1 };
var script = new ScriptExecution { name = "Batch", script = "return 1 + 1" };
var results = client.BatchJobEx(job, script);
```

## Architecture Overview

```
┌─────────────────┐         SOAP/HTTP          ┌─────────────────┐
│   RCC Arbiter   │ ◄────────────────────────► │   RCC Service   │
│   (.NET Client) │                             │   (C++ SOAP)    │
└─────────────────┘                             └─────────────────┘
                                                         │
                                                         ▼
                                                ┌─────────────────┐
                                                │  Lua Execution  │
                                                │   Environment   │
                                                └─────────────────┘
```

## Support

For issues or questions:
1. Check RCC Service logs
2. Review WSDL documentation
3. Examine source code in `F:\roblox-2016-source-code\RCCService\`
