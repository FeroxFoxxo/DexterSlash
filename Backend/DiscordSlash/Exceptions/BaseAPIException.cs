using DiscordSlash.Enums;

namespace DiscordSlash.Exceptions
{
    public class BaseAPIException : Exception
    {
        public APIError Error { get; set; } = APIError.Unknown;

        public BaseAPIException(string? message) : base(message)
        {
        }

        public BaseAPIException(string? message, APIError error) : base(message)
        {
            Error = error;
        }

        public BaseAPIException(APIError error) : base(null)
        {
            Error = error;
        }
    }
}
