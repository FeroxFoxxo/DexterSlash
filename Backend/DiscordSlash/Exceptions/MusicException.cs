using DexterSlash.Enums;

namespace DexterSlash.Exceptions
{
    public class MusicException : BaseAPIException
    {
        public MusicException(string name, string message) : base($"{name}{message}", APIError.PlayerNotFound)
        {
        }
    }
}
