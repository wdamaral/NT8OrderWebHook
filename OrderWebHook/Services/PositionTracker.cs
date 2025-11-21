using NinjaTrader.Cbi;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using System;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Services
{
    public class PositionTracker
    {
        private readonly object _lock = new object();
        private double _currentQty = 0.0;
        private const double Epsilon = 0.00001;

        public void Initialize(Account account, Instrument instrument)
        {
            lock (_lock)
            {
                _currentQty = 0;
                if (account == null) return;
                foreach (Position p in account.Positions)
                {
                    if (p.Instrument.FullName == instrument.FullName)
                    {
                        _currentQty = p.MarketPosition == MarketPosition.Long ? p.Quantity : -p.Quantity;
                        break;
                    }
                }
            }
        }

        public SignalType ProcessExecution(Execution execution, out double newTotalQty)
        {
            lock (_lock)
            {
                double execQty = execution.Quantity;
                if (execution.MarketPosition == MarketPosition.Short) execQty = -execQty;
                _currentQty += execQty;
                newTotalQty = _currentQty;

                if (Math.Abs(_currentQty) < Epsilon) return SignalType.Exit;
                return execution.MarketPosition == MarketPosition.Long ? SignalType.Buy : SignalType.Sell;
            }
        }
    }
}
