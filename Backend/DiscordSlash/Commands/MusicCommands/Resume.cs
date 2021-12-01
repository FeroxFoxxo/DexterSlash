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

		[SlashCommand("resume", "Resumes the track if the track is paused.")]
		[ComponentInteraction("resume-button")]

		public async Task Resume()
		{
			var player = AudioService.TryGetPlayer(Context, "resume player");

			var button = new ComponentBuilder().WithButton("Pause", "pause-button", ButtonStyle.Danger);

			switch (player.State)
			{
				case PlayerState.Paused:
					await player.ResumeAsync();

					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Resumed the player.")
						.WithDescription($"Successfully resumed {player.CurrentTrack.Title}.")
						.SendEmbed(Context.Interaction, component: button);
					break;
				case PlayerState.Playing:
					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Already playing.")
						.WithDescription($"The current track is already playing!")
						.SendEmbed(Context.Interaction, component: button);
					break;
				case PlayerState.NotPlaying:
					var track = player.Queue.FirstOrDefault();

					if (track is not null)
					{
						await player.PlayAsync(track);

						await player.SkipAsync();
					}

					if (player.CurrentTrack is not null && track is not null)
						await CreateEmbed(EmojiEnum.Love)
							.WithTitle("Resumed the player.")
							.WithDescription($"Successfully resumed {player.CurrentTrack.Title}")
							.SendEmbed(Context.Interaction, component: button);
					else
						await CreateEmbed(EmojiEnum.Love)
							.WithTitle("Could not resume the player.")
							.WithDescription($"No tracks currently in queue!")
							.SendEmbed(Context.Interaction);
					break;
				default:
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Unable to resume the player!")
						.WithDescription("The player must be either in a paused state to use this command.\n" +
							$"Current state is **{player.State.Humanize()}**.")
						.SendEmbed(Context.Interaction);
					break;
			}
		}
	}
}
