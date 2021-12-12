using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;
using Lavalink4NET;

namespace DexterSlash.Attributes
{
    public class DJMusicAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo cmdInfo, IServiceProvider services)
        {
            if (context.User is not IGuildUser guildUser)
                return PreconditionResult.FromError("Command must be used in a guild channel.");

            var musicConfig = await new ConfigRepository(services).GetGuildConfig(Modules.Music, guildUser.Id) as ConfigMusic;

            if (musicConfig.DJRoleID.HasValue)
                if (!guildUser.RoleIds.Contains(musicConfig.DJRoleID.Value))
                {
                    var player = services.GetService<IAudioService>().GetPlayer(context.Guild.Id);

                    if (player != null)
                    {
                        int uCount = 0;

                        var vc = await context.Guild.GetVoiceChannelAsync(player.VoiceChannelId.Value);

                        await foreach (var _ in vc.GetUsersAsync())
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
