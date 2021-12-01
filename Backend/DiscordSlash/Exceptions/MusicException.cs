using DexterSlash.Enums;

namespace DexterSlash.Exceptions
{
    public class MusicException : BaseAPIException
    {

        public readonly string Name, Description;

        public MusicException(string name, string message) : base($"{name}{message}", APIError.PlayerNotFound)
        {
            Name = name;
            Description = message;
        }
    }
}
