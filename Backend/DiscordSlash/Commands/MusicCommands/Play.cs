using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using SpotifyAPI.Web;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("play", "Searches for the desired song. Returns top 5 most popular results.")]

		public async Task Play(string search)
		{
			if (AudioService.GetPlayer(Context.Guild.Id) == null)
				await Join();

			if (AudioService.GetPlayer(Context.Guild.Id) == null)
				return;

			var player = AudioService.TryGetPlayer(Context, "play song");

			if (Uri.TryCreate(search, UriKind.Absolute, out Uri uriResult))
			{
				string baseUrl = uriResult.Host;
				string abUrl = uriResult.AbsoluteUri;
				/*
				if (baseUrl.Contains("youtube") || baseUrl.Contains("youtu.be"))
				{
					if (abUrl.Contains("list"))
					{
						var query = HttpUtility.ParseQueryString(uriResult.Query);

						var searchRequest = YouTubeService.PlaylistItems.List("snippet");

						searchRequest.PlaylistId = query["list"];
						searchRequest.MaxResults = 25;

						var searchResponse = await searchRequest.ExecuteAsync();

						List<string> songs = new();

						if (query["v"] is not null)
						{

							var searchRequestV = YouTubeService.Videos.List("snippet");
							searchRequestV.Id = query["v"];
							var searchResponseV = await searchRequestV.ExecuteAsync();

							var youTubeVideo = searchResponseV.Items.FirstOrDefault();

							songs.Add($"{youTubeVideo.Snippet.ChannelTitle} {youTubeVideo.Snippet.Title}");
						}

						foreach (var item in searchResponse.Items)
							songs.Add(item.Snippet.Title);

						await SearchPlaylist(songs.ToArray(), player);
					}
					else
					{
						var query = HttpUtility.ParseQueryString(uriResult.Query);

						var searchRequest = YouTubeService.Videos.List("snippet");
						searchRequest.Id = query["v"];
						var searchResponse = await searchRequest.ExecuteAsync();

						var youTubeVideo = searchResponse.Items.FirstOrDefault();

						await SearchSingleTrack($"{youTubeVideo.Snippet.ChannelTitle} {youTubeVideo.Snippet.Title}", player, false);
					}
				}
				else */
				if (baseUrl.Contains("soundcloud"))
				{
					await SearchSingleTrack($"{abUrl.Split('/').TakeLast(2).First()} {abUrl.Split('/').Last()}".Replace('-', ' '), player, false);
				}
				else if (baseUrl.Contains("spotify"))
				{
					var config = SpotifyClientConfig.CreateDefault();

					var response = await new OAuthClient(config).RequestToken(ClientCredentialsRequest);

					var spotifyAPI = new SpotifyClient(config.WithToken(response.AccessToken));

					string id = abUrl.Split('/').Last().Split('?').First();

					if (abUrl.Contains("playlist"))
					{
						var playlist = await spotifyAPI.Playlists.GetItems(id);

						List<string> songs = new();

						foreach (var item in playlist.Items)
						{
							if (item.Track is FullTrack track)
							{
								songs.Add($"{track.Artists.First().Name} {track.Name}");
							}
						}

						if (songs.Any())
						{
							await SearchPlaylist(songs.ToArray(), player);
						}
						else
							await CreateEmbed(EmojiEnum.Annoyed)
								.WithTitle($"Unable to search spotify!")
								.WithDescription("None of these tracks could be resolved. Please contact a developer!")
								.SendEmbed(Context.Interaction);
					}
					else if (abUrl.Contains("track"))
					{
						var track = await spotifyAPI.Tracks.Get(id);

						await SearchSingleTrack($"{track.Artists.First().Name} {track.Name}", player, false);
					}
					else
					{
						await CreateEmbed(EmojiEnum.Annoyed)
							.WithTitle($"Unable to search spotify!")
							.WithDescription("This music type is not implemented. Please contact a developer!")
							.SendEmbed(Context.Interaction);
					}
				}
			}
			else
			{
				await SearchSingleTrack(search, player, true);
			}
		}

		public async Task SearchPlaylist(string[] playlist, VoteLavalinkPlayer player)
		{
			bool wasEmpty = player.Queue.Count == 0 && player.State != PlayerState.Playing;

			List<LavalinkTrack> tracks = new ();

			foreach (string search in playlist)
			{
				var track = await AudioService.GetTrackAsync(search, SearchMode.YouTube);

				if (track is not null)
				{
					tracks.Add(track);
					await player.PlayAsync(track);
				}
			}

			List<EmbedBuilder> embeds;

			if (wasEmpty)
				embeds = player.GetQueue("🎶 Playlist Music Queue");
			else
				embeds = tracks.ToArray().GetQueueFromTrackArray("🎶 Playlist Music Queue");

			await InteractiveService.CreateReactionMenu(embeds, Context);
		}

		public async Task SearchSingleTrack(string search, VoteLavalinkPlayer player, bool searchList)
		{
			var tracks = await AudioService.GetTracksAsync(search, SearchMode.YouTube);

			var track = tracks.FirstOrDefault();

			if (track == null)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Unable to search!")
					.WithDescription($"The requested search: **{search}**, returned no results.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (searchList)
			{
				var topResults = tracks.Count() <= 5 ? tracks.ToList() : tracks.Take(5).ToList();

				string line1 = topResults.Count <= 5
					? $"I found {topResults.Count} tracks matching your search."
					: $"I found {tracks.Count():N0} tracks matching your search, here are the top 5.";

				var embedFields = new List<EmbedFieldBuilder>();

				var options = new List<string>();

				for (int i = 0; i < topResults.Count; i++)
				{
					if (options.Contains(topResults[i].Title))
						continue;

					options.Add(topResults[i].Title);

					embedFields.Add(new()
					{
						Name = $"#{i + 1}. {topResults[i].Title}",
						Value = $"Uploader: {topResults[i].Author}\n" + $"Duration: {topResults[i].Duration.HumanizeTimeSpan()}"
					});
				}

				var embed = CreateEmbed(EmojiEnum.Unknown)
					.WithTitle("Search Results:")
					.WithDescription($"{Context.User.Mention}, {line1}")
					.WithFields(embedFields)
					.Build();

				var result = await InteractiveService.SendSelectionAsync(
					new SelectionBuilder<string>()
						.WithSelectionPage(PageBuilder.FromEmbed(embed))
						.WithOptions(options)
						.WithInputType(InputType.SelectMenus)
						.WithDeletion(DeletionOptions.Invalid)
						.Build()
					, Context.Channel, TimeSpan.FromMinutes(2));

				await result.Message.DeleteAsync();

				if (!result.IsSuccess)
					return;

				track = tracks.Where(search => result.Value.EndsWith(search.Title)).FirstOrDefault();
			}

			if (player.Queue.Count == 0 && player.State != PlayerState.Playing)
			{
				await player.PlayAsync(track);

				await CreateEmbed(EmojiEnum.Unknown)
					.GetNowPlaying(track)
					.SendEmbed(Context.Interaction);
			}
			else
			{
				await player.PlayAsync(track);

				await CreateEmbed(EmojiEnum.Unknown)
					.GetQueuedTrack(track, player.Queue.Count)
					.SendEmbed(Context.Interaction);
			}
		}
	}
}
