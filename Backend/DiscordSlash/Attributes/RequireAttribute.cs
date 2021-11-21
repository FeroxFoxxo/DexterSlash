using DiscordSlash.Enums;
using DiscordSlash.Models;
using DiscordSlash.Repositories;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordSlash.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequireAttribute : SlashCheckBaseAttribute
    {
        private readonly PermissionLevel UserPerm;

        public RequireAttribute(PermissionLevel permissionLevel)
        {
			UserPerm = permissionLevel;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Guild == null)
                return false;

            var guildConfig = await new GuildConfigRepository(ctx.Services).GetGuildConfig(ctx.Guild.Id);

			var currentPerm = GetPermissionLevel(ctx.Member, guildConfig);

			return currentPerm == UserPerm;
		}

		public PermissionLevel GetPermissionLevel(DiscordMember user, GuildConfig config)
		{
			if (user == null)
				return PermissionLevel.Default;
			else if (user.Roles.Where(role => config.AdminRoles.Contains(role.Id)).Any())
				return PermissionLevel.Administrator;
			else if (user.Roles.Where(role => config.ModRoles.Contains(role.Id)).Any())
				return PermissionLevel.Moderator;
			else if (user.Roles.Where(role => config.WelcomerRoles.Contains(role.Id)).Any())
				return PermissionLevel.Welcomer;
			else if (user.Roles.Where(role => config.ElevatedRoles.Contains(role.Id)).Any())
				return PermissionLevel.Elevated;
			else if (user.Roles.Where(role => config.DJRoles.Contains(role.Id)).Any())
				return PermissionLevel.DJ;
			else
				return PermissionLevel.Default;
		}

	}
}
