using DiscordSlash.Models;
using DiscordSlash.Services;
using System.Diagnostics;

namespace DiscordSlash.Repositories
{
    public class StatusRepository : BaseRepository<StatusRepository>
    {

        private readonly DiscordBot _discordBot;

        public StatusRepository(IServiceProvider serviceProvider) : base(serviceProvider) {
            _discordBot = serviceProvider.GetRequiredService<DiscordBot>();
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
                if (!_discordBot.IsRunning())
                {
                    botStatus.Online = false;
                    botStatus.LastDisconnect = _discordBot.GetLastDisconnectTime();
                }
                botStatus.ResponseTime = _discordBot.GetPing();
            }
            catch (Exception)
            {
                botStatus.Online = false;
                botStatus.LastDisconnect = _discordBot.GetLastDisconnectTime();
            }
            return botStatus;
        }
    }
}
