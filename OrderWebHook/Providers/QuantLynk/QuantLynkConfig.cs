using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.QuantLynk
{
    public class QuantLynkConfig : IProviderConfig
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public string AlertId { get; set; }
    }
}
