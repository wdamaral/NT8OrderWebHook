using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Services
{
    /// <summary>
    /// Wraps HttpClient to allow for easier testing/mocking of the actual network layer.
    /// </summary>
    public class HttpWebhookSender : IWebhookSender
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public HttpWebhookSender(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<int> PostAsync(string url, string jsonPayload, string providerName)
        {
            try
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                Stopwatch sw = Stopwatch.StartNew();

                var response = await _client.PostAsync(url, content);

                sw.Stop();

                // Detailed Log: "ATS sent - Response 200 OK - Elapsed: 100ms"
                string detailedMsg = string.Format("{0} sent - Response {1} {2} - Payload {3} - Elapsed: {4}ms",
                    providerName,
                    (int)response.StatusCode,
                    response.StatusCode,
                    jsonPayload,
                    sw.ElapsedMilliseconds);

                // Short Log: "ATS: 200 (100ms)"
                string shortMsg = string.Format("{0:HH.mm.ss} - {1} sent: Response {2} {3} ({4}ms)",
                    DateTime.Now,
                    providerName,
                    (int)response.StatusCode,
                    response.StatusCode,
                    sw.ElapsedMilliseconds);

                _logger.Log(detailedMsg, shortMsg);

                return (int)response.StatusCode;
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0} Error: {1}", providerName, ex.Message),
                            string.Format("{0}: Failed", providerName));
                return -1;
            }
        }
    }
}
