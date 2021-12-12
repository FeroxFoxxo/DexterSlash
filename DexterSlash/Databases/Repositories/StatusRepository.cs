using DexterSlash.Databases.Models;
using Discord;
using Discord.WebSocket;
using System.Diagnostics;

namespace DexterSlash.Databases.Repositories
{
    public class StatusRepository : BaseRepository
    {

        private readonly DiscordShardedClient _client;

        public StatusRepository(IServiceProvider serviceProvider) : base(serviceProvider) {
            _client = serviceProvider.GetRequiredService<DiscordShardedClient>();
        }

        public async Task<StatusDetail> GetDbStatus()
        {
            StatusDetail dbStatus = new();
            try
            {
                Stopwatch timer = new();
                timer.Start();
                dbStatus.Online = await _context.CanConnectAsync();
                timer.Stop();
                dbStatus.ResponseTime = timer.Elapsed.TotalMilliseconds;
            }
            catch (Exception)
            {
                dbStatus.Online = false;
            }
            return dbStatus;
        }

        public StatusDetail GetBotStatus()
        {
            StatusDetail botStatus = new();
            try
            {
                if (_client.LoginState != LoginState.LoggedIn)
                    botStatus.Online = false;

                botStatus.ResponseTime = _client.Latency;
            }
            catch (Exception)
            {
                botStatus.Online = false;
            }
            return botStatus;
        }
    }
}
