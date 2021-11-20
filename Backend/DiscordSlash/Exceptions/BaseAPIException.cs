using DiscordSlash.Enums;

namespace DiscordSlash.Exceptions
{
    public class BaseAPIException : Exception
    {
        public APIError Error { get; set; } = APIError.Unknown;

    }
}
