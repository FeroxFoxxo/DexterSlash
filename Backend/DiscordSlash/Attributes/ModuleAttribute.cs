using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Attributes
{
    public class ModuleAttribute : PreconditionAttribute
    {

        public readonly Modules Module;

        public ModuleAttribute(Modules module)
        {
            Module = module;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (await new ConfigRepository(services).GetGuildConfig(Module, context.Guild.Id) == null)
                return PreconditionResult.FromError($"This command has not been enabled in this guild!");

            switch (Module)
            {
                case Modules.Music:
                    if (context.Channel.Id is default(ulong) || context.User.Id is default(ulong))
                        break;

                    var musicConfig = await new ConfigRepository(services).GetGuildConfig(Modules.Music, context.User.Id) as ConfigMusic;

                    if (context.Channel.Id != musicConfig.GuildId)
                        return PreconditionResult.FromError("To use this command, you must be in a music channel!");

                    break;
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
