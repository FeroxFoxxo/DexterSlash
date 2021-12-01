using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("leave", "Disconnects me from the current voice channel.")]
		[DJMusic]

		public async Task Leave()
		{
			var player = AudioService.TryGetPlayer(Context, "leave vc");

			string vcName = $"**{Context.Guild.GetVoiceChannel(player.VoiceChannelId.Value).Name}**";

			try
			{
				await player.DisconnectAsync();

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Sucessfully left voice channel!")
					.WithDescription($"Disconnected from {vcName}.")
					.SendEmbed(Context.Interaction);
			}
			catch (Exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to leave VC!")
					.WithDescription($"Failed to disconnect from {vcName}.\nIf the issue persists, please contact the developers for support.")
					.SendEmbed(Context.Interaction);

				Logger.LogError($"Failed to disconnect from voice channel {vcName} in {Context.Guild.Id} via $leave.");

				return;
			}
		}

	}
}
