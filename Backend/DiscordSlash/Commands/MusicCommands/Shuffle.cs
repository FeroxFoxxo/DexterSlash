using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("shuffle", "Shuffles the music queue in a random order.")]
		[DJMusic]

		public async Task Shuffle()
		{
			var player = AudioService.TryGetPlayer(Context, "shuffle queue");

			if (!player.Queue.Any())
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to shuffle queue!")
					.WithDescription(
						"There aren't any songs in the queue.\n" +
						"Please add songs to the queue with the `play` command and try again.")
					.SendEmbed(Context.Interaction);

				return;
			}

			player.Queue.Shuffle();

			await QueueEmbed("1", "🔀 Queue Shuffle");
		}

	}
}
