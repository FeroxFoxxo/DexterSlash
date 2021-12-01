using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("stop", "Displays the current music queue.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Stop()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to stop songs!")
					.WithDescription("I couldn't find the music player for this server.\n" +
					"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			string vcName = $"**{player.VoiceChannel.Name}**";

			try
			{
				string prevTrack = player.Track.Title;

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
