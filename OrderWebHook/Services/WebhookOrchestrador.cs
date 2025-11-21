using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.Services
{
    /// <summary>
    /// Manages the background consumer loop and distributes messages to providers.
    /// </summary>
    public class WebhookOrchestrator : IDisposable
    {
        private readonly BlockingCollection<ExecutionSnapshot> _queue;
        private readonly CancellationTokenSource _cts;
        private readonly IEnumerable<IWebhookProvider> _providers;
        private readonly ILogger _logger;
        private Task _consumerTask;

        public WebhookOrchestrator(IEnumerable<IWebhookProvider> providers, ILogger logger)
        {
            _providers = providers;
            _logger = logger;
            _queue = new BlockingCollection<ExecutionSnapshot>();
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            if (_consumerTask == null)
            {
                _consumerTask = Task.Run(ConsumerLoop);
            }
        }

        public void Enqueue(ExecutionSnapshot snap)
        {
            if (!_queue.IsAddingCompleted) _queue.Add(snap);
        }

        private async Task ConsumerLoop()
        {
            try
            {
                foreach (var snap in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    var activeProviders = _providers.Where(p => p.IsEnabled).ToList();
                    if (!activeProviders.Any()) continue;

                    var tasks = activeProviders.Select(p => p.ProcessAsync(snap));
                    await Task.WhenAll(tasks);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Log("Orchestrator Error: " + ex.Message, "Orchestrator Error");
            }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _cts.Cancel();
            if (_consumerTask != null) _consumerTask.Wait(1000);
            _cts.Dispose();
            _queue.Dispose();
        }
    }
}
