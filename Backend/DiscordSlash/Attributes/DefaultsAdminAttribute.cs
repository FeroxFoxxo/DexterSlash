using Discord;
using Discord.Interactions;

namespace DexterSlash.Attributes
{
    public class DefaultsAdminAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo cmdInfo, IServiceProvider services)
        {
            if (context.User is not IGuildUser guildUser)
                return PreconditionResult.FromError("Command must be used in a guild channel.");
            else if (!guildUser.GuildPermissions.Has(GuildPermission.Administrator))
                return PreconditionResult.FromError(ErrorMessage ?? $"You require administrative privilages to run this command.");
            else
                return PreconditionResult.FromSuccess();
        }
    }
}
