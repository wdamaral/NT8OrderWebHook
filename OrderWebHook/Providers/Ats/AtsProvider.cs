using Newtonsoft.Json;
using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Ats
{
    public class AtsProvider : IWebhookProvider
    {
        private readonly AtsConfig _config;
        private readonly IWebhookSender _sender;

        public string Name => "ATS";
        public bool IsEnabled => _config.Enabled;

        public AtsProvider(AtsConfig config, IWebhookSender sender)
        {
            _config = config;
            _sender = sender;
        }

        public async Task ProcessAsync(ExecutionSnapshot snap)
        {
            var payload = new AtsPayload
            {
                UserId = _config.UserId,
                SpamKey = _config.SpamKey,
                Contract = snap.InstrumentName,
                Quantity = snap.Quantity,
                Price = snap.Price,
                TradeType = GetTradeType(snap.Signal),
                Strategy = 0
            };

            string json = JsonConvert.SerializeObject(payload);
            await _sender.PostAsync(_config.Url, json, Name);
        }

        private string GetTradeType(SignalType signal)
        {
            if (signal == SignalType.Exit || signal == SignalType.Flatten) return "exit";
            return signal == SignalType.Buy ? "buy" : "sell";
        }
    }
}
