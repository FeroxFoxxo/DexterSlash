
namespace DiscordSlash.Logging
{
    internal class ConsoleLogger : ILogger
    {

        private string CategoryName;
        private readonly LogLevel Level = LogLevel.Information;

        public ConsoleLogger(string categoryName)
        {
            CategoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= Level;
        }

        private bool IsBlocked(string message, LogLevel logLevel)
        {
            if (CategoryName == "DSharpPlus.BaseDiscordClient")
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

            if (CategoryName.StartsWith("DiscordSlash"))
            {
                CategoryName = CategoryName.Split('.').Last();
            }

            string currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string prefix = $"[{currentTime}] [{shortLogLevel}] {CategoryName}[{eventId.Id}]: ";

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