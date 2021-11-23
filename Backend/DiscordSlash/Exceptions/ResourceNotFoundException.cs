using DiscordSlash.Enums;

namespace DiscordSlash.Exceptions
{
    public class ResourceNotFoundException : BaseAPIException
    {
        public ResourceNotFoundException(string message) : base(message, APIError.ResourceNotFound)
        {
        }

        public ResourceNotFoundException() : base("Resource not found.", APIError.ResourceNotFound)
        {
        }
    }
}
