using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Events;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in voice channels.")]
	public class LoopCommand : BaseCommand<LoopCommand>
	{

		public LavaNode LavaNode { get; set; }
		public MusicEvent MusicEvent { get; set; }

		[SlashCommand("loop", "Toggles looping of the current playlist between `single` / `all` / `off`.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Loop(LoopType loopType)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to loop songs!")
					.WithDescription("Failed to join voice channel.\nAre you in a voice channel?")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to loop songs!")
					.WithDescription("The player must be actively playing a track in order to loop it.")
					.SendEmbed(Context.Interaction);

				return;
			}


			lock (MusicEvent.LoopLocker)
				MusicEvent.LoopedGuilds[player.VoiceChannel.Guild.Id] = loopType;

			switch (loopType)
			{
				case LoopType.Single:
					await CreateEmbed(EmojiEnum.Unknown)
						.WithTitle($"🔂 Repeated Current Track")
						.WithDescription($"Successfully started repeating **{player.Track.Title}**.")
						.SendEmbed(Context.Interaction);
					break;
				case LoopType.All:
					await CreateEmbed(EmojiEnum.Unknown)
						.WithTitle($"🔂 Looping Tracks")
						.WithDescription($"Successfully started looping **{player.Vueue.Count + 1} tracks**.")
						.SendEmbed(Context.Interaction);
					break;
				case LoopType.Off:
					await CreateEmbed(EmojiEnum.Unknown)
						.WithTitle($"🔂 Stopped Looping Tracks")
						.WithDescription($"Successfully stopped looping the current queue.")
						.SendEmbed(Context.Interaction);
					break;
			}
		}
	}
}
