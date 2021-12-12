using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Lavalink4NET.Player;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("nowplaying", "Display the currently playing song.")]

		public async Task NowPlaying()
		{
			var player = AudioService.TryGetPlayer(Context, "find the current song");

			if (player.State != PlayerState.Playing)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to find current song!")
					.WithDescription("The player must be actively playing a track in order to see its information.")
					.SendEmbed(Context.Interaction);

				return;
			}

			await CreateEmbed(EmojiEnum.Unknown)
				.GetNowPlaying(player.CurrentTrack)
				.SendEmbed(Context.Interaction);
		}
	}
}
