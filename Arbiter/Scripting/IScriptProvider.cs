using System.Collections.Generic;
using System.IO;

namespace RCCArbiter.Scripting
{
    public interface IScriptProvider
    {
        IEnumerable<string> ListScripts();
        bool TryGetScript(string name, out string scriptContent);
    }
}
