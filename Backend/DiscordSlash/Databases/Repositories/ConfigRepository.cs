using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Enums;
using Microsoft.EntityFrameworkCore;

namespace DexterSlash.Databases.Repositories
{
    public class ConfigRepository : BaseRepository
    {
        public ConfigRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public async Task<ConfigBase> GetGuildConfig(Modules module, ulong guildId)
        {
            ConfigBase config = module switch
            {
                Modules.Modmail => _context.ConfigModMail.AsQueryable().FirstOrDefault(x => x.GuildId == guildId),
                Modules.Leveling => _context.ConfigLeveling.AsQueryable().FirstOrDefault(x => x.GuildId == guildId),
                Modules.Music => _context.ConfigMusic.AsQueryable().FirstOrDefault(x => x.GuildId == guildId),
                Modules.Utility => _context.ConfigUtility.AsQueryable().FirstOrDefault(x => x.GuildId == guildId),
                Modules.Moderator => _context.ConfigModerator.AsQueryable().FirstOrDefault(x => x.GuildId == guildId),
                _ => throw new NotImplementedException()
            };

            return config;
        }

    }
}
