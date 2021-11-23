using DexterSlash.Enums;
using DexterSlash.Exceptions;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;

namespace DexterSlash.Identity
{
    public class OAuthManager
    {
        private readonly Dictionary<string, DiscordOAuth> _identities;

        private readonly ILogger<OAuthManager> _logger;

        private readonly Dictionary<string, CacheApiResponse> _cache;

        public OAuthManager(ILogger<OAuthManager> logger)
        {
            _logger = logger;

            _identities = new();
            _cache = new ();
        }

        public async Task<DiscordRestClient> FetchCurrentUserInfo(string token, CacheBehavior cacheBehavior)
        {
            CacheKey cacheKey = CacheKey.TokenUser(token);
            DiscordRestClient user = null;

            try
            {
                user = TryGetFromCache<DiscordRestClient>(cacheKey, cacheBehavior);
                if (user != null) return user;
            }
            catch (NotFoundInCacheException)
            {
                return user;
            }

            try
            {
                _logger.LogError(token);

                user = new DiscordRestClient();

                await user.LoginAsync(TokenType.Bearer, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to fetch current user for token '{token}' from API.");
                return FallBackToCache<DiscordRestClient>(cacheKey, cacheBehavior);
            }
            _cache[cacheKey.Key] = new CacheApiResponse(user);

            return user;
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

            DiscordRestClient user = await FetchCurrentUserInfo(token, CacheBehavior.IgnoreButCacheOnError);

            DiscordOAuth identity = new(user);
            _identities[key] = identity;
            return identity;
        }

        private T TryGetFromCache<T>(CacheKey cacheKey, CacheBehavior cacheBehavior)
        {
            if (cacheBehavior == CacheBehavior.OnlyCache)
            {
                if (_cache.ContainsKey(cacheKey.Key))
                {
                    return _cache[cacheKey.Key].GetContent<T>();
                }
                else
                {
                    throw new NotFoundInCacheException(cacheKey.Key);
                }
            }
            if (_cache.ContainsKey(cacheKey.Key) && cacheBehavior == CacheBehavior.Default)
            {
                if (!_cache[cacheKey.Key].IsExpired())
                {
                    return _cache[cacheKey.Key].GetContent<T>();
                }
                _cache.Remove(cacheKey.Key);
            }
            return default;
        }

        private T FallBackToCache<T>(CacheKey cacheKey, CacheBehavior cacheBehavior)
        {
            if (cacheBehavior != CacheBehavior.IgnoreCache)
            {
                if (_cache.ContainsKey(cacheKey.Key))
                {
                    if (!_cache[cacheKey.Key].IsExpired())
                    {
                        return _cache[cacheKey.Key].GetContent<T>();
                    }
                    _cache.Remove(cacheKey.Key);
                }
            }
            return default;
        }
    }
}
