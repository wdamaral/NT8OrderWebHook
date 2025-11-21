using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using NinjaTrader.NinjaScript;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using NinjaTrader.Custom.Indicators.OrderWebHook.Services;
using NinjaTrader.Custom.Indicators.OrderWebHook.UI;
using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Custom.Indicators.OrderWebHook.Providers.Ats;
using NinjaTrader.Custom.Indicators.OrderWebHook.Providers.QuantLynk;
using NinjaTrader.Custom.Indicators.OrderWebHook.Utils;
using NinjaTrader.Custom.Indicators.OrderWebHook.Models;
using AccountNameConverter = NinjaTrader.Custom.Indicators.OrderWebHook.UI.AccountNameConverter;
using NinjaTrader.Custom.Indicators;

namespace NinjaTrader.NinjaScript.Indicators
{

    /// <summary>
    /// Main Indicator Entry Point.
    /// Acts as the Composition Root and Configuration holder.
    /// </summary>
    public class OrderWebHook : Indicator, ILogger
    {
        private WebhookOrchestrator _orchestrator;
        private PositionTracker _tracker;
        private ChartUiManager _uiManager;
        private Account _activeAccount;
        private string _instrumentNameCode;

        // Configuration Objects (Bound to UI)
        private readonly AtsConfig _atsConfig = new AtsConfig();
        private readonly QuantLynkConfig _qlConfig = new QuantLynkConfig();

        #region 1. ATS Settings
        [NinjaScriptProperty]
        [Display(Name = "Enable ATS", Order = 1, GroupName = "1. ATS Settings")]
        public bool AtsEnabled { get { return _atsConfig.Enabled; } set { _atsConfig.Enabled = value; } }

        [NinjaScriptProperty]
        [Display(Name = "Webhook URL", Order = 2, GroupName = "1. ATS Settings")]
        public string AtsUrl { get { return _atsConfig.Url; } set { _atsConfig.Url = value; } }

        [NinjaScriptProperty]
        [Display(Name = "User ID", Order = 3, GroupName = "1. ATS Settings")]
        public string AtsUserId { get { return _atsConfig.UserId; } set { _atsConfig.UserId = value; } }

        [NinjaScriptProperty]
        [Display(Name = "Spam Key", Order = 4, GroupName = "1. ATS Settings")]
        public string AtsSpamKey { get { return _atsConfig.SpamKey; } set { _atsConfig.SpamKey = value; } }
        #endregion

        #region 2. QuantLynk Settings
        [NinjaScriptProperty]
        [Display(Name = "Enable QuantLynk", Order = 1, GroupName = "2. QuantLynk Settings")]
        public bool QlEnabled { get { return _qlConfig.Enabled; } set { _qlConfig.Enabled = value; } }

        [NinjaScriptProperty]
        [Display(Name = "Webhook URL", Order = 2, GroupName = "2. QuantLynk Settings")]
        public string QlUrl { get { return _qlConfig.Url; } set { _qlConfig.Url = value; } }

        [NinjaScriptProperty]
        [Display(Name = "QV User ID", Order = 3, GroupName = "2. QuantLynk Settings")]
        public string QlUserId { get { return _qlConfig.UserId; } set { _qlConfig.UserId = value; } }

        [NinjaScriptProperty]
        [Display(Name = "Alert ID", Order = 4, GroupName = "2. QuantLynk Settings")]
        public string QlAlertId { get { return _qlConfig.AlertId; } set { _qlConfig.AlertId = value; } }
        #endregion

        #region 3. General Settings
        [NinjaScriptProperty]
        [Display(Name = "Account Name", Description = "The account to monitor orders for", Order = 0, GroupName = "3. General Settings")]
        [TypeConverter(typeof(AccountNameConverter))]
        public string AccountName { get; set; }
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Sends multi-service webhooks on execution.";
                Name = "OrderWebHook";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                PaintPriceMarkers = false;
                ScaleJustification = ScaleJustification.Right;

                AccountName = "Sim101";

