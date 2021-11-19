using DiscordSlash.Enums;
using DiscordSlash.Services;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace DiscordSlash.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class BaseCommand<T> : ApplicationCommandModule
    {

        protected ILogger<T> logger { get; set; }
        protected IdentityManager identityManager { get; set; }
        protected Identity currentIdentity { get; set; }
        protected IServiceProvider serviceProvider { get; set; }

        public BaseCommand(IServiceProvider serviceProvider)
        {
            logger = (ILogger<T>)serviceProvider.GetService(typeof(ILogger<T>));
            identityManager = (IdentityManager)serviceProvider.GetService(typeof(IdentityManager));

            this.serviceProvider = serviceProvider;
        }

        public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            if (ctx.Channel.Type == ChannelType.Text)
            {
                logger.LogInformation($"{ctx.User.Id} used {ctx.CommandName} in {ctx.Channel.Id} | {ctx.Guild.Id} {ctx.Guild.Name}");
            }
            else
            {
                logger.LogInformation($"{ctx.User.Id} used {ctx.CommandName} in DM");
            }

            currentIdentity = await identityManager.GetIdentity(ctx.User);

            if (currentIdentity == null)
            {
                logger.LogError($"Failed to register command identity for '{ctx.User.Id}'.");
                return false;
            }

            return await base.BeforeSlashExecutionAsync(ctx);
        }


        protected async Task Require(BaseContext ctx, params RequireCheckEnum[] checks)
        {
            foreach (RequireCheckEnum check in checks)
            {
                switch (check)
                {
                    case RequireCheckEnum.GuildModerator:
                        await RequireDiscordPermission(ctx, DiscordPermission.Moderator);
                        continue;
                }
            }
        }

        private async Task RequireDiscordPermission(BaseContext ctx, DiscordPermission permission)
        {
            await RequireRegisteredGuild(ctx);
            if (currentIdentity.IsSiteAdmin())
            {
                return;
            }
            switch (permission)
            {
                case (DiscordPermission.Member):
                    if (currentIdentity.IsOnGuild(ctx.Guild.Id)) return;
                    break;
                case (DiscordPermission.Moderator):
                    if (await currentIdentity.HasModRoleOrHigherOnGuild(ctx.Guild.Id)) return;
                    break;
                case (DiscordPermission.Admin):
                    if (await currentIdentity.HasAdminRoleOnGuild(ctx.Guild.Id)) return;
                    break;
            }
            throw new UnauthorizedException("You are not allowed to do that.");
        }

        private async Task RequireRegisteredGuild(BaseContext ctx)
        {
            try
            {
                await GuildConfigRepository.CreateDefault(serviceProvider).GetGuildConfig(ctx.Guild.Id);
            }
            catch (ResourceNotFoundException)
            {
                throw new UnregisteredGuildException(ctx.Guild.Id);
            }
            catch (NullReferenceException)
            {
                throw new BaseAPIException("Only usable in a guild.", APIError.OnlyUsableInAGuild);
            }
        }
    }
}
