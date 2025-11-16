using System;
using System.Diagnostics;
using System.IO;
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

        public RCCProcessManager(IConfiguration configuration, string configSection = "Rendering")
        {
            _year = configuration[$"{configSection}:Year"] ?? "2016";
            _port = int.Parse(configuration[$"{configSection}:Port"] ?? "64989");
            _executable = configuration[$"{configSection}:Executable"] ?? "RCCService.exe";
            _arguments = configuration[$"{configSection}:Arguments"] ?? "-console";

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

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = fullArguments,
                    WorkingDirectory = _workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
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
        /// Stop the RCC process
        /// </summary>
        public void Stop()
        {
            if (_process == null || _process.HasExited)
            {
                return;
            }

            Console.WriteLine($"Stopping RCC ({_year})...");
            
            try
            {
                _process.Kill();
                _process.WaitForExit(5000);
                Console.WriteLine($"RCC ({_year}) stopped.");
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

        public void Dispose()
        {
            Stop();
        }
    }
}
