using DexterSlash.Enums;
using DexterSlash.Attributes;
using Discord;
using Discord.Interactions;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using DexterSlash.Extensions;
using Fergun.Interactive;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in voice channels.")]
	public class LyricsCommand : BaseCommand<LyricsCommand>
	{

		public LavaNode LavaNode { get; set; }
		public InteractiveService InteractiveService { get; set; }

		[SlashCommand("lyrics", "Replies with the lyrics to the current track that is playing, or one provided.")]
		[EnabledBy(Modules.Music)]

		public async Task Lyrics(string song = default)
		{
			if (song == default)
			{
				if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
				{
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Unable to find lyrics!")
						.WithDescription("Failed to join voice channel.\nAre you in a voice channel?")
						.SendEmbed(Context.Interaction);

					return;
				}

				if (player.PlayerState != PlayerState.Playing)
				{
					await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Unable to find song!")
						.WithDescription("Woaaah there, I'm not playing any tracks. " +
							"Please make sure I'm playing something before trying to find the lyrics for it!")
						.SendEmbed(Context.Interaction);

					return;
				}

				song = player.Track.Title;
			}

			SearchResponse searchResult;

			try
			{
				searchResult = await LavaNode.SearchAsync(SearchType.YouTube, song);
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

			foreach (var track in searchResult.Tracks.Take(5))
			{
				if (track is null)
				{
					continue;
				}

				try
				{
					var lyrics = await track.FetchLyricsFromGeniusAsync();

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
