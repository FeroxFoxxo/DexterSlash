namespace DiscordSlash.Identities
{
    public abstract class Identity
    {
        public DateTime ValidUntil { get; set; }
        protected string Token;

        public Identity(string token)
        {
            Token = token;
            ValidUntil = DateTime.UtcNow.AddMinutes(15);
        }

        public abstract bool IsOnGuild(ulong guildId);
    }
}
