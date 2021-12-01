using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("volume", "Changes the volume. Values are 0-150 and 100 is the default..")]
		[DJMusic]

		public async Task Volume(int volumeLevel = 100)
		{
			var player = AudioService.TryGetPlayer(Context, "change volume");

			if (volumeLevel > 1000 || volumeLevel < 0)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to change volume!")
					.WithDescription("Volume out of range: 0% - 1000%!")
					.SendEmbed(Context.Interaction);
				return;
			}

			try
			{
				float oldVolume = player.Volume;

				await player.SetVolumeAsync(volumeLevel / 100f);

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
