using DiscordSlash.Commands;
using DiscordSlash.Exceptions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Net.WebSockets;
using System.Text;

namespace DiscordSlash.Services
{
    public class DiscordBot : IHostedService
    {
        private readonly ILogger<DiscordBot> logger;
        private readonly DiscordClient client;
        private bool isRunning = false;
        private DateTime? lastDisconnect = null;

        public DiscordBot(ILogger<DiscordBot> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;

            DiscordConfiguration discordConfiguration = new ()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
                MessageCacheSize = 10240
            };

            client = new DiscordClient(discordConfiguration);

            client.SocketErrored += SocketErroredHandler;
            client.Resumed += ResumedHandler;
            client.Ready += ReadyHandler;

            var slash = client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = serviceProvider
            });

            slash.RegisterCommands<SayCommand>(613441321751019550);

            slash.SlashCommandErrored += CmdErroredHandler;
        }

        private Task ResumedHandler(DiscordClient sender, ReadyEventArgs e)
        {
            logger.LogWarning("Client reconnected.");
            isRunning = true;
            return Task.CompletedTask;
        }

        private Task SocketErroredHandler(DiscordClient sender, SocketErrorEventArgs e)
        {
            if (e.Exception is WebSocketException)
            {
                logger.LogCritical("Client disconnected.");
                isRunning = false;
                lastDisconnect = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        private Task ReadyHandler(DiscordClient sender, ReadyEventArgs e)
        {
            logger.LogInformation("Client connected.");
            isRunning = true;

            return Task.CompletedTask;
        }

        private async Task CmdErroredHandler(SlashCommandsExtension _, SlashCommandErrorEventArgs e)
        {
            if (e.Exception is BaseAPIException baseException)
            {
                logger.LogError($"Command '{e.Context.CommandName}' invoked by '{e.Context.User.Username}#{e.Context.User.Discriminator}' failed: {(e.Exception as BaseAPIException).Error}");

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
                logger.LogError($"Command '{e.Context.CommandName}' invoked by '{e.Context.User.Username}#{e.Context.User.Discriminator}' failed: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
            }
        }

        public DateTime? GetLastDisconnectTime()
        {
            return lastDisconnect;
        }

        public bool IsRunning()
        {
            return isRunning;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await client.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.DisconnectAsync();
        }
    }
}
