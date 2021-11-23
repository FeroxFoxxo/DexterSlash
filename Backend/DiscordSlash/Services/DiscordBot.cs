using DiscordSlash.Commands;
using DiscordSlash.Exceptions;
using DiscordSlash.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Net.WebSockets;
using System.Text;

namespace DiscordSlash.Services
{
    public class DiscordBot
    {
        private readonly ILogger<DiscordBot> _logger;
        private readonly DiscordClient _client;
        private bool _isRunning = false;
        private DateTime? _lastDisconnect = null;

        public DiscordBot(ILogger<DiscordBot> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new LoggerProvider());

            DiscordConfiguration discordConfiguration = new ()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
                LoggerFactory = loggerFactory,
                MessageCacheSize = 1024
            };

            _client = new DiscordClient(discordConfiguration);

            _client.SocketErrored += SocketErroredHandler;
            _client.Resumed += ResumedHandler;
            _client.Ready += ReadyHandler;

            var slash = _client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = serviceProvider
            });

            slash.RegisterCommands<SayCommand>(613441321751019550);

            slash.SlashCommandErrored += CmdErroredHandler;
        }

        private Task ResumedHandler(DiscordClient sender, ReadyEventArgs e)
        {
            _logger.LogWarning("Client reconnected.");
            _isRunning = true;
            return Task.CompletedTask;
        }

        private Task SocketErroredHandler(DiscordClient sender, SocketErrorEventArgs e)
        {
            if (e.Exception is WebSocketException)
            {
                _logger.LogCritical("Client disconnected.");
                _isRunning = false;
                _lastDisconnect = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        private Task ReadyHandler(DiscordClient sender, ReadyEventArgs e)
        {
            _logger.LogInformation("Client connected.");
            _isRunning = true;

            return Task.CompletedTask;
        }

        private async Task CmdErroredHandler(SlashCommandsExtension _, SlashCommandErrorEventArgs e)
        {
            if (e.Exception is BaseAPIException baseException)
            {
                _logger.LogError($"Command '{e.Context.CommandName}' invoked by '{e.Context.User.Username}#{e.Context.User.Discriminator}' failed: {(e.Exception as BaseAPIException).Error}");

                string errorCode = "0#" + ((int)baseException.Error).ToString("D7");

                StringBuilder sb = new ();
                sb.AppendLine("Something went wrong.");
                sb.AppendLine($"`{baseException.Error}`");
                sb.Append($"**Code** ");
                sb.Append($"`{errorCode}`");

                try
                {
                    await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(sb.ToString()));
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent(sb.ToString()));
                }
            }
            else
            {
                _logger.LogError($"Command '{e.Context.CommandName}' invoked by '{e.Context.User.Username}#{e.Context.User.Discriminator}' failed: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
            }
        }

        public DateTime? GetLastDisconnectTime()
        {
            return _lastDisconnect;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public int GetPing()
        {
            return _client.Ping;
        }

        public async Task StartAsync()
        {
            await _client.ConnectAsync();
        }
    }
}
