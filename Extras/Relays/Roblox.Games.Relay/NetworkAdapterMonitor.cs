using System;
using System.Diagnostics;

namespace Roblox.Games.Relay
{
    internal class NetworkAdapterMonitor : IDisposable
    {
        internal long DownloadSpeed { get; private set; }
        internal long UploadSpeed { get; private set; }
        internal float DownloadSpeedKilobytesPerSecond => DownloadSpeed / 1024f;
        internal float UploadSpeedKilobytesPerSecond => UploadSpeed / 1024f;

        public NetworkAdapterMonitor(string name)
        {
            _BytesReceivedPerformanceCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
            _BytesSentPerformanceCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);
            _DlValueOld = _BytesReceivedPerformanceCounter.NextSample().RawValue;
            _UlValueOld = _BytesSentPerformanceCounter.NextSample().RawValue;
        }

        internal void Refresh()
        {
            _DlValue = _BytesReceivedPerformanceCounter.NextSample().RawValue;
            _UlValue = _BytesSentPerformanceCounter.NextSample().RawValue;
            DownloadSpeed = _DlValue - _DlValueOld;
            UploadSpeed = _UlValue - _UlValueOld;
            _DlValueOld = _DlValue;
            _UlValueOld = _UlValue;
        }
        public void Dispose()
        {
            _BytesReceivedPerformanceCounter?.Dispose();
            if (_BytesSentPerformanceCounter == null)
                return;
            _BytesSentPerformanceCounter.Dispose();
        }

        private readonly PerformanceCounter _BytesReceivedPerformanceCounter;
        private readonly PerformanceCounter _BytesSentPerformanceCounter;
        private long _DlValue;
        private long _UlValue;
        private long _DlValueOld;
        private long _UlValueOld;
    }
}
