using DexterSlash.Enums;
using DexterSlash.Extensions;
using DexterSlash.Workers;
using Discord.WebSocket;
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
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _aloneDisconnectTokens;

        private readonly IAudioService _audioService;
        private readonly DiscordShardedClient _client;
        private readonly ILogger<MusicEvent> _logger;

        public MusicEvent(IAudioService audioService, ILogger<MusicEvent> logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
            _audioService = audioService;

            _disconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
            _aloneDisconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
        }

        public override void Initialize()
        {
            _audioService.TrackEnd += TrackEnd;
            _audioService.TrackStuck += TrackStuck;
            _audioService.TrackStarted += TrackStarted;
            _audioService.TrackException += TrackException;

            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
        {
            var voiceSocket1 = voiceState1.VoiceChannel;
            var voiceSocket2 = voiceState2.VoiceChannel;

            if (voiceSocket1 != null)
            {
                if (IsBotInChannel(voiceSocket1))
                {
                    var player = _audioService.GetPlayer(voiceSocket1.Guild.Id);

                    if (NumberOfUserInChannel(voiceSocket1) >= 2)
                    {
                        await CancelAloneDisconnectAsync(player);
                    }
                    else
                    {
                        await DisconnectBot(player);
                    }
                }
            }

            if (voiceSocket2 != null)
            {
                if (IsBotInChannel(voiceSocket2))
                {
                    var player = _audioService.GetPlayer(voiceSocket2.Guild.Id);
                    if (NumberOfUserInChannel(voiceSocket2) >= 2)
                    {
                        await CancelAloneDisconnectAsync(player);
                    }
                    else
                    {
                        await DisconnectBot(player);
                    }
                }
            }
        }

        private async Task DisconnectBot(LavalinkPlayer player)
        {
            await Task.Run(async () =>
            {
                await BotIsAloneAsync(player, TimeSpan.FromSeconds(30));
                return Task.CompletedTask;
            });
        }

        public async Task BotIsAloneAsync(LavalinkPlayer player, TimeSpan timeSpan)
        {
            if (!_aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
            {
                value = new CancellationTokenSource();
                _aloneDisconnectTokens.TryAdd(player.VoiceChannelId, value);
            }
            else if (value.IsCancellationRequested)
            {
                _aloneDisconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
                value = _aloneDisconnectTokens[player.VoiceChannelId ?? 0];
            }

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
        }

        private bool IsBotInChannel(SocketGuildChannel socketChannel)
        {
            var bot = socketChannel.Users.FirstOrDefault(x => x.Id.Equals(_client.CurrentUser.Id));
            return bot != null;
        }

        private int NumberOfUserInChannel(SocketGuildChannel socketChannel)
        {
            var voiceUsers = _client.Guilds.FirstOrDefault(
                    x => x.Name.Equals(socketChannel.Guild.Name))
                ?.VoiceChannels.FirstOrDefault(
                    x => x.Name.Equals(socketChannel.Name))
                ?.Users;

            return voiceUsers?.Count ?? 0;
        }

        private async Task TrackException(object sender, TrackExceptionEventArgs eventArgs)
        {
            if (eventArgs.Player is DexterPlayer player)
            {
                _logger.LogWarning($"Track {player.CurrentTrack?.Title} threw an exception. Please check Lavalink console/logs.");

                await CreateEmbed(EmojiEnum.Annoyed)
                    .WithTitle("Oops! This is on us.")
                    .WithDescription($"We could not play {player.CurrentTrack?.Title} because of an error.\n" +
                        $"Please contact a developer if this error persists!")
                    .SendEmbed(player.Context.Interaction);

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

        private async Task TrackStuck(object sender, TrackStuckEventArgs eventArgs)
        {

            if (eventArgs.Player is DexterPlayer player)
            {
                _logger.LogWarning($"Track {player.CurrentTrack?.Title} got stuck. Please check Lavalink console/logs.");

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

        private async Task TrackStarted(object sender, TrackStartedEventArgs eventArgs) =>
            await CancelDisconnectAsync(eventArgs.Player);

        private async Task TrackEnd(object sender, TrackEndEventArgs eventArgs)
        {
            if (eventArgs.Reason != TrackEndReason.LoadFailed)
            {
                if (eventArgs.Player is DexterPlayer player)
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
                if (eventArgs.Player is DexterPlayer player)
                {
                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Oops! This is on us.")
                        .WithDescription($"We could not play {player.CurrentTrack?.Title} because of an error.\n" +
                            $"Please contact a developer if this error persists!")
                        .SendEmbed(player.Context.Interaction);

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

        public async Task CancelAloneDisconnectAsync(LavalinkPlayer player)
        {
            if (!_aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
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