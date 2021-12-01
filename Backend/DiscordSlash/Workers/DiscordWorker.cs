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
        private readonly LavalinkWorker _lavalinkWorker;
        private readonly IAudioService _audio;
        private readonly InactivityTrackingService _inactivity;

        public DiscordWorker(InteractionService interactions, IServiceProvider services, DiscordShardedClient client,
            ILogger<DiscordWorker> logger, LavalinkWorker lavalinkWorker, IAudioService audio, InactivityTrackingService inactivity)
        {
            _interactions = interactions;
            _services = services;
            _client = client;
            _logger = logger;
            _lavalinkWorker = lavalinkWorker;
            _audio = audio;
            _inactivity = inactivity;
        }

        protected override async Task ExecuteAsync(CancellationToken stop)
        {
            _logger.LogInformation("Starting DiscordWorker registered in Startup.");

            _lavalinkWorker.Start();

            _lavalinkWorker.IsReady.WaitOne();

            Startup.GetEvents()
                .ForEach(type => (_services.GetRequiredService(type) as Event).Initialize());

            _inactivity.BeginTracking();

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

            await _audio.InitializeAsync();
            _logger.LogInformation("Audio Service is ready!");
        }
    }
}
