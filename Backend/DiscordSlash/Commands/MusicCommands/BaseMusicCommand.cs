using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Events;
using Discord.Interactions;
using Fergun.Interactive;
using Lavalink4NET;
using Lavalink4NET.Lyrics;
using SpotifyAPI.Web;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in vcs.")]
    [Module(Modules.Music)]

    public partial class BaseMusicCommand : BaseCommand<BaseMusicCommand>
    {
        public IAudioService AudioService { get; set; }

        public InteractiveService InteractiveService { get; set; }

        public ClientCredentialsRequest ClientCredentialsRequest { get; set; }

        public MusicEvent MusicEvent { get; set; }

        public LyricsService LyricsService { get; set; }

    }
}
