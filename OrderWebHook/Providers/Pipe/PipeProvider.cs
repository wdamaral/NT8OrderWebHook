using Newtonsoft.Json;
using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using NinjaTrader.Custom.Indicators.OrderWebHook.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System;
using NinjaTrader.Cbi;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Pipe
{
    public class PipeProvider : IWebhookProvider
    {
        private readonly PipeConfig _config;
        private readonly Action<string, string, LogLevel> _logger;
        public string Name => "Pipe";
        public bool IsEnabled => _config.Enabled;

        public PipeProvider(PipeConfig config, Action<string, string, LogLevel> logger)
        {
            _config = config;
            _logger = logger;
        }

		public async Task ProcessAsync(ExecutionSnapshot snap)
		{
		    string pipeName = _config.PipeName;
		    if (string.IsNullOrEmpty(pipeName)) return;
		
		    try
		    {
                Stopwatch sw = Stopwatch.StartNew();
                string payload = PayloadFactory.GetPipePayload(snap, _config);
		        using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
		        {
		            await pipeClient.ConnectAsync(250).ConfigureAwait(false);
		
		            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(payload);
		            
		            await pipeClient.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
                    
					sw.Stop();

                    _logger(string.Format("Pipe sent - Payload {0} - Elapsed: {1}ms", payload, sw.ElapsedMilliseconds), string.Format("{0:HH.mm.ss} - PIPE sent - ({1}ms)", DateTime.Now, sw.ElapsedMilliseconds), LogLevel.Information);
		        }
		    }
		    catch (System.TimeoutException)
		    {
		        _logger("Pipe Timeout (Server Missing)", "Pipe: Warn", LogLevel.Warning);
		    }
		    catch (IOException ex)
		    {
		        // "Pipe is broken" or similar errors
		        _logger($"Pipe IO Error: {ex.Message}", "Pipe: Error", LogLevel.Error);
		    }
		    catch (Exception ex)
		    {
		        _logger($"Pipe Error: {ex.Message}", "Pipe: Error", LogLevel.Error);
		    }
		}
    }
}







