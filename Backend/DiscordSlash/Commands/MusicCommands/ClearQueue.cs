using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("clearqueue", "Clears the current music player queue.")]
		[DJMusic]

		public async Task ClearQueue()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to clear queue!")
					.WithDescription("I couldn't find the music player for this server.\n" +
					"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			try
			{
				int songCount = player.Vueue.Count;

				player.Vueue.Clear();

				await player.StopAsync();

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Playback halted.")
					.WithDescription($"Cleared {songCount} from playing in {player.VoiceChannel.Name}.")
					.SendEmbed(Context.Interaction);
			}
			catch (Exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to clear queue!")
					.WithDescription($"Failed to clear queue.\nIf the issue persists, please contact the developers for support.")
					.SendEmbed(Context.Interaction);

				Logger.LogError($"Failed to clear queue from voice channel {player.VoiceChannel.Name} in {Context.Guild.Id}.");

				return;
			}
		}
	}
}
