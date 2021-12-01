using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Humanizer;
using Lavalink4NET.Player;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("pause", "Pauses the current track.")]
		[ComponentInteraction("pause-button")]

		public async Task Pause()
		{
			var player = AudioService.TryGetPlayer(Context, "resume player");

			var button = new ComponentBuilder().WithButton("Resume", "resume-button");

			switch (player.State)
			{
				case PlayerState.Playing:
					await player.PauseAsync();

					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Paused the player.")
						.WithDescription($"Successfully paused {player.CurrentTrack.Title}.")
						.SendEmbed(Context.Interaction, component: button);
					break;
				case PlayerState.Paused:
				case PlayerState.NotPlaying:
					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Could not pause the player.")
						.WithDescription($"The track is already paused!")
						.SendEmbed(Context.Interaction);
					break;
				default:
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Unable to pause the player!")
						.WithDescription("The player must be either in a playing or paused state to use this command.\n" +
							$"Current state is **{player.State.Humanize()}**.")
						.SendEmbed(Context.Interaction);
					break;
			}
		}
	}
}
