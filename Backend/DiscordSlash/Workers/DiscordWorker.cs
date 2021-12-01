using DexterSlash.Events;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace DexterSlash.Workers
{
    public class DiscordWorker : BackgroundService
    {

        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly DiscordShardedClient _client;
        private readonly ILogger<DiscordWorker> _logger;

        public DiscordWorker(InteractionService interactions, IServiceProvider services, DiscordShardedClient client, ILogger<DiscordWorker> logger)
        {
            _interactions = interactions;
            _services = services;
            _client = client;
            _logger = logger;

            Startup.GetEvents()
                .ForEach(type => (services.GetRequiredService(type) as Event).Initialize());
        }

        protected override async Task ExecuteAsync(CancellationToken _)
        {
            _logger.LogInformation("Starting DiscordWorker registered in Startup.");

            try
            {
                await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Modules could not initialize!");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
            await _client.StartAsync();
            _logger.LogInformation("Started running client!");
        }
    }
}
