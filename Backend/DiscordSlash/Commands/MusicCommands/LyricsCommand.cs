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

		[SlashCommand("lyrics", "Replies with the lyrics to the song provided.")]
		[EnabledBy(Modules.Music)]

		public async Task Lyrics(string song)
		{
			await SendLyricsFromTrack(song);
		}

		[SlashCommand("lyrics", "Replies with the lyrics to the current track that is playing.")]
		[EnabledBy(Modules.Music)]

		public async Task Lyrics()
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

			await SendLyricsFromTrack(player.Track.Title);
		}

		public async Task SendLyricsFromTrack(string song)
		{
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
						await SendLyricsEmbed(lyrics, "GENIUS", track.Title);
						return;
					}

				}
				catch (Exception) { }

				try
				{
					var lyrics = await track.FetchLyricsFromOvhAsync();

					if (!string.IsNullOrWhiteSpace(lyrics))
					{
						await SendLyricsEmbed(lyrics, "OHV", track.Title);
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

		private async Task SendLyricsEmbed (string fullLyrics, string name, string trackTitle)
		{
			List<EmbedBuilder> embeds = new();

			var lyricsList = fullLyrics.Split('[');

			foreach (var lyrics in lyricsList)
				if (lyrics.Length > 0)
					embeds.Add(
						CreateEmbed(EmojiEnum.Unknown)
							.WithTitle($"🎶 {trackTitle} - {name} Lyrics")
							.WithDescription($"{(lyricsList.Length == 1 ? "" : "[")}" +
								$"{(lyrics.Length > 1700 ? lyrics[..1700] : lyrics)}")
						);

			await InteractiveService.CreateReactionMenu(embeds, Context);
		}

	}
}
