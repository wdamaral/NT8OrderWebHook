using System;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Models
{
    public class ExecutionSnapshot
    {
        public DateTime Time { get; set; }
        public string InstrumentName { get; set; }
        public SignalType Signal { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
