using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Roblox.Thumbnails.Relay
{
    internal sealed class PerfStatsMonitor : IDisposable
    {
        internal void Start()
        {
            _TotalPhysicalMemory = _ComputerInfo.TotalPhysicalMemory / _BytesInGigabyte;
            _Adapters = (
                from n in new PerformanceCounterCategory("Network Interface").GetInstanceNames()
                where n != "MS TCP Loopback interface"
                select new NetworkAdapterMonitor(n)
            ).ToList();

            _ProcessorTimeTotalCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _Timer = new Timer(TimerTick, null, _TimerIntervalInMilliseconds, _TimerIntervalInMilliseconds);
            _PhysicalCoreCount =
                new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor")
                .Get()
                .Cast<ManagementBaseObject>()
                .Sum((ManagementBaseObject item) => int.Parse(item["NumberOfCores"].ToString()));
        }
        internal ServerStatistics GetServerStatistics()
        {
            return new ServerStatistics
            {
                TotalPhysicalMemoryGigabytes = _TotalPhysicalMemory,
                ProcessorCount = _PhysicalCoreCount,
                LogicalProcessorCount = _LogicalCoreCount,
                AvailablePhysicalMemoryGigabytes = _ComputerInfo.AvailablePhysicalMemory / _BytesInGigabyte,
                UploadSpeedKilobytesPerSecond = _Adapters.FirstOrDefault()?.UploadSpeedKilobytesPerSecond ?? 0f,
                DownloadSpeedKilobytesPerSecond = _Adapters.FirstOrDefault()?.DownloadSpeedKilobytesPerSecond ?? 0f,
                RccServiceProcesses = Process.GetProcessesByName("RCCService").Length,
                CpuUsage = _CpuUsage,
                ThumbnailsRelayVersion = _AssemblyVersion
            };
        }
        private void TimerTick(object state)
        {
            _Adapters.ForEach(a => a.Refresh());
            _CpuUsage = _ProcessorTimeTotalCounter.NextValue();
        }
        public void Dispose()
        {
            _Timer?.Dispose();
            _ProcessorTimeTotalCounter?.Dispose();
            _Adapters.ForEach(a => a.Dispose());
            _Adapters.Clear();
        }

        private static readonly string _AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private readonly ComputerInfo _ComputerInfo = new ComputerInfo();
        private readonly int _LogicalCoreCount = Environment.ProcessorCount;
        private List<NetworkAdapterMonitor> _Adapters = new List<NetworkAdapterMonitor>();
        private const int _TimerIntervalInMilliseconds = 1000;
        private const float _BytesInGigabyte = 1.073742E+09f;
        private PerformanceCounter _ProcessorTimeTotalCounter;
        private Timer _Timer;
        private float _CpuUsage;
        private int _PhysicalCoreCount;
        private float _TotalPhysicalMemory;
    }
}
