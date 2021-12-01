using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("skip", "Skips the number of songs specified at once.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Skip(int skipCount = 1)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to skip song!")
					.WithDescription("I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			var curTrack = player.Track;
			bool emptyQueue = player.Vueue.Count == 0;

			if (curTrack == null)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to skip song!")
					.WithDescription("There isn't anything to skip.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (emptyQueue)
			{
				await player.StopAsync();
				await CreateEmbed(EmojiEnum.Love)
					.WithTitle($"Skipped {curTrack.Title}.")
					.WithDescription("No more tracks remaining.")
					.SendEmbed(Context.Interaction);
			}
			else if (skipCount == 1)
			{
				await player.SkipAsync();

				await CreateEmbed(EmojiEnum.Love)
					.GetNowPlaying(player.Track)
					.AddField("Skipped", curTrack.Title)
					.SendEmbed(Context.Interaction);
			}
			else
			{
				int actualSkipCount = 0;

				for (int i = 0; i < skipCount; i++)
				{
					try
					{
						await player.SkipAsync();
						actualSkipCount++;
					}
					catch (InvalidOperationException)
					{
						await player.StopAsync();
						break;
					}
				}

				string s = actualSkipCount == 1 ? "" : "s";

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Songs have been skipped!")
					.WithDescription($"Skipped {actualSkipCount:N0} track{s}.")
					.SendEmbed(Context.Interaction);
			}
		}

	}
}
