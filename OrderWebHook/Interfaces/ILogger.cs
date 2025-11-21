
using NinjaTrader.Cbi;
namespace NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces
{
    public interface ILogger
    {
        /// <summary>
        /// Logs a message. 
        /// </summary>
        /// <param name="outputMessage">Detailed message for the Output Window</param>
        /// <param name="gridMessage">Short message for the UI Grid</param>
        void Log(string outputMessage, string gridMessage, LogLevel logLevel = LogLevel.Information);
    }
}