                // Defaults
                _atsConfig.Url = "https://api.ats.com/hook";
                _atsConfig.UserId = "USER_123";
                _qlConfig.Url = "https://hooks.quantlynk.com";
            }
            else if (State == State.Configure)
            {
                NetworkBootstrapper.Optimize();

                // 1. Create Infrastructure
                var httpSender = new HttpWebhookSender(new HttpClient(), this);

                // 2. Create Providers
                var providers = new List<IWebhookProvider>
                {
                    new AtsProvider(_atsConfig, httpSender),
                    new QuantLynkProvider(_qlConfig, httpSender)
                };

                // 3. Orchestrate
                _orchestrator = new WebhookOrchestrator(providers, this);
                _orchestrator.Start();
            }
            else if (State == State.DataLoaded)
            {
                if (Instrument != null)
                    _instrumentNameCode = InstrumentUtils.GetFuturesCode(Instrument);

                lock (Account.All)
                {
                    _activeAccount = Account.All.FirstOrDefault(a => a.Name == AccountName);
                }

                if (_activeAccount != null)
                {
                    _tracker = new PositionTracker();
                    _tracker.Initialize(_activeAccount, Instrument);
                    _activeAccount.ExecutionUpdate += OnExecutionUpdate;
                }
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        _uiManager = new ChartUiManager(this, ChartControl, _atsConfig, _qlConfig);
                        _uiManager.LoadControlPanel();
                    });
                }
            }
            else if (State == State.Terminated)
            {
                if (_activeAccount != null)
                    _activeAccount.ExecutionUpdate -= OnExecutionUpdate;

                if (_orchestrator != null)
                    _orchestrator.Dispose();

                if (_uiManager != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        _uiManager.UnloadControlPanel();
                    });
                }
            }
        }

        protected override void OnBarUpdate() { }

        private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
        {
            try
            {
                if (State == State.Terminated || e.Execution == null) return;

                // Filter Logic
                if (Instrument != null && e.Execution.Instrument.FullName != Instrument.FullName) return;
                if (_activeAccount != null && e.Execution.Account.Name != _activeAccount.Name) return;
                if (e.Execution.Order == null || e.Execution.Order.OrderState != OrderState.Filled) return;

                // Core Logic
                double newTotalQty;
                SignalType signal = _tracker.ProcessExecution(e.Execution, out newTotalQty);

                var snap = new ExecutionSnapshot
                {
                    Time = DateTime.Now,
                    InstrumentName = _instrumentNameCode ?? e.Execution.Instrument.FullName,
                    Signal = signal,
                    Quantity = e.Quantity,
                    Price = e.Price
                };

                _orchestrator.Enqueue(snap);
            }
            catch (Exception ex)
            {
                Log("Error in ExecutionUpdate: " + ex.Message, "Exec Error", LogLevel.Error);
            }
        }

        // -- ILogger Implementation --
        public void Log(string outputMessage, string gridMessage, LogLevel logLevel = LogLevel.Information)
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    string formattedMsg = string.Format("{0} | {1}", DateTime.Now.ToShortTimeString(), outputMessage);
                    NinjaTrader.Cbi.Log.Process(typeof(Resource), "CbiLogGeneric", new object[] { formattedMsg }, logLevel, LogCategories.Default);

                    if (_uiManager != null && !string.IsNullOrEmpty(gridMessage))
                    {
                        _uiManager.LogMessage(gridMessage);
                    }
                });
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OrderWebHook[] cacheOrderWebHook;
		public OrderWebHook OrderWebHook(bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			return OrderWebHook(Input, atsEnabled, atsUrl, atsUserId, atsSpamKey, qlEnabled, qlUrl, qlUserId, qlAlertId, accountName);
		}

		public OrderWebHook OrderWebHook(ISeries<double> input, bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			if (cacheOrderWebHook != null)
				for (int idx = 0; idx < cacheOrderWebHook.Length; idx++)
					if (cacheOrderWebHook[idx] != null && cacheOrderWebHook[idx].AtsEnabled == atsEnabled && cacheOrderWebHook[idx].AtsUrl == atsUrl && cacheOrderWebHook[idx].AtsUserId == atsUserId && cacheOrderWebHook[idx].AtsSpamKey == atsSpamKey && cacheOrderWebHook[idx].QlEnabled == qlEnabled && cacheOrderWebHook[idx].QlUrl == qlUrl && cacheOrderWebHook[idx].QlUserId == qlUserId && cacheOrderWebHook[idx].QlAlertId == qlAlertId && cacheOrderWebHook[idx].AccountName == accountName && cacheOrderWebHook[idx].EqualsInput(input))
						return cacheOrderWebHook[idx];
			return CacheIndicator<OrderWebHook>(new OrderWebHook(){ AtsEnabled = atsEnabled, AtsUrl = atsUrl, AtsUserId = atsUserId, AtsSpamKey = atsSpamKey, QlEnabled = qlEnabled, QlUrl = qlUrl, QlUserId = qlUserId, QlAlertId = qlAlertId, AccountName = accountName }, input, ref cacheOrderWebHook);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OrderWebHook OrderWebHook(bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			return indicator.OrderWebHook(Input, atsEnabled, atsUrl, atsUserId, atsSpamKey, qlEnabled, qlUrl, qlUserId, qlAlertId, accountName);
		}

		public Indicators.OrderWebHook OrderWebHook(ISeries<double> input , bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			return indicator.OrderWebHook(input, atsEnabled, atsUrl, atsUserId, atsSpamKey, qlEnabled, qlUrl, qlUserId, qlAlertId, accountName);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OrderWebHook OrderWebHook(bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			return indicator.OrderWebHook(Input, atsEnabled, atsUrl, atsUserId, atsSpamKey, qlEnabled, qlUrl, qlUserId, qlAlertId, accountName);
		}

		public Indicators.OrderWebHook OrderWebHook(ISeries<double> input , bool atsEnabled, string atsUrl, string atsUserId, string atsSpamKey, bool qlEnabled, string qlUrl, string qlUserId, string qlAlertId, string accountName)
		{
			return indicator.OrderWebHook(input, atsEnabled, atsUrl, atsUserId, atsSpamKey, qlEnabled, qlUrl, qlUserId, qlAlertId, accountName);
		}
	}
}

#endregion
