using Newtonsoft.Json;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Models
{
    public class PipePayload
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("spam-key")]
        public string SpamKey { get; set; }

        [JsonProperty("contract")]
        public string Contract { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("trade_type")]
        public string TradeType { get; set; }

        [JsonProperty("strategy")]
        public int Strategy { get; set; }
    }
}
