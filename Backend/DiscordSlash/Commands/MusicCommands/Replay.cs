using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Lavalink4NET.Player;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("replay", "Replays the current track.")]
		[DJMusic]

		public async Task Replay()
		{
			var player = AudioService.TryGetPlayer(Context, "replay song");

			if (player.State == PlayerState.NotConnected)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Not playing any tracks.")
					.WithDescription($"I'm unable to replay a track when the bot isn't playing anything!")
					.SendEmbed(Context.Interaction);

				return;
			}

			await MusicEvent.CancelDisconnectAsync(player);
			await player.ReplayAsync();

			await CreateEmbed(EmojiEnum.Love)
				.WithTitle("Replaying song!")
				.WithDescription($"Replaying current track, {player.CurrentTrack.Title} by {player.CurrentTrack.Author}.")
				.SendEmbed(Context.Interaction);

		}
	}
}
