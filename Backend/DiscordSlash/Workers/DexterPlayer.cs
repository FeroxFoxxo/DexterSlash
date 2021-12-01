using Discord;
using Lavalink4NET.Player;

namespace DexterSlash.Workers
{
    public class DexterPlayer : VoteLavalinkPlayer
    {
        public IInteractionContext Context { get; private set; }

        public void SetInteraction(IInteractionContext context) => Context = context;

    }
}
