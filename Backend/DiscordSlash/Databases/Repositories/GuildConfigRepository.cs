using DexterSlash.Databases.Models;
using DexterSlash.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DexterSlash.Databases.Repositories
{
    public class GuildConfigRepository : BaseRepository<GuildConfigRepository>
    {
        public GuildConfigRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public async Task<GuildConfig> GetGuildConfig(ulong guildId)
        {
            GuildConfig guildConfig = await _context.GuildConfigs.AsQueryable().FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (guildConfig == null)
            {
                throw new ResourceNotFoundException($"GuildConfig with id {guildId} not found.");
            }

            return guildConfig;
        }

    }
}
