using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using System.Collections.Concurrent;

namespace DexterSlash.Events
{
    public class MusicEvent : Event
    {
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _disconnectTokens;

        private readonly IAudioService _audioService;
        private readonly ILogger<MusicEvent> _logger;

        public MusicEvent(IAudioService audioService, ILogger<MusicEvent> logger)
        {
            _disconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
            _logger = logger;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            //_audioService.TrackEnd += AudioService_TrackEnd;
            //_audioService.TrackStuck += AudioService_TrackStuck;
            //_audioService.TrackStarted += AudioService_TrackStarted;
            //_audioService.TrackException += AudioService_TrackException;
        }

        private async Task AudioService_TrackException(object sender, TrackExceptionEventArgs eventArgs)
        {
            if (eventArgs.Player is VoteLavalinkPlayer player)
            {
                _logger.LogWarning($"Track {player.CurrentTrack?.Title} threw an exception. " +
                    $"Please check Lavalink console/logs.");

                if (player.Queue.Count == 0)
                {
                    await player.StopAsync();
                    await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                }
                else
                {
                    await player.SkipAsync();
                }
            }
        }

        private async Task AudioService_TrackStuck(object sender, TrackStuckEventArgs eventArgs)
        {
            if (eventArgs.Player is VoteLavalinkPlayer player)
            {
                _logger.LogWarning($"Track {player.CurrentTrack?.Title} got stuck. " +
                    $"Please check Lavalink console/logs.");

                if (player.IsLooping)
                {
                    if (player.CurrentTrack != null)
                    {
                        var currentSong = eventArgs.TrackIdentifier;
                        var track = await _audioService.GetTrackAsync(currentSong, SearchMode.YouTube, true);
                        if (track != null)
                        {
                            await player.PlayAsync(track);
                        }
                        else
                        {
                            await player.StopAsync();
                            await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                        }
                    }
                    else
                    {
                        await player.StopAsync();
                        await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                    }
                }

                if (player.Queue.Count == 0 && !player.IsLooping)
                {
                    await player.StopAsync();
                    await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                }
                else
                {
                    await player.SkipAsync();
                }
            }
        }

        private async Task AudioService_TrackStarted(object sender, TrackStartedEventArgs eventArgs) => 
            await CancelDisconnectAsync(eventArgs.Player);

        private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
        {
            if (eventArgs.Reason != TrackEndReason.LoadFailed)
            {
                if (eventArgs.Player is VoteLavalinkPlayer player)
                {
                    if (player.IsLooping)
                    {
                        await player.ReplayAsync();
                        return;
                    }

                    if (player.Queue.IsEmpty && eventArgs.Reason != TrackEndReason.Replaced)
                    {
                        await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
                    }
                }
            }
            else
            {
                if (eventArgs.Player is VoteLavalinkPlayer player)
                {
                    if (player.Queue.IsEmpty)
                    {
                        await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
                        await player.StopAsync();
                    }
                    else
                    {
                        await player.SkipAsync();
                    }
                }
            }
        }

        public async Task CancelDisconnectAsync(LavalinkPlayer player)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
            {
                value = new CancellationTokenSource();
            }
            else if (value.IsCancellationRequested)
            {
                return;
            }

            await Task.Run(() => value.Cancel(true));
        }

        public async Task InitiateDisconnectAsync(LavalinkPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannelId, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannelId ?? 0];
            }

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
        }

    }
}