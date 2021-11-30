using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.WebSocket;
using Fergun.Interactive;
using System.Collections.Concurrent;
using System.Diagnostics;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace DexterSlash.Events
{
    public class MusicEvent : Event
    {
        private readonly DiscordShardedClient _client;

        private readonly LavaNode _lavaNode;

        private readonly ILogger<MusicEvent> _logger;

		private readonly InteractiveService _interactiveService;

		private readonly IServiceProvider _services;

        private readonly List<int> _shardsReady;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

        public readonly Dictionary<ulong, LoopType> LoopedGuilds;

        public object LoopLocker;

        public object QueueLocker;

        public MusicEvent(ILogger<MusicEvent> logger, DiscordShardedClient client, InteractiveService interactive, LavaNode lavaNode, IServiceProvider services)
        {
            _client = client;
            _logger = logger;
            _interactiveService = interactive;
            _lavaNode = lavaNode;
			_services = services;

            _shardsReady = new();
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

            LoopLocker = new();
            QueueLocker = new();

            LoopedGuilds = new();
        }

        public override void Initialize()
        {
            _client.ShardReady += ClientOnShardReady;
            _client.JoinedGuild += DisposeMusicPlayerAsync;
            _client.LeftGuild += DisposeMusicPlayerAsync;
            _client.UserVoiceStateUpdated += ProtectPlayerIntegrityOnDisconnectAsync;

            _lavaNode.OnTrackStart += OnTrackStarted;
			_lavaNode.OnTrackEnd += OnTrackEnded;
		}
		private async Task ClientOnShardReady(DiscordSocketClient client)
		{
			AddReadyShard(client.ShardId);

			if (_shardsReady.Count == _client.Shards.Count)
			{
				if (!_lavaNode.IsConnected)
				{
					// If an error is thrown here, please see if your LavaLink is running!
					// There is a logs folder for errors, but the common issue is Java 10 not being installed.
					// You will have to do this each time you want to test.

					await _services.UseLavaNodeAsync();
				}
			}
		}

		public void AddReadyShard(int shardId)
		{
			if (!_shardsReady.Contains(shardId))
			{
				_shardsReady.Add(shardId);
			}
		}

		private async Task OnTrackStarted(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEvent)
		{
			_logger.LogInformation($"Track started for guild {trackEvent.Player.VoiceChannel.Guild.Id}:\n\t" +
								   $"[Name: {trackEvent.Track.Title} | Duration: {trackEvent.Track.Duration.HumanizeTimeSpan()}]");

			lock (LoopLocker)
				if (!LoopedGuilds.ContainsKey(trackEvent.Player.VoiceChannel.Guild.Id))
					LoopedGuilds.Add(trackEvent.Player.VoiceChannel.Guild.Id, LoopType.Off);

			if (!_disconnectTokens.TryGetValue(trackEvent.Player.VoiceChannel.Id, out var value))
				return;

			if (value.IsCancellationRequested)
				return;

			value.Cancel(true);
		}

		private async Task OnTrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEvent)
		{
			_logger.LogInformation($"Track ended for guild {trackEvent.Player.VoiceChannel.Guild.Id} " +
								   $"-> {trackEvent.Player.Vueue.Count:N0} tracks remaining.");

			if (trackEvent.Reason == TrackEndReason.LoadFailed)
			{
				_logger.LogError($"Load failed for track in guild: {trackEvent.Player.VoiceChannel.Guild.Id}\n\t" +
								 $"Track info: [Name: {trackEvent.Track.Title} | Duration: {trackEvent.Track.Duration.HumanizeTimeSpan()} | " +
								 $"Url: {trackEvent.Track.Url} | Livestream?: {trackEvent.Track.IsStream}]");

				return;
			}

			if (trackEvent.Reason != TrackEndReason.Stopped && trackEvent.Reason != TrackEndReason.Finished && trackEvent.Reason != TrackEndReason.LoadFailed)
				return;

			var player = trackEvent.Player;

			if (player == null)
				return;

			bool canDequeue;

			LavaTrack queueable;

			LoopType loopType = LoopType.Off;

			lock (LoopLocker)
				if (LoopedGuilds.ContainsKey(trackEvent.Player.VoiceChannel.Guild.Id))
					loopType = LoopedGuilds[trackEvent.Player.VoiceChannel.Guild.Id];

			if (loopType == LoopType.All)
				lock (QueueLocker)
					player.Vueue.Enqueue(player.Track);

			if (loopType != LoopType.Single)
			{
				while (true)
				{
					canDequeue = player.Vueue.TryDequeue(out queueable);

					if (queueable != null || !canDequeue)
						break;
				}
			}
			else
			{
				canDequeue = true;
				queueable = player.Track;
			}

			if (!canDequeue)
			{
				if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
				{
					value = new CancellationTokenSource();
					_disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
				}
				else if (value.IsCancellationRequested)
				{
					_disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
					value = _disconnectTokens[player.VoiceChannel.Id];
				}

				await Task.Delay(TimeSpan.FromSeconds(15), value.Token);

				if (value.IsCancellationRequested)
					return;

				if (player.PlayerState == PlayerState.Playing)
					return;

				var dcEmbed = CreateEmbed(EmojiEnum.Unknown)
					.WithDescription("🎵 No more songs in queue, disconnecting!");

				await _lavaNode.LeaveAsync(player.VoiceChannel);

				lock (LoopLocker)
					if (LoopedGuilds.ContainsKey(trackEvent.Player.VoiceChannel.Guild.Id))
						LoopedGuilds.Remove(trackEvent.Player.VoiceChannel.Guild.Id);

				await _interactiveService.DelayedSendMessageAndDeleteAsync
					(player.TextChannel, deleteDelay: TimeSpan.FromSeconds(10), embed: dcEmbed.Build());

				return;
			}

			await trackEvent.Player.PlayAsync(queueable);

			if (queueable is null)
				return;

			await _interactiveService.DelayedSendMessageAndDeleteAsync(
				trackEvent.Player.TextChannel,
				null,
				TimeSpan.FromSeconds(10),
				embed: CreateEmbed(EmojiEnum.Unknown).GetNowPlaying(queueable).Build()
			);
		}

		private async Task ProtectPlayerIntegrityOnDisconnectAsync(SocketUser user, SocketVoiceState ogState, SocketVoiceState newState)
		{
			if (!AllShardsReady(_client))
				return;

			if (!user.IsBot)
				if (ogState.VoiceChannel is not null)
					if (ogState.VoiceChannel.Users.Where(user => user.Id == _client.CurrentUser.Id).FirstOrDefault() is not null)
						if (ogState.VoiceChannel.Users.Count <= 1)
						{
							await _lavaNode.LeaveAsync(ogState.VoiceChannel ?? newState.VoiceChannel);

							lock (LoopLocker)
								if (LoopedGuilds.ContainsKey(ogState.VoiceChannel.Guild.Id))
									LoopedGuilds.Remove(ogState.VoiceChannel.Guild.Id);

						}

			if (user.Id != _client.CurrentUser.Id || newState.VoiceChannel != null)
				return;

			try
			{
				await _lavaNode.LeaveAsync(ogState.VoiceChannel ?? newState.VoiceChannel);

				lock (LoopLocker)
					if (LoopedGuilds.ContainsKey(ogState.VoiceChannel.Guild.Id))
						LoopedGuilds.Remove(ogState.VoiceChannel.Guild.Id);
			}
			catch (Exception) { }
		}

		private bool AllShardsReady(DiscordShardedClient client)
		{
			return client.Shards.Count == _shardsReady.Count;
		}

		private async Task DisposeMusicPlayerAsync(SocketGuild guild)
		{
			if (_lavaNode.TryGetPlayer(guild, out var player))
			{
				try
				{
					await _lavaNode.LeaveAsync(player.VoiceChannel);
					await player.DisposeAsync();
					_logger.LogInformation($"Guild {guild.Id} had an active music player. " + "It has been properly disposed of.");
				}
				catch (Exception) { }
			}
		}
	}
}
