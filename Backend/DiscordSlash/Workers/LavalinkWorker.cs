using System.Diagnostics;

namespace DexterSlash.Events
{
    public class LavalinkWorker
    {
        private readonly ILogger<LavalinkWorker> _logger;

        public EventWaitHandle IsReady { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        public LavalinkWorker(ILogger<LavalinkWorker> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            Process clientProcess = new();

            clientProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar {Directory.GetCurrentDirectory()}\\Lavalink\\Lavalink.jar",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            clientProcess.OutputDataReceived += OutputHandler;
            clientProcess.ErrorDataReceived += OutputHandler;

            clientProcess.Start();
            clientProcess.BeginOutputReadLine();
            clientProcess.BeginErrorReadLine();
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            var message = e.Data;
            if (!string.IsNullOrEmpty(message))
            {
                var lastIndexOf = message.LastIndexOf(']');
                var cleanMessage = message;

                if (lastIndexOf > 0)
                {
                    cleanMessage = message[(lastIndexOf + 1)..];
                }

                if (message.Contains("INFO"))
                {
                    _logger.LogInformation("Lavalink Jar: {Message}", cleanMessage);
                }
                else if (message.Contains("WARN"))
                {
                    _logger.LogWarning("Lavalink Jar: {Message}", cleanMessage);
                }
                else if (message.Contains("ERROR"))
                {
                    _logger.LogError("Lavalink Jar: {Message}", cleanMessage);
                }

                if (message.Contains("https://github.com/Frederikam/Lavalink/issues/295"))
                {
                    IsReady.Set();
                }
            }
        }
    }
}
