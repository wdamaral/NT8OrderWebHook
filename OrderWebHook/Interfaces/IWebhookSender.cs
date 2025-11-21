using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces
{
    public interface IWebhookSender
    {
        Task<int> PostAsync(string url, string jsonPayload, string providerName);
    }
}
