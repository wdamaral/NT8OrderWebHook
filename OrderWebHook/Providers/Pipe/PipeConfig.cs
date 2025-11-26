using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Pipe
{
    public class PipeConfig : IProviderConfig
    {
        public bool Enabled { get; set; }
        public string PipeName { get; set; }
        public string UserId { get; set; }
        public string SpamKey { get; set; }
    }
}
