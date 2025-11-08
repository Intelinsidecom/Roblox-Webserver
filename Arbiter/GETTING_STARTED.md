# Getting Started with RCC Arbiter

This guide will help you get the RCC Arbiter up and running quickly.

## Prerequisites

1. **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download
2. **RCC Service** - A running instance of the Roblox RCC Service
3. **Text Editor** - Any editor to modify `appsettings.json`

## Step-by-Step Setup

### Step 1: Configure the Arbiter

Open `appsettings.json` and update these settings:

```json
{
  "RCCService": {
    "ServiceUrl": "http://localhost:53640",
    "SoapNamespace": "http://economy-simulator.org/"
  }
}
```

**Important Changes:**
- `ServiceUrl`: Set this to your RCC Service endpoint
- `SoapNamespace`: Set this to match your RCC Service SOAP namespace

If you don't know your SOAP namespace, check your RCC Service WSDL file or leave it as the default.

### Step 2: Build the Project

Open a terminal in the Arbiter directory and run:

```bash
dotnet build
```

You should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 3: Choose Your Mode

#### Option A: Console Mode (For Testing)

Run the arbiter in interactive console mode:

```bash
dotnet run
```

You'll see:
```
=== RCC Service Arbiter ===

Connecting to RCC Service at: http://localhost:53640

--- Test 1: HelloWorld ---
HelloWorld Result: Hello World

--- Test 2: GetVersion ---
RCC Version: 1.0.0.0

...
```

After the tests, you can type Lua scripts interactively:
```
Lua> print("Hello!")
Lua> return 1 + 1
Results:
  [0] (number) 2
Lua> exit
```

#### Option B: HTTP Server Mode (For Integration)

Run the arbiter as an HTTP server:

```bash
dotnet run -- --server
```

You'll see:
```
=== HTTP Server Mode ===

HTTP Server listening on: http://localhost:5000
RCC Service URL: http://localhost:53640

Available endpoints:
  GET  /              - Server info
  POST /execute       - Execute Lua script (body: Lua code)
  POST /helloworld    - Execute Hello World script
  GET  /status        - Get RCC status
  GET  /version       - Get RCC version
```

The server will keep running until you press Ctrl+C.

### Step 4: Test the API (Server Mode Only)

#### Using PowerShell:

```powershell
# Test Hello World
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/helloworld"

# Execute custom script
$script = "return 1 + 1"
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/execute" `
  -Body $script -ContentType "text/plain"
```

Or run the test script:
```powershell
.\test-api.ps1
```

#### Using curl (Linux/Mac/Windows):

```bash
# Test Hello World
curl -X POST http://localhost:5000/helloworld

# Execute custom script
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "return 1 + 1"
```

Or run the test script:
```bash
chmod +x test-api.sh
./test-api.sh
```

## Common Scenarios

### Scenario 1: Testing Lua Scripts

**Goal**: Test Lua scripts before deploying to production.

**Solution**: Use console mode
```bash
dotnet run
```

Type your scripts at the `Lua>` prompt and see results immediately.

### Scenario 2: Integrating with Web Application

**Goal**: Execute Lua scripts from your web application.

**Solution**: Use HTTP server mode
```bash
dotnet run -- --server
```

From your web app, make HTTP POST requests to `/execute`:
```csharp
var client = new HttpClient();
var script = "return game:GetService('Players').MaxPlayers";
var content = new StringContent(script, Encoding.UTF8, "text/plain");
var response = await client.PostAsync("http://localhost:5000/execute", content);
var result = await response.Content.ReadAsStringAsync();
```

### Scenario 3: Different RCC Service

**Goal**: Connect to a different RCC Service instance.

**Solution**: Override URL via command line
```bash
dotnet run http://192.168.1.100:53640 -- --server
```

Or update `appsettings.json`:
```json
{
  "RCCService": {
    "ServiceUrl": "http://192.168.1.100:53640"
  }
}
```

### Scenario 4: Custom SOAP Namespace

**Goal**: Your RCC Service uses a different namespace.

**Solution**: Update `appsettings.json`:
```json
{
  "RCCService": {
    "SoapNamespace": "http://your-custom-namespace.com/"
  }
}
```

## Troubleshooting

### Problem: "Connection Refused"

**Symptoms:**
```
Fatal Error: No connection could be made because the target machine actively refused it.
```

**Solutions:**
1. Verify RCC Service is running
2. Check the `ServiceUrl` in `appsettings.json`
3. Test connectivity: `curl http://localhost:53640`
4. Check firewall settings

### Problem: "SOAP Fault: Namespace Mismatch"

**Symptoms:**
```
The message with Action '' cannot be processed...
```

**Solutions:**
1. Update `SoapNamespace` in `appsettings.json`
2. Check your RCC Service WSDL for the correct namespace
3. Ensure namespace ends with `/`

### Problem: "Port Already in Use"

**Symptoms:**
```
Failed to bind to address http://localhost:5000
```

**Solutions:**
1. Change port in `appsettings.json`:
   ```json
   "HttpServer": {
     "Urls": "http://localhost:5001"
   }
   ```
2. Or specify via command line:
   ```bash
   dotnet run -- --server --urls http://localhost:5001
   ```

### Problem: Script Timeout

**Symptoms:**
```
The request channel timed out while waiting for a reply
```

**Solutions:**
1. Increase timeout in `appsettings.json`:
   ```json
   "Timeout": {
     "SendMinutes": 20,
     "ReceiveMinutes": 20
   }
   ```
2. Optimize your Lua script
3. Check RCC Service logs for errors

## Next Steps

### For Development
- Read [USAGE.md](USAGE.md) for complete API documentation
- Check [Examples/SimpleExample.cs](Examples/SimpleExample.cs) for code examples
- Explore sample scripts in [Scripts/](Scripts/) folder

### For Production
- Configure proper timeouts
- Set up monitoring/logging
- Consider load balancing multiple arbiter instances
- Implement authentication/authorization if needed

### For Advanced Usage
- Create custom endpoints in `Program.cs`
- Extend `RCCClient.cs` with helper methods
- Implement job management features
- Add persistent job tracking

## Quick Reference

### Console Mode Commands
```bash
# Default (from appsettings.json)
dotnet run

# Custom URL
dotnet run http://your-server:port
```

### HTTP Server Mode Commands
```bash
# Enable via appsettings.json
dotnet run -- --server

# Custom URL
dotnet run http://your-server:port -- --server

# Custom port
dotnet run -- --server --urls http://localhost:5001
```

### API Endpoints
```
GET  /              - Server info
POST /helloworld    - Execute Hello World
POST /execute       - Execute custom Lua (body: Lua code)
GET  /status        - RCC status
GET  /version       - RCC version
```

### Configuration File Location
```
C:\Users\Intel\Documents\GitHub\RobloxWebserver\Arbiter\appsettings.json
```

## Support

For more information:
- **README.md** - Project overview
- **USAGE.md** - Complete API documentation
- **SUMMARY.md** - Technical details
- **QUICKSTART.md** - Quick start guide

RCC Service source code:
- `F:\roblox-2016-source-code\RCCService\`
