using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Events;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;
using Humanizer;
using SpotifyAPI.Web;
using System.Web;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using SearchResponse = Victoria.Responses.Search.SearchResponse;

namespace DexterSlash.Commands.MusicCommands
{
	[Group("music", "A list of commands that play music in voice channels.")]
	public class PlayCommand : BaseCommand<PlayCommand>
	{

		public LavaNode LavaNode { get; set; }

		public InteractiveService InteractiveService { get; set; }

		public ClientCredentialsRequest ClientCredentialsRequest { get; set; }

		public MusicEvent MusicEvent { get; set; }

		[SlashCommand("play", "Searches for the desired song. Returns top 5 most popular results. Click on one of the reaction icons to play the appropriate track.")]
		[EnabledBy(Modules.Music)]

		public async Task Play(string search)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to play song!")
					.WithDescription("I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			try
			{
				await LavaNode.SearchAsync(SearchType.YouTube, search);
			}
			catch (Exception)
			{
				Logger.LogError("Lavalink is not connected!\nFailing with embed error...");

				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Unable to search!")
					.WithDescription("Failure: lavalink dependency missing.\nPlease check the console logs for more details.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (Uri.TryCreate(search, UriKind.Absolute, out Uri uriResult))
			{
				string baseUrl = uriResult.Host;
				string abUrl = uriResult.AbsoluteUri;

				if (baseUrl.Contains("youtube") || baseUrl.Contains("youtu.be"))
				{
					/*
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
					*/
				}
				else if (baseUrl.Contains("soundcloud"))
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

		public async Task SearchPlaylist(string[] playlist, LavaPlayer<LavaTrack> player)
		{
			bool wasEmpty = player.Vueue.Count == 0 && player.PlayerState != PlayerState.Playing;

			List<LavaTrack> tracks = new ();

			foreach (string search in playlist)
			{
				SearchResponse searchResult = await LavaNode.SearchAsync(SearchType.YouTube, search);

				var track = searchResult.Tracks.FirstOrDefault();

				if (track is not null)
				{
					tracks.Add(track);
					if (player.Vueue.Count == 0 && player.PlayerState != PlayerState.Playing)
					{
						await player.PlayAsync(track);
					}
					else
					{
						lock (MusicEvent.QueueLocker)
						{
							player.Vueue.Enqueue(track);
						}
					}
				}
			}

			List<EmbedBuilder> embeds;

			if (wasEmpty)
				embeds = player.GetQueue("🎶 Playlist Music Queue", MusicEvent);
			else
				embeds = tracks.ToArray().GetQueueFromTrackArray("🎶 Playlist Music Queue");

			await InteractiveService.CreateReactionMenu(embeds, Context);
		}

		public async Task SearchSingleTrack(string search, LavaPlayer<LavaTrack> player, bool searchList)
		{
			SearchResponse searchResult;

			searchResult = await LavaNode.SearchAsync(SearchType.YouTube, search);

			var track = searchResult.Tracks.FirstOrDefault();

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
				var topResults = searchResult.Tracks.Count <= 5 ? searchResult.Tracks.ToList() : searchResult.Tracks.Take(5).ToList();

				string line1 = topResults.Count <= 5
					? $"I found {topResults.Count} tracks matching your search."
					: $"I found {searchResult.Tracks.Count:N0} tracks matching your search, here are the top 5.";

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

				track = searchResult.Tracks.Where(search => result.Value.EndsWith(search.Title)).FirstOrDefault();
			}

			if (player.Vueue.Count == 0 && player.PlayerState != PlayerState.Playing)
			{
				await player.PlayAsync(track);

				await CreateEmbed(EmojiEnum.Unknown)
					.GetNowPlaying(track)
					.SendEmbed(Context.Interaction);
			}
			else
			{
				lock (MusicEvent.QueueLocker)
				{
					player.Vueue.Enqueue(track);
				}

				await CreateEmbed(EmojiEnum.Unknown)
					.GetQueuedTrack(track, player.Vueue.Count)
					.SendEmbed(Context.Interaction);
			}
		}

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
