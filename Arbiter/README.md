# RCC Service Arbiter

A .NET SOAP client utility for communicating with the Roblox RCC (Roblox Compute Cloud) Service.

## Overview

This arbiter allows you to:
- Connect to an RCC Service instance via SOAP
- Execute Lua scripts on the RCC Service via console or HTTP API
- Manage jobs (open, close, renew leases)
- Query service status and diagnostics
- **NEW**: HTTP REST API for easy integration
- **NEW**: Configurable SOAP namespace and service URL

## Quick Start

### 1. Configure Settings

Edit `appsettings.json`:
```json
{
  "RCCService": {
    "ServiceUrl": "http://localhost:53640",
    "SoapNamespace": "http://economy-simulator.org/"
  },
  "HttpServer": {
    "Enabled": true,
    "Urls": "http://localhost:5000"
  }
}
```

**Important**: Change `ServiceUrl` and `SoapNamespace` to match your RCC Service.

### 2. Build

```bash
cd C:\Users\Intel\Documents\GitHub\RobloxWebserver\Arbiter
dotnet build
```

### 3. Run

**Console Mode:**
```bash
dotnet run
```

**HTTP Server Mode:**
```bash
dotnet run -- --server
```

**Custom RCC URL:**
```bash
dotnet run http://your-rcc-server:port
```

## Features

### 1. HTTP REST API (Server Mode)

When running in server mode, you get a REST API:

**POST /helloworld** - Execute Hello World script
```bash
curl -X POST http://localhost:5000/helloworld
```

**POST /execute** - Execute custom Lua script
```bash
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "return 1 + 1"
```

**GET /status** - Get RCC status
```bash
curl http://localhost:5000/status
```

**GET /version** - Get RCC version
```bash
curl http://localhost:5000/version
```

See [USAGE.md](USAGE.md) for complete API documentation.

### 2. Service Health Checks
- `HelloWorld()` - Basic connectivity test
- `GetVersion()` - Get RCC Service version
- `GetStatus()` - Get service status and environment count

### 3. Script Execution
- `BatchJob()` / `BatchJobEx()` - Execute a Lua script in a temporary job
- `OpenJob()` / `OpenJobEx()` - Create a persistent job and execute initialization script
- `Execute()` / `ExecuteEx()` - Execute script in an existing job

### 3. Job Management
- `RenewLease()` - Extend job expiration time
- `CloseJob()` - Close a specific job
- `GetExpiration()` - Get remaining time for a job
- `GetAllJobs()` / `GetAllJobsEx()` - List all active jobs
- `CloseExpiredJobs()` - Close all expired jobs
- `CloseAllJobs()` - Close all jobs

### 4. Diagnostics
- `Diag()` / `DiagEx()` - Get diagnostic information

## Example: Hello World

The program automatically executes a "Hello World" Lua script on startup:

```lua
print('Hello World from Lua!')
```

## Interactive Mode

After the initial tests, the program enters interactive mode where you can type Lua scripts directly:

```
Lua> print("Hello from interactive mode!")
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

## SOAP Protocol

The arbiter uses the SOAP protocol as defined in the RCC Service WSDL. The service contract includes:

- **Namespace**: `http://economy-simulator.org/`
- **Binding**: BasicHttpBinding
- **Message Format**: Document/Literal

## Data Types

### Job
- `id` (string) - Unique job identifier
- `expirationInSeconds` (double) - Job lifetime
- `category` (int) - Job category
- `cores` (double) - CPU cores allocated

### ScriptExecution
- `name` (string) - Script name
- `script` (string) - Lua script code
- `arguments` (LuaValue[]) - Script arguments

### LuaValue
- `type` (LuaType) - Value type (NIL, BOOLEAN, NUMBER, STRING, TABLE)
- `value` (string) - String representation of value
- `table` (LuaValue[]) - For table types, nested values

## Requirements

- .NET 8.0 or later
- Running RCC Service instance
- Network access to RCC Service endpoint

## Troubleshooting

### Connection Refused
Ensure the RCC Service is running:
```bash
# From the RCC Service directory
RCCService.exe -Console -port 53640
```

### SOAP Faults
Check the RCC Service logs for detailed error messages. Common issues:
- Invalid Lua syntax
- Script timeout
- Memory limits exceeded

## Architecture

The arbiter consists of:
- `RCCServiceContracts.cs` - SOAP service and data contracts
- `RCCClient.cs` - SOAP client wrapper
- `Program.cs` - Main application with interactive mode

## Future Enhancements

Potential additions:
- Script file loading
- Batch script execution from directory
- Job monitoring and auto-renewal
- Result logging to file
- Multi-RCC load balancing
