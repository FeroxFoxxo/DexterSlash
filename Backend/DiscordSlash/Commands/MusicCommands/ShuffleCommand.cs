using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Fergun.Interactive;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("shuffle", "Shuffles the music queue in a random order.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Shuffle()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to shuffle queue!")
					.WithDescription(
						"I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (!player.Vueue.Any())
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to shuffle queue!")
					.WithDescription(
						"There aren't any songs in the queue.\n" +
						"Please add songs to the queue with the `play` command and try again.")
					.SendEmbed(Context.Interaction);

				return;
			}

			player.Vueue.Shuffle();

			var embeds = player.GetQueue("🔀 Queue Shuffle", MusicEvent);

			await InteractiveService.CreateReactionMenu(embeds, Context);
		}

	}
}
