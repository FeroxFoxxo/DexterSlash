using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Events;
using DexterSlash.Extensions;
using Discord.Interactions;
using Fergun.Interactive;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	[Group("music", "A list of commands that play music in voice channels.")]
	public class QueueCommand : BaseCommand<QueueCommand>
	{

		public LavaNode LavaNode { get; set; }
		public InteractiveService InteractiveService { get; set; }
		public MusicEvent MusicEvent { get; set; }

		[SlashCommand("queue", "Displays the current queue of songs.")]
		[EnabledBy(Modules.Music)]

		public async Task Queue()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
						.WithTitle("Could not display queue.")
						.WithDescription(
							"I couldn't find the music player for this server.\n" +
							"Please ensure I am connected to a voice channel before using this command.")
						.SendEmbed(Context.Interaction);

				return;
			}

			var embeds = player.GetQueue("🎶 Music Queue", MusicEvent);

			await InteractiveService.CreateReactionMenu(embeds, Context);
		}

	}
}
