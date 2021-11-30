using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Attributes
{
    public class DJMusicAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo cmdInfo, IServiceProvider services)
        {
            if (context.User is not IGuildUser guildUser)
                return PreconditionResult.FromError("Command must be used in a guild channel.");

            var musicConfig = await services.GetRequiredService<ConfigRepository>().GetGuildConfig<ConfigMusic>(Modules.Music, guildUser.Id);

            if (musicConfig.DJRoleID.HasValue)
                if (!guildUser.RoleIds.Contains(musicConfig.DJRoleID.Value))
                {
                    if (services.GetService<LavaNode>().TryGetPlayer(context.Guild, out var player))
                    {
                        int uCount = 0;

                        await foreach (var _ in player.VoiceChannel.GetUsersAsync())
                            uCount++;

                        if (uCount <= 2)
                            return PreconditionResult.FromSuccess();
                    }

                    return PreconditionResult.FromError(ErrorMessage ?? $"You require the DJ role to run this command.");
                }

            return PreconditionResult.FromSuccess();
        }
    }
}
