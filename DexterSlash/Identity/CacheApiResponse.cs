namespace DexterSlash.Identity
{
    public class CacheApiResponse
    {
        private readonly object _content;
        private readonly DateTime _expiresAt;

        public CacheApiResponse(object content, int cacheMinutes = 30)
        {
            _content = content;
            _expiresAt = DateTime.Now.AddMinutes(cacheMinutes);
        }

        public T GetContent<T>()
        {
            return (T)_content;
        }

        public bool IsExpired()
        {
            return DateTime.Now > _expiresAt;
        }
    }
}
