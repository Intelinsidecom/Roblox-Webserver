using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RCCArbiter
{
    public static class PortManager
    {
        private static readonly HashSet<int> _reservedPorts = new();
        private static readonly object _lock = new();

        private const int GameServerMinPort = 54000;
        private const int GameServerMaxPort = 55000;
        private const int RccMinPort = 50000;
        private const int RccMaxPort = 51000;

        public static int FindFreePort()
        {
            lock (_lock)
            {
                var random = new Random();
                var availablePorts = Enumerable.Range(RccMinPort, RccMaxPort - RccMinPort + 1)
                    .Where(port => !_reservedPorts.Contains(port) && IsPortAvailable(port))
                    .ToList();

                if (availablePorts.Any())
                {
                    var selectedPort = availablePorts[random.Next(availablePorts.Count)];
                    _reservedPorts.Add(selectedPort);
                    return selectedPort;
                }

                Console.WriteLine("No available ports found for RCC allocation");
                return 0;
            }
        }

        public static int GenerateGameServerPort()
        {
            lock (_lock)
            {
                var random = new Random();
                var availablePorts = Enumerable.Range(GameServerMinPort, GameServerMaxPort - GameServerMinPort + 1)
                    .Where(port => !_reservedPorts.Contains(port) && IsPortAvailable(port))
                    .ToList();

                if (availablePorts.Any())
                {
                    var selectedPort = availablePorts[random.Next(availablePorts.Count)];
                    _reservedPorts.Add(selectedPort);
                    return selectedPort;
                }

                Console.WriteLine("No available ports found for game server allocation");
                return 0;
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
