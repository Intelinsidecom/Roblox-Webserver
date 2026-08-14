using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    public static class PortManager
    {
        private static readonly HashSet<int> _reservedPorts = new();
        private static readonly object _lock = new();

        private const int DefaultGameServerMinPort = 54000;
        private const int DefaultGameServerMaxPort = 55000;
        private const int DefaultRccMinPort = 50000;
        private const int DefaultRccMaxPort = 51000;

        private static int _gameServerMinPort = DefaultGameServerMinPort;
        private static int _gameServerMaxPort = DefaultGameServerMaxPort;
        private static int _rccMinPort = DefaultRccMinPort;
        private static int _rccMaxPort = DefaultRccMaxPort;

        public static int GameServerMinPort => _gameServerMinPort;
        public static int GameServerMaxPort => _gameServerMaxPort;
        public static int RccMinPort => _rccMinPort;
        public static int RccMaxPort => _rccMaxPort;

        /// <summary>
        /// Loads custom port ranges from the PortRanges config section.
        /// Falls back to defaults when values are missing or invalid.
        /// </summary>
        public static void Initialize(IConfiguration? configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _gameServerMinPort = ReadPort(configuration, "PortRanges:GameServerMin", DefaultGameServerMinPort);
            _gameServerMaxPort = ReadPort(configuration, "PortRanges:GameServerMax", DefaultGameServerMaxPort);
            ValidateRange("game server", _gameServerMinPort, _gameServerMaxPort, DefaultGameServerMinPort, DefaultGameServerMaxPort, ref _gameServerMinPort, ref _gameServerMaxPort);

            _rccMinPort = ReadPort(configuration, "PortRanges:RccMin", DefaultRccMinPort);
            _rccMaxPort = ReadPort(configuration, "PortRanges:RccMax", DefaultRccMaxPort);
            ValidateRange("RCC", _rccMinPort, _rccMaxPort, DefaultRccMinPort, DefaultRccMaxPort, ref _rccMinPort, ref _rccMaxPort);
        }

        private static int ReadPort(IConfiguration configuration, string key, int defaultValue)
        {
            var value = configuration[key];
            if (int.TryParse(value, out var parsed) && parsed >= 0)
            {
                return parsed;
            }
            return defaultValue;
        }

        private static void ValidateRange(string name, int min, int max, int defaultMin, int defaultMax, ref int outMin, ref int outMax)
        {
            if (min <= max)
            {
                return;
            }

            Console.WriteLine($"[PortManager] Invalid {name} port range (min={min}, max={max}). Using default {defaultMin}-{defaultMax}.");
            outMin = defaultMin;
            outMax = defaultMax;
        }

        public static int FindFreePort()
        {
            lock (_lock)
            {
                var random = new Random();
                var availablePorts = Enumerable.Range(_rccMinPort, _rccMaxPort - _rccMinPort + 1)
                    .Where(port => !_reservedPorts.Contains(port) && IsPortAvailable(port))
                    .ToList();

                if (availablePorts.Any())
                {
                    var selectedPort = availablePorts[random.Next(availablePorts.Count)];
                    _reservedPorts.Add(selectedPort);
                    return selectedPort;
                }

                Console.WriteLine("No available ports found for RCC allocation");
                return -1;
            }
        }

        public static int GenerateGameServerPort()
        {
            lock (_lock)
            {
                var random = new Random();
                var availablePorts = Enumerable.Range(_gameServerMinPort, _gameServerMaxPort - _gameServerMinPort + 1)
                    .Where(port => !_reservedPorts.Contains(port) && IsPortAvailable(port))
                    .ToList();

                if (availablePorts.Any())
                {
                    var selectedPort = availablePorts[random.Next(availablePorts.Count)];
                    _reservedPorts.Add(selectedPort);
                    return selectedPort;
                }

                Console.WriteLine("No available ports found for game server allocation");
                return -1;
            }
        }

        public static void ReleasePort(int port)
        {
            lock (_lock)
            {
                if (_reservedPorts.Remove(port))
                {
		// expand it, idk
                }
            }
        }

        public static bool ReservePort(int port)
        {
            lock (_lock)
            {
                if (!_reservedPorts.Contains(port) && IsPortAvailable(port))
                {
                    _reservedPorts.Add(port);
                    return true;
                }
                return false;
            }
        }

        private static bool IsPortAvailable(int port)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
