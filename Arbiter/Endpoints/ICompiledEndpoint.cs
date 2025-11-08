using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public interface ICompiledEndpoint
    {
        string Route { get; }
        string ScriptName { get; }
        void SetConfiguration(IConfiguration configuration);
        IDictionary<string, string> MapParameters(HttpRequest req);
    }
}
