using DiscordSlash.Enums;

namespace DiscordSlash.Exceptions
{
    public class BaseAPIException : Exception
    {
        public APIError Error { get; set; } = APIError.Unknown;

        public BaseAPIException(string message, APIError error) : base(message)
        {
            Error = error;
        }
    }
}
