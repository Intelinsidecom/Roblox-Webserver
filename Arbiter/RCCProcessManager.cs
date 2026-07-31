using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    /// <summary>
    /// Manages RCC process lifecycle for different versions/purposes
    /// </summary>
    public class RCCProcessManager : IDisposable
    {
        private Process? _process;
        private readonly string _year;
        private readonly int _port;
        private readonly string _executable;
        private readonly string _arguments;
        private readonly string _workingDirectory;
        private readonly bool _separateWindow;
        private bool _forceSeparateWindow;
        private Timer? _healthWatchdog;
        private readonly object _watchdogLock = new();
        private int _busyCount;
        private bool _disposed;

        public RCCProcessManager(IConfiguration configuration, string configSection = "Rendering")
        {
            _year = configuration[$"{configSection}:Year"] ?? "2016";
            _port = int.Parse(configuration[$"{configSection}:Port"] ?? "64989");
            _executable = configuration[$"{configSection}:Executable"] ?? "RCCService.exe";
            _arguments = configuration[$"{configSection}:Arguments"] ?? "-console";
            _separateWindow = bool.TryParse(configuration[$"{configSection}:SeparateWindow"], out var sw) && sw;

            // Build path: RCC/{Year}/{Executable}
            var baseDir = Directory.GetCurrentDirectory();
            _workingDirectory = Path.Combine(baseDir, "RCC", _year);

            if (!Directory.Exists(_workingDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"RCC directory not found: {_workingDirectory}\n" +
                    $"Please create the directory structure: RCC/{_year}/ and place the RCC binaries there.");
            }

            var executablePath = Path.Combine(_workingDirectory, _executable);
            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException(
                    $"RCC executable not found: {executablePath}\n" +
                    $"Please place {_executable} in RCC/{_year}/");
            }
        }

        public RCCProcessManager(IConfiguration configuration, string configSection, int overridePort)
            : this(configuration, configSection)
        {
            _port = overridePort;
        }

        public string Year => _year;
        public int Port => _port;
        public string ServiceUrl => $"http://localhost:{_port}";
        public bool IsRunning => _process != null && !_process.HasExited;
        
        public int BusyCount => _busyCount;

        public void IncrementBusy() => Interlocked.Increment(ref _busyCount);

        public void DecrementBusy() => Interlocked.Decrement(ref _busyCount);

        public bool ForceSeparateWindow
        {
            get => _forceSeparateWindow;
            set => _forceSeparateWindow = value;
        }

        /// <summary>
        /// Enable health watchdog that auto-restarts the process if it dies
        /// </summary>
        public void EnableAutoRestart(TimeSpan? checkInterval = null)
        {
            var interval = checkInterval ?? TimeSpan.FromSeconds(15);
            lock (_watchdogLock)
            {
                _healthWatchdog?.Dispose();
                _healthWatchdog = new Timer(HealthWatchdogCallback, null, interval, interval);
            }
        }

        /// <summary>
        /// Disable the health watchdog
        /// </summary>
        public void DisableAutoRestart()
        {
            lock (_watchdogLock)
            {
                _healthWatchdog?.Dispose();
                _healthWatchdog = null;
            }
        }

        private void HealthWatchdogCallback(object? state)
        {
            if (_disposed)
                return;

            if (!IsRunning)
            {
                if (_busyCount > 0)
                {
                    Console.WriteLine($"[Watchdog] RCC ({_year}) on port {_port} is dead but has {_busyCount} active requests, skipping restart (will restart when requests drain)");
                    return;
                }

                Console.WriteLine($"[Watchdog] RCC ({_year}) on port {_port} is dead, restarting...");
                try
                {
                    Stop();
                    Start();
                    Console.WriteLine($"[Watchdog] RCC ({_year}) restarted successfully on port {_port}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Watchdog] Failed to restart RCC ({_year}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Start the RCC process
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                Console.WriteLine($"RCC ({_year}) is already running on port {_port}");
                return;
            }

            var executablePath = Path.Combine(_workingDirectory, _executable);
            var fullArguments = $"{_arguments} -port {_port}";

            Console.WriteLine($"Starting RCC ({_year})...");
            Console.WriteLine($"  Executable: {executablePath}");
            Console.WriteLine($"  Arguments: {fullArguments}");
            Console.WriteLine($"  Working Dir: {_workingDirectory}");
            Console.WriteLine($"  Service URL: {ServiceUrl}");
            Console.WriteLine();

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = fullArguments,
                WorkingDirectory = _workingDirectory,
                UseShellExecute = _forceSeparateWindow || _separateWindow,
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            _process = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _process.Start();
                Console.WriteLine($"RCC ({_year}) started with PID: {_process.Id}");
                
                // Give it a moment to start up
                System.Threading.Thread.Sleep(2000);
                
                if (_process.HasExited)
                {
                    throw new Exception($"RCC process exited immediately with code: {_process.ExitCode}");
                }
                
                if (!WaitForPort(_port, TimeSpan.FromSeconds(10)))
                {
                    Stop();
                    throw new Exception($"RCC ({_year}) started but is not listening on port {_port} after verification timeout");
                }
                
                Console.WriteLine($"RCC ({_year}) is running on port {_port}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start RCC: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop the RCC process with escalating force if needed
        /// </summary>
        public void Stop()
        {
            if (_process == null)
            {
                return;
            }

            if (_process.HasExited)
            {
                _process.Dispose();
                _process = null;
                return;
            }

            var pid = _process.Id;
            Console.WriteLine($"Stopping RCC ({_year}) PID {pid}...");
            
            try
            {
                _process.Kill(entireProcessTree: true);
                var exited = _process.WaitForExit(15000);
                
                if (!exited && !_process.HasExited)
                {
                    try
                    {
                        KillProcessTree(pid);
                    }
                    catch (Exception treeEx)
                    {
                        Console.WriteLine($"Warning: Failed to kill process tree: {treeEx.Message}");
                    }
                    
                    _process.WaitForExit(5000);
                }
                
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"RCC ({_year}) already exited.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping RCC: {ex.Message}");
            }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }

        /// <summary>
        /// Kill a process and all its children recursively
        /// </summary>
        private void KillProcessTree(int processId)
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {processId} /T /F",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    
                    using var process = Process.Start(psi);
                    process?.WaitForExit(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"taskkill failed: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "pkill",
                        Arguments = $"-P {processId}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var pkill = Process.Start(psi);
                    pkill?.WaitForExit(2000);
                }
                catch (Exception ex) { Console.WriteLine($"[ERROR] KillProcessTree pkill -P {processId}: {ex}"); }
            }
        }

        private static bool WaitForPort(int port, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                try
                {
                    using var client = new TcpClient();
                    var result = client.BeginConnect(IPAddress.Loopback, port, null, null);
                    if (result.AsyncWaitHandle.WaitOne(500))
                    {
                        client.EndConnect(result);
                        if (client.Connected)
                            return true;
                    }
                }
                catch
                {
                }
            }
            return false;
        }

        public void Dispose()
        {
            _disposed = true;
            lock (_watchdogLock)
            {
                _healthWatchdog?.Dispose();
                _healthWatchdog = null;
            }
            Stop();
            PortManager.ReleasePort(_port);
        }
    }
}
