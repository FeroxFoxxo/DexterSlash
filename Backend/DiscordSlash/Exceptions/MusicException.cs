using DexterSlash.Enums;

namespace DexterSlash.Exceptions
{
    public class MusicException : BaseAPIException
    {
        public MusicException(string message) : base(message, APIError.PlayerNotFound)
        {
        }
    }
}
