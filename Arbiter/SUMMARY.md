# RCC Arbiter - Project Summary

## What Was Created

A complete .NET 8.0 SOAP client arbiter for communicating with the Roblox RCC (Roblox Compute Cloud) Service, with both console and HTTP REST API modes.

## Key Features

### ✅ Configurable Settings
- **Service URL**: Change RCC Service endpoint via `appsettings.json` or command line
- **SOAP Namespace**: Configurable namespace (no need to hardcode `economy-simulator.org`)
- **Timeouts**: Adjustable send/receive timeouts
- **HTTP Server**: Enable/disable and configure HTTP API port

### ✅ Dual Operation Modes

#### 1. Console Mode (Interactive)
- Direct SOAP communication with RCC Service
- Interactive Lua script execution
- Real-time result display
- Perfect for testing and development

#### 2. HTTP Server Mode
- REST API for easy integration
- JSON responses
- Multiple endpoints for different operations
- Perfect for production integration

### ✅ HTTP API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | / | Server info and available endpoints |
| POST | /helloworld | Execute "Hello World" Lua script |
| POST | /execute | Execute custom Lua script (body: Lua code) |
| GET | /status | Get RCC Service status |
| GET | /version | Get RCC Service version |

### ✅ Complete SOAP Service Support

All RCC Service SOAP operations are supported:
- HelloWorld, GetVersion, GetStatus
- OpenJob, OpenJobEx - Create persistent jobs
- BatchJob, BatchJobEx - Execute temporary jobs
- Execute, ExecuteEx - Run scripts in existing jobs
- RenewLease, CloseJob, GetExpiration
- GetAllJobs, CloseExpiredJobs, CloseAllJobs
- Diag, DiagEx - Diagnostics

## Project Structure

```
Arbiter/
├── Program.cs                 # Main application with HTTP server
├── RCCClient.cs              # SOAP client wrapper
├── RCCServiceContracts.cs    # SOAP service/data contracts
├── RCCArbiter.csproj         # Project file
├── appsettings.json          # Configuration file
├── README.md                 # Main documentation
├── USAGE.md                  # Detailed usage guide
├── QUICKSTART.md             # Quick start guide
├── run.bat                   # Build and run script
├── Scripts/
│   ├── HelloWorld.lua        # Sample Hello World script
│   └── Advanced.lua          # Advanced example script
└── Examples/
    └── SimpleExample.cs      # Programmatic usage examples
```

## Configuration File (appsettings.json)

```json
{
  "RCCService": {
    "ServiceUrl": "http://localhost:53640",
    "SoapNamespace": "http://economy-simulator.org/",
    "Timeout": {
      "SendMinutes": 10,
      "ReceiveMinutes": 10
    },
    "DefaultJob": {
      "ExpirationSeconds": 60,
      "Category": 0,
      "Cores": 1.0
    }
  },
  "HttpServer": {
    "Enabled": true,
    "Port": 5000,
    "Urls": "http://localhost:5000"
  }
}
```

## Usage Examples

### Console Mode
```bash
# Use default settings from appsettings.json
dotnet run

# Override RCC Service URL
dotnet run http://your-rcc-server:port
```

### HTTP Server Mode
```bash
# Enable via appsettings.json or use flag
dotnet run -- --server

# Test Hello World endpoint
curl -X POST http://localhost:5000/helloworld

# Execute custom Lua script
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "print('Hello from HTTP!'); return 42"
```

### PowerShell Integration
```powershell
# Execute Hello World
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/helloworld"

# Execute custom script
$script = "return 'Hello from PowerShell!'"
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/execute" `
  -Body $script -ContentType "text/plain"
```

### C# Integration
```csharp
using var client = new HttpClient();

// Hello World
var response = await client.PostAsync(
    "http://localhost:5000/helloworld", 
    null
);
var result = await response.Content.ReadAsStringAsync();

// Custom script
var script = "return 1 + 1";
var content = new StringContent(script, Encoding.UTF8, "text/plain");
response = await client.PostAsync(
    "http://localhost:5000/execute", 
    content
);
```

## Why This Approach?

### ✅ Configurable
- No hardcoded URLs or namespaces
- Easy to adapt to different RCC Service configurations
- Change settings without recompiling

### ✅ Flexible
- Console mode for development/testing
- HTTP API for production integration
- Both modes use the same underlying SOAP client

### ✅ Easy Integration
- REST API is language-agnostic
- JSON responses are easy to parse
- Works with curl, PowerShell, C#, Python, JavaScript, etc.

### ✅ Production Ready
- Proper error handling
- Configurable timeouts
- Async/await patterns
- Disposable resources

## Technical Details

### Technologies Used
- **.NET 8.0**: Modern .NET platform
- **System.ServiceModel**: SOAP/WCF client support
- **ASP.NET Core**: HTTP server and minimal APIs
- **Microsoft.Extensions.Configuration**: Configuration management

### SOAP Communication
- Uses `BasicHttpBinding` for SOAP 1.1
- Supports large messages (2GB max)
- Configurable timeouts
- Proper channel lifecycle management

### Data Contracts
All RCC Service data types are properly mapped:
- `Job` - Job configuration
- `ScriptExecution` - Lua script with arguments
- `LuaValue` - Lua return values (nil, boolean, number, string, table)
- `Status` - Service status information

## Next Steps

### For Testing
1. Start your RCC Service
2. Update `appsettings.json` with correct URL and namespace
3. Run in console mode: `dotnet run`
4. Test with Hello World script

### For Production
1. Configure `appsettings.json` for your environment
2. Enable HTTP server mode
3. Run: `dotnet run -- --server`
4. Integrate with your application via HTTP API

### For Development
1. Use the `RCCClient` class directly in your C# code
2. See `Examples/SimpleExample.cs` for usage patterns
3. Extend with additional endpoints as needed

## Documentation

- **README.md** - Overview and quick start
- **USAGE.md** - Complete API documentation with examples
- **QUICKSTART.md** - Step-by-step getting started guide
- **SUMMARY.md** - This file

## Support

The arbiter is based on the RCC Service WSDL specification found at:
`F:\roblox-2016-source-code\RCCService\gSOAP\RCCService.wsdl`

For RCC Service implementation details, see:
`F:\roblox-2016-source-code\RCCService\`

## License

This is a utility tool for communicating with Roblox RCC Service. Use according to your Roblox licensing terms.
