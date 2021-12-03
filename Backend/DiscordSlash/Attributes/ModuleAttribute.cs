using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Attributes
{
    public class ModuleAttribute : Attribute
    {

        public readonly Modules Module;

        public ModuleAttribute(Modules module)
        {
            Module = module;
        }

        public async Task<PreconditionResult> CheckRequirementsAsync(ulong guildID, ulong channelID, ulong userID, IServiceProvider services)
        {
            if (await new ConfigRepository(services).GetGuildConfig<ConfigBase>(Module, guildID) == null)
                return PreconditionResult.FromError($"This command has not been enabled in this guild!");

            switch (Module)
            {
                case Modules.Music:
                    if (channelID is default(ulong) || userID is default(ulong))
                        break;

                    var musicConfig = await new ConfigRepository(services).GetGuildConfig<ConfigMusic>(Modules.Music, userID);

                    if (channelID != musicConfig.GuildId)
                        return PreconditionResult.FromError("To use this command, you must be in a music channel!");

                    break;
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
