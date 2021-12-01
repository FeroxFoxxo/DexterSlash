using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("stop", "Displays the current music queue.")]
		[DJMusic]

		public async Task Stop()
		{
			var player = AudioService.TryGetPlayer(Context, "stop songs");

			string vcName = $"**{Context.Guild.GetVoiceChannel(player.VoiceChannelId.Value).Name}**";

			try
			{
				string prevTrack = player.CurrentTrack.Title;

				await player.StopAsync();

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Playback halted.")
					.WithDescription($"Stopped {prevTrack} from playing in {vcName}.").SendEmbed(Context.Interaction);
			}
			catch (Exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to stop songs!")
					.WithDescription($"Failed to disconnect from {vcName}.\nIf the issue persists, please contact the developers for support.")
					.SendEmbed(Context.Interaction);

				Logger.LogError($"Failed to disconnect from voice channel '{vcName}' in {Context.Guild.Id}.");

				return;
			}
		}
	}
}
