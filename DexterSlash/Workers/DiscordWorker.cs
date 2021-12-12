using DexterSlash.Events;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Tracking;
using System.Reflection;

namespace DexterSlash.Workers
{
    public class DiscordWorker : BackgroundService
    {

        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly DiscordShardedClient _client;
        private readonly ILogger<DiscordWorker> _logger;
        private readonly IAudioService _audio;
        private readonly InactivityTrackingService _inactivity;

        public DiscordWorker(InteractionService interactions, IServiceProvider services, DiscordShardedClient client,
            ILogger<DiscordWorker> logger, IAudioService audio, InactivityTrackingService inactivity)
        {
            _interactions = interactions;
            _services = services;
            _client = client;
            _logger = logger;
            _audio = audio;
            _inactivity = inactivity;
        }

        protected override async Task ExecuteAsync(CancellationToken stop)
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

            Startup.GetEvents()
                .ForEach(type => (_services.GetRequiredService(type) as Event).Initialize());

            _inactivity.BeginTracking();

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));

            await _client.StartAsync();

            _logger.LogInformation("Started running client!");

            await _audio.InitializeAsync();

            _logger.LogInformation("Audio Service is ready!");
        }
    }
}
