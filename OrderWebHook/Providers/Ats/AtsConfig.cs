using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Ats
{
    public class AtsConfig : IProviderConfig
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public string SpamKey { get; set; }
    }
}
