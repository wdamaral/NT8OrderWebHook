using Newtonsoft.Json;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Ats;
using NinjaTrader.Custom.Indicators.OrderWebHook.Providers.QuantLynk;
using NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Pipe;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Services
{
    public static class PayloadFactory
    {
        public static string GetAtsPayload(ExecutionSnapshot snap, AtsConfig config)
        {
            var payload = new AtsPayload
            {
                UserId = config.UserId,
                SpamKey = config.SpamKey,
                Contract = snap.InstrumentName,
                Quantity = snap.Quantity,
                Price = snap.Price,
                Strategy = 0,
                TradeType = GetTradeType(snap.Signal)
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string GetPipePayload(ExecutionSnapshot snap, PipeConfig config)
        {
            var payload = new PipePayload
            {
                UserId = config.UserId,
                SpamKey = config.SpamKey,
                Contract = snap.InstrumentName,
                Quantity = snap.Quantity,
                Price = snap.Price,
                Strategy = 0,
                TradeType = GetTradeType(snap.Signal)
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string GetQlPayload(ExecutionSnapshot snap, QuantLynkPayload config)
        {
            var payload = new QuantLynkPayload
            {
                UserId = config.UserId,
                AlertId = config.AlertId
            };
            if (snap.Signal == SignalType.Exit || snap.Signal == SignalType.Flatten)
                payload.Flatten = true;
            else
            {
                payload.Quantity = snap.Quantity;
                payload.OrderType = "market";
                payload.Action = snap.Signal == SignalType.Buy ? "buy" : "sell";
            }
            return JsonConvert.SerializeObject(payload);
        }


        private static string GetTradeType(SignalType signal)
        {
            if (signal == SignalType.Exit || signal == SignalType.Flatten) return "exit";
            return signal == SignalType.Buy ? "buy" : "sell";
        }
    }
}






