using System;
using Microsoft.Extensions.Configuration;

namespace Economy
{
    public sealed class MarketplaceFeeService
    {
        private readonly int _feePercent;
        private const long PlatformAccountId = 1;

        public MarketplaceFeeService(IConfiguration configuration)
        {
            _feePercent = configuration.GetValue<int>("Marketplace:FeePercent", 30);
            if (_feePercent < 0) _feePercent = 0;
            if (_feePercent > 100) _feePercent = 100;
        }

        public MarketplaceFeeService(int feePercent)
        {
            _feePercent = feePercent < 0 ? 0 : (feePercent > 100 ? 100 : feePercent);
        }

        public int FeePercent => _feePercent;

        public long CalculateFee(long price)
        {
            if (price <= 0) return 0;
            var fee = (long)(price * _feePercent / 100.0);
            return fee > 0 ? fee : 1;
        }

        public long CalculateSellerProceeds(long price)
        {
            return price - CalculateFee(price);
        }

        public static long GetPlatformAccountId() => PlatformAccountId;
    }
}
