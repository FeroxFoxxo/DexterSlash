using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;
using DexterSlash.Events;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in voice channels.")]
	public class LeaveCommand : BaseCommand<LeaveCommand>
	{

		public LavaNode LavaNode { get; set; }
		public MusicEvent MusicEvent { get; set; }

		[SlashCommand("leave", "Disconnects me from the current voice channel.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Leave()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to leave VC!")
					.WithDescription("I couldn't find the music player for this server.\n" +
					"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			string vcName = $"**{player.VoiceChannel.Name}**";

			try
			{
				await LavaNode.LeaveAsync(player.VoiceChannel);

				lock (MusicEvent.LoopLocker)
					if (MusicEvent.LoopedGuilds.ContainsKey(player.VoiceChannel.Guild.Id))
						MusicEvent.LoopedGuilds.Remove(player.VoiceChannel.Guild.Id);

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
