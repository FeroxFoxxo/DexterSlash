using DSharpPlus.Entities;

namespace DiscordSlash.Identity
{
    public class DiscordOAuth
    {

        public readonly DateTime ValidUntil;

        public readonly DiscordUser CurrentUser;

        public DiscordOAuth(DiscordUser currentUser)
        {
            ValidUntil = DateTime.UtcNow.AddMinutes(15);
            CurrentUser = currentUser;
        }

    }
}