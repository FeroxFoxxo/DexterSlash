using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("volume", "Changes the volume. Values are 0-150 and 100 is the default..")]
		[DJMusic]

		public async Task Volume(int volumeLevel = 100)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to change volume!")
					.WithDescription("I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			try
			{
				int oldVolume = player.Volume;

				await player.SetVolumeAsync(volumeLevel);

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Volume changed.")
					.WithDescription($"Sucessfully changed volume from {oldVolume} to {volumeLevel}")
					.SendEmbed(Context.Interaction);
			}
			catch (Exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to change volume!")
					.WithDescription($"Failed to change volume to {volumeLevel}.\nIf the issue persists, please contact the developers for support.")
					.SendEmbed(Context.Interaction);

				Logger.LogError($"Failed to change volume in {Context.Guild.Id}.");

				return;
			}
		}
	}
}
