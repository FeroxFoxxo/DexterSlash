namespace DexterSlash.Identity
{
    public class CacheKey
    {
        public readonly string Key;

        private CacheKey(string key)
        {
            Key = key;
        }

        public static CacheKey TokenUser(string token) => new ($"t:{token}");
    }
}
