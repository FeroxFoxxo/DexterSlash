﻿using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Attributes
{
    public class EnabledByAttribute : PreconditionAttribute
    {

        private readonly Modules _module;

        public EnabledByAttribute(Modules module)
        {
            _module = module;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.User is not IGuildUser guildUser)
                return PreconditionResult.FromError("Command must be used in a guild channel.");
            else if (await services.GetRequiredService<ConfigRepository>().GetGuildConfig<ConfigBase>(_module, guildUser.Guild.Id) == null)
                return PreconditionResult.FromError(ErrorMessage ?? $"This command has not been enabled in this guild!");
            else
            {
                switch (_module)
                {
                    case Modules.Music:
                        var musicConfig = await new ConfigRepository(services).GetGuildConfig<ConfigMusic>(Modules.Music, guildUser.Id);

                        if (context.Channel.Id != musicConfig.GuildId)
                            return PreconditionResult.FromError("To use this command, you must be in a music channel!");

                        break;
                }

                return PreconditionResult.FromSuccess();
            }
        }
    }
}
