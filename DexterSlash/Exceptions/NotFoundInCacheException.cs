using DexterSlash.Enums;

namespace DexterSlash.Exceptions
{
    public class NotFoundInCacheException : BaseAPIException
    {
        public string CacheKey { get; set; }

        public NotFoundInCacheException(string message, string cacheKey) : base(message, APIError.NotFoundInCache)
        {
            CacheKey = cacheKey;
        }

        public NotFoundInCacheException(string cacheKey) : base($"'{cacheKey}' is not cached.", APIError.NotFoundInCache)
        {
            CacheKey = cacheKey;
        }

        public NotFoundInCacheException() : base("Failed to find key in local cache.", APIError.NotFoundInCache)
        {
        }

    }
}
