using DiscordSlash.Enums;
using DiscordSlash.Models;
using DiscordSlash.Repositories;
using DSharpPlus.SlashCommands;

namespace DiscordSlash.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequireAttribute : SlashCheckBaseAttribute
    {
        private readonly PermissionLevel PermissionLevel;

        public RequireAttribute(PermissionLevel permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Guild == null)
                return false;

            var usr = ctx.Member;

            if (usr == null)
                return false;

            GuildConfig guildConfig = await new GuildConfigRepository(ctx.Services).GetGuildConfig(ctx.Guild.Id);


        }

    }
}
