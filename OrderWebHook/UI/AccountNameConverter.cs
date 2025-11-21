using NinjaTrader.Cbi;
using System.ComponentModel;
using System.Linq;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.UI
{
    public class AccountNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) { return new StandardValuesCollection(Account.All.Select(a => a.Name).ToList()); }
    }
}
