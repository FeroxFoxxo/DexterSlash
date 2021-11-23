using DexterSlash.Enums;
using DexterSlash.Exceptions;
using DexterSlash.Services;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;

namespace DexterSlash.Identity
{
    public class OAuthManager
    {
        private readonly Dictionary<string, DiscordOAuth> _identities;

        private readonly ILogger<OAuthManager> _logger;

        private readonly IServiceProvider _serviceProvider;

        public OAuthManager(ILogger<OAuthManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _identities = new();
        }


        public async Task<DiscordOAuth> GetIdentity(HttpContext httpContext)
        {
            DiscordOAuth identity = await GetOrCreateIdentity(httpContext);
            if (identity == null)
            {
                throw new InvalidIdentityException();
            }
            return identity;
        }

        private async Task<DiscordOAuth> GetOrCreateIdentity(HttpContext httpContext)
        {
            string key = httpContext.Request.Cookies["dex_access_token"];

            if (string.IsNullOrEmpty(key))
            {
                throw new UnauthorizedException();
            }

            if (_identities.ContainsKey(key))
            {
                DiscordOAuth identity = _identities[key];
                if (identity.ValidUntil >= DateTime.UtcNow)
                {
                    return identity;
                }
                else
                {
                    _identities.Remove(key);
                }
            }

            return await RegisterNewIdentity(httpContext);
        }

        private async Task<DiscordOAuth> RegisterNewIdentity(HttpContext httpContext)
        {
            string key = httpContext.Request.Cookies["dex_access_token"];

            _logger.LogInformation("Registering new DiscordIdentity.");

            string token = await httpContext.GetTokenAsync("Cookies", "access_token");

            DiscordRestClient user = await _serviceProvider.GetService<RestBot>()
                .FetchCurrentUserInfo(token, CacheBehavior.IgnoreButCacheOnError);

            DiscordOAuth identity = new(user);
            _identities[key] = identity;
            return identity;
        }

    }
}
