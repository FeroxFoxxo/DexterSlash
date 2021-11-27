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
            else if (!(await services.GetRequiredService<GuildConfigRepository>().GetGuildConfig(guildUser.Guild.Id)).EnabledModules.HasFlag(_module))
                return PreconditionResult.FromError(ErrorMessage ?? $"You require administrative privilages to run this command.");
            else
                return PreconditionResult.FromSuccess();
        }
    }
}
