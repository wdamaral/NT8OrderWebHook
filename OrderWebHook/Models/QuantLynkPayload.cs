using Newtonsoft.Json;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Models
{
    public class QuantLynkPayload
    {
        [JsonProperty("qv_user_id")]
        public string UserId { get; set; }

        [JsonProperty("alert_id")]
        public string AlertId { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }

        [JsonProperty("order_type", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderType { get; set; }

        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("flatten", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Flatten { get; set; }
    }
}
