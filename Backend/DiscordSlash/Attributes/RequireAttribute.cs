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
        private readonly UserPermission _permissionLevel;

        public RequireAttribute(UserPermission permissionLevel)
        {
			_permissionLevel = permissionLevel;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Guild == null)
                return false;

            var guildConfig = await new GuildConfigRepository(ctx.Services)
				.GetGuildConfig(ctx.Guild.Id);

			var currentPerm = GetPermissionLevel(ctx.Member, guildConfig);

			return currentPerm == _permissionLevel;
		}

		public UserPermission GetPermissionLevel(DiscordMember user, GuildConfig config)
		{
			if (user == null)
				return UserPermission.Default;
			else if (user.Roles.Where(role => config.AdminRoles.Contains(role.Id)).Any())
				return UserPermission.Administrator;
			else if (user.Roles.Where(role => config.ModRoles.Contains(role.Id)).Any())
				return UserPermission.Moderator;
			else if (user.Roles.Where(role => config.WelcomerRoles.Contains(role.Id)).Any())
				return UserPermission.Welcomer;
			else if (user.Roles.Where(role => config.ElevatedRoles.Contains(role.Id)).Any())
				return UserPermission.Elevated;
			else if (user.Roles.Where(role => config.DJRoles.Contains(role.Id)).Any())
				return UserPermission.DJ;
			else
				return UserPermission.Default;
		}

	}
}
