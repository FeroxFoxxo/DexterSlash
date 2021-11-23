using DexterSlash.Enums;
using DexterSlash.Exceptions;
using DexterSlash.Identity;
using Discord;
using Discord.Rest;

namespace DexterSlash.Services
{
    public class RestBot
    {
        private readonly ILogger<RestBot> _logger;
        private readonly Dictionary<string, CacheApiResponse> _cache;

        public RestBot(ILogger<RestBot> logger)
        {
            _logger = logger;
            _cache = new Dictionary<string, CacheApiResponse>();
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
