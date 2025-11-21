using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces
{
    public interface IWebhookProvider
    {
        string Name { get; }
        bool IsEnabled { get; }
        Task ProcessAsync(Models.ExecutionSnapshot snapshot);
    }
}
