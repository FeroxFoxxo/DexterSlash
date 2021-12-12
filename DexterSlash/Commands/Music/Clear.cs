using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("clear", "Clears the current music player queue.")]
		[ComponentInteraction("clear-button")]
		[AttributeDJ]

		public async Task Clear()
		{
			var player = AudioService.TryGetPlayer(Context, "clear queue");

			string vcName = $"**{Context.Guild.GetVoiceChannel(player.VoiceChannelId.Value).Name}**";

			try
			{
				if (player != null && player.Queue.Tracks.Count > 0)
				{
					int songCount = player.Queue.Tracks.Count;

					player.Queue.Clear();

					await player.StopAsync();

					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Playback halted.")
						.WithDescription($"Cleared {songCount} from playing in {vcName}.")
						.SendEmbed(Context.Interaction);
				}
				else
					await CreateEmbed(EmojiEnum.Love)
						.WithTitle("Playback halted.")
						.WithDescription($"The queue was empty, so I wasn't able to clear anything!.")
						.SendEmbed(Context.Interaction);
			}
			catch (Exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to clear queue!")
					.WithDescription($"Failed to clear queue.\nIf the issue persists, please contact the developers for support.")
					.SendEmbed(Context.Interaction);

				Logger.LogError($"Failed to clear queue from voice channel {vcName} in {Context.Guild.Id}.");

				return;
			}
		}
	}
}
