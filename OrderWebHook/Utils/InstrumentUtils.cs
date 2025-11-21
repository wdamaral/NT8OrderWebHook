using NinjaTrader.Cbi;
using System.Linq;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Utils
{
    public static class InstrumentUtils
    {
        public static string GetFuturesCode(Instrument inst)
        {
            if (inst == null || inst.MasterInstrument.InstrumentType != InstrumentType.Future)
                return inst != null ? inst.FullName : "";

            try
            {
                string root = inst.MasterInstrument.Name;
                int m = inst.Expiry.Month;
                string mCode = "Z";
                switch (m)
                {
                    case 1: mCode = "F"; break;
                    case 2: mCode = "G"; break;
                    case 3: mCode = "H"; break;
                    case 4: mCode = "J"; break;
                    case 5: mCode = "K"; break;
                    case 6: mCode = "M"; break;
                    case 7: mCode = "N"; break;
                    case 8: mCode = "Q"; break;
                    case 9: mCode = "U"; break;
                    case 10: mCode = "V"; break;
                    case 11: mCode = "X"; break;
                    case 12: mCode = "Z"; break;
                }
                string yStr = inst.Expiry.ToString("yy");
                return root + mCode + yStr.Last();
            }
            catch { return inst.FullName; }
        }
    }
}
