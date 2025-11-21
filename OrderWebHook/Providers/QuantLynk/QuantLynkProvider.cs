using Newtonsoft.Json;
using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.QuantLynk
{
    public class QuantLynkProvider : IWebhookProvider
    {
        private readonly QuantLynkConfig _config;
        private readonly IWebhookSender _sender;

        public string Name => "QuantLynk";
        public bool IsEnabled => _config.Enabled;

        public QuantLynkProvider(QuantLynkConfig config, IWebhookSender sender)
        {
            _config = config;
            _sender = sender;
        }

        public async Task ProcessAsync(ExecutionSnapshot snap)
        {
            var payload = new QuantLynkPayload
            {
                UserId = _config.UserId,
                AlertId = _config.AlertId
            };

            if (snap.Signal == SignalType.Exit || snap.Signal == SignalType.Flatten)
            {
                payload.Flatten = true;
            }
            else
            {
                payload.Quantity = snap.Quantity;
                payload.OrderType = "market";
                payload.Action = snap.Signal == SignalType.Buy ? "buy" : "sell";
            }

            string json = JsonConvert.SerializeObject(payload);
            await _sender.PostAsync(_config.Url, json, Name);
        }
    }
}
