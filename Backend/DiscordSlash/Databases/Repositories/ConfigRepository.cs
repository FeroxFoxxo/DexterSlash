using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Enums;
using DexterSlash.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DexterSlash.Databases.Repositories
{
    public class ConfigRepository : BaseRepository
    {
        public ConfigRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public async Task<T> GetGuildConfig<T>(Modules module, ulong guildId) where T : ConfigBase
        {
            DbSet<T> guildDB = module switch
            {
                Modules.Modmail => _context.ConfigModMail as DbSet<T>,
                Modules.Leveling => _context.ConfigLeveling as DbSet<T>,
                Modules.Music => _context.ConfigMusic as DbSet<T>,
                Modules.Utility => _context.ConfigUtility as DbSet<T>,
                Modules.Moderator => _context.ConfigModerator as DbSet<T>,
                _ => throw new NotImplementedException()
            };

            var guildConfig = await guildDB.AsQueryable().FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (guildConfig == null)
            {
                throw new ResourceNotFoundException($"Guild configuration with id {guildId} was not found.");
            }

            return guildConfig;
        }

    }
}
