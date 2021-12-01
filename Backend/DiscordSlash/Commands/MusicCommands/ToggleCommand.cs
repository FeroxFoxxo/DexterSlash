using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Humanizer;
using Victoria.Node;
using Victoria.Player;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in voice channels.")]
	public class ToggleCommand : BaseCommand<ToggleCommand>
	{

		public LavaNode LavaNode { get; set; }

		[SlashCommand("toggle", "Toggles whether this player is currently paused. Use while songs are playing to pause the player, use while a player is paused to resume it.")]

		public async Task PauseCommand()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to toggle player!")
					.WithDescription("I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (player.PlayerState == PlayerState.Paused)
			{
				await player.ResumeAsync();

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Resumed the player.")
					.WithDescription($"Successfully resumed {player.Track.Title}")
					.SendEmbed(Context.Interaction);
			}
			else if (player.PlayerState == PlayerState.Playing)
			{
				await player.PauseAsync();
				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Paused the player.")
					.WithDescription($"Successfully paused {player.Track.Title}")
					.SendEmbed(Context.Interaction);
			}
			else if (player.PlayerState == PlayerState.Stopped)
			{
				var track = player.Vueue.FirstOrDefault();

				if (track is not null)
				{
					await player.PlayAsync(track);

					await player.SkipAsync();
				}

				if (player.Track is not null && track is not null)
					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Resumed the player.")
						.WithDescription($"Successfully resumed {player.Track.Title}")
						.SendEmbed(Context.Interaction);
				else
					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Could not resume the player.")
						.WithDescription($"No tracks currently in queue!")
						.SendEmbed(Context.Interaction);
			}
			else
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to pause the player!")
					.WithDescription("The player must be either in a playing or paused state to use this command.\n" +
						$"Current state is **{player.PlayerState.Humanize()}**.")
					.SendEmbed(Context.Interaction);
			}
		}
	}
}
