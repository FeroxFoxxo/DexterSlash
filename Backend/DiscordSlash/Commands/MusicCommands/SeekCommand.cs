using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("seek", "Seeks the music player to the timespan given.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Seek(string seekPosition)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Could not seek current song.")
						.WithDescription(
							"I couldn't find the music player for this server.\n" +
							"Please ensure I am connected to a voice channel before using this command.")
						.SendEmbed(Context.Interaction);

				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Could not seek current song.")
						.WithDescription("I couldn't find a playing song to seek to~!")
						.SendEmbed(Context.Interaction);

				return;
			}

			TimeSpan? result = null;

			if (seekPosition.Contains(':'))
			{

				string[] times = Array.Empty<string>();
				int h = 0, m = 0, s;

				if (seekPosition.Contains(':'))
					times = seekPosition.Split(':');

				if (times.Length == 2)
				{
					m = int.Parse(times[0]);
					s = int.Parse(times[1]);
				}
				else if (times.Length == 3)
				{
					h = int.Parse(times[0]);
					m = int.Parse(times[1]);
					s = int.Parse(times[2]);
				}
				else
				{
					s = int.Parse(seekPosition);
				}

				if (s < 0 || m < 0 || h < 0)
				{
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Could not seek song!")
						.WithDescription("Please enter in positive value")
						.SendEmbed(Context.Interaction);

					return;
				}

				result = new(h, m, s);
			}

			if (!result.HasValue)
				if (TimeSpan.TryParse(seekPosition, out TimeSpan newTime))
					result = newTime;
				else
				{
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Could not seek song!")
						.WithDescription("The time you chose to seek could not be converted to a TimeSpan.")
						.SendEmbed(Context.Interaction);

					return;
				}


			if (player.Track.Duration < result)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Could not seek song!")
					.WithDescription("Value must not be greater than current track duration")
					.SendEmbed(Context.Interaction);

				return;
			}

			await player.SeekAsync(result.Value);

			await CreateEmbed(EmojiEnum.Love)
					.WithTitle($"Seeked current song to {result.Value.HumanizeTimeSpan()}.")
					.WithDescription($"Seeked applied {player.Track} from {player.Track.Position.HumanizeTimeSpan()} to {result.Value.HumanizeTimeSpan()}~!")
					.SendEmbed(Context.Interaction);
		}

	}
}
