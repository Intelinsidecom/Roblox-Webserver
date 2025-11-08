using System;
using RCCArbiter;

namespace Examples
{
    /// <summary>
    /// Simple example demonstrating how to use the RCC Client programmatically
    /// </summary>
    public class SimpleExample
    {
        public static void Main(string[] args)
        {
            // Create client instance
            using (var client = new RCCClient("http://localhost:53640"))
            {
                // Example 1: Simple Hello World
                Console.WriteLine("=== Example 1: Hello World ===");
                ExecuteSimpleScript(client, "print('Hello World!')");

                // Example 2: Return values
                Console.WriteLine("\n=== Example 2: Return Values ===");
                var results = ExecuteSimpleScript(client, "return 'Success', 42, true");
                PrintResults(results);

                // Example 3: Math operations
                Console.WriteLine("\n=== Example 3: Math Operations ===");
                results = ExecuteSimpleScript(client, "return 10 + 20, 5 * 6, 100 / 4");
                PrintResults(results);

                // Example 4: String manipulation
                Console.WriteLine("\n=== Example 4: String Manipulation ===");
                results = ExecuteSimpleScript(client, @"
                    local str = 'Hello Roblox'
                    return str, string.upper(str), string.len(str)
                ");
                PrintResults(results);

                // Example 5: Tables
                Console.WriteLine("\n=== Example 5: Tables ===");
                results = ExecuteSimpleScript(client, @"
                    return {
                        name = 'Test',
                        value = 123,
                        active = true
                    }
                ");
                PrintResults(results);

                // Example 6: Persistent Job
                Console.WriteLine("\n=== Example 6: Persistent Job ===");
                PersistentJobExample(client);
            }
        }

        static LuaValue[] ExecuteSimpleScript(RCCClient client, string luaCode)
        {
            var job = new Job
            {
                id = Guid.NewGuid().ToString(),
                expirationInSeconds = 60,
                category = 0,
                cores = 1
            };

            var script = new ScriptExecution
            {
                name = "Example",
                script = luaCode
            };

            try
            {
                return client.BatchJobEx(job, script);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new LuaValue[0];
            }
        }

        static void PersistentJobExample(RCCClient client)
        {
            string jobId = "PersistentJob_" + Guid.NewGuid().ToString();

            try
            {
                // Open job with initialization script
                var job = new Job
                {
                    id = jobId,
                    expirationInSeconds = 120,
                    category = 0,
                    cores = 1
                };

                var initScript = new ScriptExecution
                {
                    name = "Initialize",
                    script = @"
                        _G.counter = 0
                        print('Job initialized')
                    "
                };

                Console.WriteLine($"Opening job: {jobId}");
                client.OpenJobEx(job, initScript);

                // Execute multiple scripts in the same job
                for (int i = 1; i <= 3; i++)
                {
                    var execScript = new ScriptExecution
                    {
                        name = $"Execute_{i}",
                        script = @"
                            _G.counter = _G.counter + 1
                            print('Counter: ' .. _G.counter)
                            return _G.counter
                        "
                    };

                    Console.WriteLine($"Executing script {i}...");
                    var results = client.ExecuteEx(jobId, execScript);
                    PrintResults(results);
                }

                // Close the job
                Console.WriteLine($"Closing job: {jobId}");
                client.CloseJob(jobId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                try
                {
                    client.CloseJob(jobId);
                }
                catch { }
            }
        }

        static void PrintResults(LuaValue[] results)
        {
            if (results == null || results.Length == 0)
            {
                Console.WriteLine("  (no return values)");
                return;
            }

            Console.WriteLine($"  Returned {results.Length} value(s):");
            for (int i = 0; i < results.Length; i++)
            {
                var value = results[i];
                Console.Write($"    [{i}] ");

                switch (value.type)
                {
                    case LuaType.LUA_TNIL:
                        Console.WriteLine("nil");
                        break;
                    case LuaType.LUA_TBOOLEAN:
                        Console.WriteLine($"(boolean) {value.value}");
                        break;
                    case LuaType.LUA_TNUMBER:
                        Console.WriteLine($"(number) {value.value}");
                        break;
                    case LuaType.LUA_TSTRING:
                        Console.WriteLine($"(string) \"{value.value}\"");
                        break;
                    case LuaType.LUA_TTABLE:
                        Console.WriteLine($"(table) with {value.table?.Length ?? 0} elements");
                        break;
                    default:
                        Console.WriteLine($"(unknown)");
                        break;
                }
            }
        }
    }
}
