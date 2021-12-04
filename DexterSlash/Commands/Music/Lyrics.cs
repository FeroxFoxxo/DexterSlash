using DexterSlash.Enums;
using Discord;
using Discord.Interactions;
using DexterSlash.Extensions;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("lyrics", "Replies with the lyrics to the current track that is playing, or one provided.")]

		public async Task Lyrics(string song = default)
		{
			if (song == default)
			{
				var player = AudioService.TryGetPlayer(Context, "find lyrics");

				if (player.State != PlayerState.Playing)
				{
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Unable to find song!")
						.WithDescription("Woaaah there, I'm not playing any tracks. " +
							"Please make sure I'm playing something before trying to find the lyrics for it!")
						.SendEmbed(Context.Interaction);

					return;
				}

				song = player.CurrentTrack.Title;
			}

			IEnumerable<LavalinkTrack> searchResult;

			try
			{
				searchResult = await AudioService.GetTracksAsync(song, SearchMode.YouTube);
			}
			catch (Exception)
			{
				Logger.LogError("Lavalink is not connected! Failing with embed error...");

				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Unable to find lyrics for {song}!")
					.WithDescription("Failure: lavalink dependency missing.\nPlease check the console logs for more details.")
					.SendEmbed(Context.Interaction);

				return;
			}

			foreach (var track in searchResult.Take(5))
			{
				if (track is null)
				{
					continue;
				}

				try
				{
					var lyrics = await LyricsService.GetLyricsAsync(track.Author, track.Title);

					if (!string.IsNullOrWhiteSpace(lyrics))
					{
						List<EmbedBuilder> embeds = new();

						var lyricsList = lyrics.Split('[');

						foreach (var lyrical in lyricsList)
							if (lyrical.Length > 0)
								embeds.Add(
									CreateEmbed(EmojiEnum.Unknown)
										.WithTitle($"🎶 {track.Title} - {track.Author} Lyrics")
										.WithDescription($"{(lyricsList.Length == 1 ? "" : "[")}" +
											$"{(lyrical.Length > 1700 ? lyrical[..1700] : lyrical)}")
									);

						await InteractiveService.CreateReactionMenu(embeds, Context);
						return;
					}

				}
				catch (Exception) { }
			}

			await CreateEmbed(EmojiEnum.Annoyed)
				.WithTitle("Unable to find song!")
				.WithDescription($"No lyrics found for:\n**{song}**.")
				.SendEmbed(Context.Interaction);
		}

	}
}
