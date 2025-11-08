# RCC Arbiter - Usage Guide

## Configuration

The arbiter uses `appsettings.json` for configuration. You can customize:

### RCC Service Settings
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
  }
}
```

**Important**: Change `ServiceUrl` and `SoapNamespace` to match your RCC Service configuration.

### HTTP Server Settings
```json
{
  "HttpServer": {
    "Enabled": true,
    "Port": 5000,
    "Urls": "http://localhost:5000"
  }
}
```

## Running Modes

### 1. Interactive Console Mode (Default)

```bash
dotnet run
```

This mode:
- Tests connectivity with RCC Service
- Executes a Hello World script
- Provides an interactive Lua console

### 2. HTTP Server Mode

Enable in `appsettings.json`:
```json
"HttpServer": {
  "Enabled": true
}
```

Or run with flag:
```bash
dotnet run -- --server
```

### 3. Custom RCC URL

```bash
dotnet run http://your-rcc-server:port
```

Or in HTTP server mode:
```bash
dotnet run http://your-rcc-server:port -- --server
```

## HTTP API Endpoints

When running in HTTP server mode, the following endpoints are available:

### GET /
Get server information and available endpoints.

**Response:**
```json
{
  "service": "RCC Arbiter",
  "version": "1.0.0",
  "rccUrl": "http://localhost:53640",
  "endpoints": [
    "GET /",
    "POST /execute",
    "POST /helloworld",
    "GET /status",
    "GET /version"
  ]
}
```

### POST /helloworld
Execute a "Hello World" Lua script on the RCC Service.

**Request:**
```bash
curl -X POST http://localhost:5000/helloworld
```

**Response:**
```json
{
  "success": true,
  "jobId": "12345678-1234-1234-1234-123456789abc",
  "script": "print('Hello World from Lua!')",
  "results": []
}
```

### POST /execute
Execute a custom Lua script on the RCC Service.

**Request:**
```bash
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "return 1 + 1, 'test', true"
```

**Response:**
```json
{
  "success": true,
  "jobId": "12345678-1234-1234-1234-123456789abc",
  "script": "return 1 + 1, 'test', true",
  "results": [
    { "type": "number", "value": "2" },
    { "type": "string", "value": "test" },
    { "type": "boolean", "value": "true" }
  ]
}
```

### GET /status
Get RCC Service status.

**Request:**
```bash
curl http://localhost:5000/status
```

**Response:**
```json
{
  "success": true,
  "version": "1.0.0.0",
  "environmentCount": 0
}
```

### GET /version
Get RCC Service version.

**Request:**
```bash
curl http://localhost:5000/version
```

**Response:**
```json
{
  "success": true,
  "version": "1.0.0.0"
}
```

## Examples

### Example 1: Execute Hello World via HTTP

```bash
# Start the arbiter in server mode
dotnet run -- --server

# In another terminal, call the endpoint
curl -X POST http://localhost:5000/helloworld
```

### Example 2: Execute Custom Lua Script

```bash
# Execute a simple math operation
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "return 10 + 20"

# Execute a more complex script
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: text/plain" \
  -d "
local sum = 0
for i = 1, 10 do
  sum = sum + i
end
return sum
"
```

### Example 3: Use with PowerShell

```powershell
# Hello World
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/helloworld"

# Execute custom script
$script = "return 'Hello from PowerShell!'"
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/execute" -Body $script -ContentType "text/plain"

# Get status
Invoke-RestMethod -Method Get -Uri "http://localhost:5000/status"
```

### Example 4: Use from C# Application

```csharp
using System.Net.Http;
using System.Text;

var client = new HttpClient();
var baseUrl = "http://localhost:5000";

// Hello World
var helloResponse = await client.PostAsync($"{baseUrl}/helloworld", null);
var helloResult = await helloResponse.Content.ReadAsStringAsync();
Console.WriteLine(helloResult);

// Execute custom script
var script = "return 42, 'answer'";
var content = new StringContent(script, Encoding.UTF8, "text/plain");
var execResponse = await client.PostAsync($"{baseUrl}/execute", content);
var execResult = await execResponse.Content.ReadAsStringAsync();
Console.WriteLine(execResult);
```

## Changing SOAP Namespace

If your RCC Service uses a different SOAP namespace (not `http://economy-simulator.org/`), update `appsettings.json`:

```json
{
  "RCCService": {
    "SoapNamespace": "http://your-custom-namespace.com/"
  }
}
```

**Note**: The current implementation uses the namespace in the service contracts. If you need to change it frequently, you may need to regenerate the service contracts from your WSDL.

## Troubleshooting

### "Connection Refused"
- Ensure RCC Service is running
- Check `ServiceUrl` in `appsettings.json`
- Verify firewall settings

### "SOAP Fault: Namespace Mismatch"
- Update `SoapNamespace` in `appsettings.json` to match your RCC Service
- Regenerate service contracts if needed

### HTTP Server Port Already in Use
- Change `Port` or `Urls` in `appsettings.json`
- Or specify different port: `dotnet run -- --urls http://localhost:5001`

### Script Execution Timeout
- Increase timeout in `appsettings.json`:
  ```json
  "Timeout": {
    "SendMinutes": 20,
    "ReceiveMinutes": 20
  }
  ```

## Integration Examples

### ASP.NET Core Web Application

```csharp
// In your controller
[ApiController]
[Route("api/[controller]")]
public class LuaController : ControllerBase
{
    private readonly HttpClient _httpClient;
    
    public LuaController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteLua([FromBody] string script)
    {
        var content = new StringContent(script, Encoding.UTF8, "text/plain");
        var response = await _httpClient.PostAsync(
            "http://localhost:5000/execute", 
            content
        );
        
        var result = await response.Content.ReadAsStringAsync();
        return Ok(result);
    }
}
```

### JavaScript/Node.js

```javascript
const axios = require('axios');

async function executeHelloWorld() {
  const response = await axios.post('http://localhost:5000/helloworld');
  console.log(response.data);
}

async function executeCustomScript(script) {
  const response = await axios.post('http://localhost:5000/execute', script, {
    headers: { 'Content-Type': 'text/plain' }
  });
  console.log(response.data);
}

executeHelloWorld();
executeCustomScript("return 'Hello from Node.js!'");
```

### Python

```python
import requests

# Hello World
response = requests.post('http://localhost:5000/helloworld')
print(response.json())

# Execute custom script
script = "return 'Hello from Python!'"
response = requests.post(
    'http://localhost:5000/execute',
    data=script,
    headers={'Content-Type': 'text/plain'}
)
print(response.json())
```

## Advanced Configuration

### Multiple RCC Services

You can run multiple arbiter instances pointing to different RCC Services:

```bash
# Terminal 1 - RCC Service 1
dotnet run http://localhost:53640 -- --server --urls http://localhost:5000

# Terminal 2 - RCC Service 2
dotnet run http://localhost:53641 -- --server --urls http://localhost:5001
```

### Load Balancing

Use a reverse proxy (nginx, HAProxy) to load balance across multiple arbiter instances:

```nginx
upstream rcc_arbiters {
    server localhost:5000;
    server localhost:5001;
    server localhost:5002;
}

server {
    listen 80;
    location / {
        proxy_pass http://rcc_arbiters;
    }
}
```
