using Discord.Rest;

namespace DexterSlash.Identity
{
    public class DiscordOAuth
    {

        public readonly DateTime ValidUntil;

        public readonly DiscordRestClient RestClient;

        public DiscordOAuth(DiscordRestClient restClient)
        {
            ValidUntil = DateTime.UtcNow.AddMinutes(15);
            RestClient = restClient;
        }

    }
}