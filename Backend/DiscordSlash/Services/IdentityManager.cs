using DiscordSlash.Identities;
using DSharpPlus.Entities;

namespace DiscordSlash.Services
{
    public class IdentityManager
    {
        private Dictionary<string, Identity> identities = new Dictionary<string, Identity>();

        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public async Task<Identity> GetIdentity(DiscordUser user)
        {
            if (user == null)
            {
                return null;
            }
            string key = $"/discord/cmd/{user.Id}";
            if (identities.ContainsKey(key))
            {
                Identity identity = identities[key];
                if (identity.ValidUntil >= DateTime.UtcNow)
                {
                    return identity;
                }
                else
                {
                    identities.Remove(key);
                }
            }

            return await RegisterNewIdentity(user);
        }

        private async Task<Identity> RegisterNewIdentity(DiscordUser user)
        {
            string key = $"/discord/cmd/{user.Id}";
            Identity identity = await DiscordCommandIdentity.Create(user, serviceProvider, serviceScopeFactory);
            identities[key] = identity;
            await _eventHandler.InvokeIdentityRegistered(new IdentityRegisteredEventArgs(identity));
            return identity;
        }

    }
}
