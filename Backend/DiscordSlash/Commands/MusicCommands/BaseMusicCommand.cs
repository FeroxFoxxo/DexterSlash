using DexterSlash.Events;
using Discord.Interactions;
using Fergun.Interactive;
using SpotifyAPI.Web;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in vcs.")]
    public partial class BaseMusicCommand : BaseCommand<BaseMusicCommand>
    {
        public LavaNode LavaNode { get; set; }

        public InteractiveService InteractiveService { get; set; }

        public MusicEvent MusicEvent { get; set; }

        public ClientCredentialsRequest ClientCredentialsRequest { get; set; }

    }
}
