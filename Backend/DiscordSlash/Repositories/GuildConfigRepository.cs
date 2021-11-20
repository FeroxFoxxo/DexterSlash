﻿using DiscordSlash.Exceptions;
using DiscordSlash.Models;

namespace DiscordSlash.Repositories
{
    public class GuildConfigRepository : BaseRepository<GuildConfigRepository>
    {
        public GuildConfigRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public async Task<GuildConfig> GetGuildConfig(ulong guildId)
        {
            GuildConfig guildConfig = await Context.GuildConfigs.AsQueryable().FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (guildConfig == null)
            {
                throw new ResourceNotFoundException($"GuildConfig with id {guildId} not found.");
            }

            return guildConfig;
        }

    }
}
