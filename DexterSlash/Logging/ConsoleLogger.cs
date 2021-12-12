
namespace DexterSlash.Logging
{
    internal class ConsoleLogger : ILogger
    {

        private string _categoryName;
        private readonly LogLevel _logLevel = LogLevel.Information;

        public ConsoleLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        private bool IsBlocked(string message, LogLevel logLevel)
        {
            if (_categoryName == "DSharpPlus.BaseDiscordClient")
            {
                if (message.Contains("Pre-emptive ratelimit triggered - waiting until") && logLevel == LogLevel.Warning)
                {
                    return true;
                }
            }
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);

            if (IsBlocked(message, logLevel)) { return; }

            string shortLogLevel;

            switch (logLevel)
            {
                case LogLevel.Trace:
                    shortLogLevel = "T";
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Debug:
                    shortLogLevel = "D";
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Information:
                    shortLogLevel = "I";
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    shortLogLevel = "W";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    shortLogLevel = "E";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    shortLogLevel = "C";
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    shortLogLevel = "N";
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            if (_categoryName.StartsWith("DiscordSlash"))
            {
                _categoryName = _categoryName.Split('.').Last();
            }

            string currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string prefix = $"[{currentTime}] [{shortLogLevel}] {_categoryName}[{eventId.Id}]: ";

            Console.WriteLine($"{prefix}{message}");

            if (exception != null)
            {
                Console.WriteLine(exception.Message);
                if (exception.StackTrace != null)
                {
                    Console.WriteLine(exception.StackTrace);
                }
            }

            Console.ResetColor();
        }
    }
}