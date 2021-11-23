using DiscordSlash.Exceptions;
using Microsoft.AspNetCore.Authentication;

namespace DiscordSlash.Identity
{
    public class OAuthManager
    {
        private readonly Dictionary<string, DiscordOAuth> Identities;

        private readonly ILogger<OAuthManager> Logger;

        private readonly IServiceProvider ServiceProvider;

        private readonly IServiceScopeFactory ServiceScopeFactory;

        public OAuthManager(ILogger<OAuthManager> logger, IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceScopeFactory;

            Identities = new();
        }

        public async Task<DiscordOAuth> GetIdentity(HttpContext httpContext)
        {
            string key = httpContext.Request.Cookies["dex_access_token"];

            if (string.IsNullOrEmpty(key))
            {
                throw new UnauthorizedException();
            }
            if (Identities.ContainsKey(key))
            {
                DiscordOAuth identity = Identities[key];
                if (identity.ValidUntil >= DateTime.UtcNow)
                {
                    return identity;
                }
                else
                {
                    Identities.Remove(key);
                }
            }

            return await RegisterNewIdentity(httpContext);
        }

        private async Task<DiscordOAuth> RegisterNewIdentity(HttpContext httpContext)
        {
            string key = string.Empty;

            key = httpContext.Request.Cookies["dex_access_token"];
            Logger.LogInformation("Registering new DiscordIdentity.");
            string token = await httpContext.GetTokenAsync("Cookies", "access_token");

            IDiscordAPIInterface api = serviceProvider.GetService(typeof(IDiscordAPIInterface)) as IDiscordAPIInterface;
            DiscordUser user = await api.FetchCurrentUserInfo(token, CacheBehavior.IgnoreButCacheOnError);
            List<DiscordGuild> guilds = await api.FetchGuildsOfCurrentUser(token, CacheBehavior.IgnoreButCacheOnError);

            DiscordOAuth identity = new DiscordOAuth(user);

            Identities[key] = identity;
            return identity;
        }

    }
}
