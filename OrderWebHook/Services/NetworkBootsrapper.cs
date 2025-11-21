using System.Net;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Services
{
    public static class NetworkBootstrapper
    {
        private static bool _init = false;
        public static void Optimize()
        {
            if (_init) return;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            _init = true;
        }
    }
}
