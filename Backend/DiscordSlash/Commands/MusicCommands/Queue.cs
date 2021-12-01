using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("queue", "Displays the current queue of songs.")]
		public async Task Queue(int pageNumber = 0)
		{
			await QueueEmbed("1", "🎶 Music Queue");
		}

		[ComponentInteraction("queue-button:*,*")]

		public async Task QueueEmbed(string parsedPageNumber, string name)
		{
			if (int.TryParse(parsedPageNumber, out int pageNumber))
			{
				var player = AudioService.TryGetPlayer(Context, "display queue");

				var embeds = player.GetQueue(name);

				if (pageNumber - 1 < 0)
				{
					pageNumber = 0;
				}
				else
				{
					pageNumber--;
				}

				var button = new ComponentBuilder()
					.WithButton("First", $"queue-button:1,{name}", ButtonStyle.Secondary)
					.WithButton("Back", $"queue-button:{pageNumber},{name}", ButtonStyle.Secondary)
					.WithButton("Clear", "clear-button")
					.WithButton("Next", $"queue-button:{pageNumber + 2},{name}", ButtonStyle.Secondary)
					.WithButton("Last", $"queue-button:{embeds.Count},{name}", ButtonStyle.Secondary);

				await embeds[pageNumber].SendEmbed(Context.Interaction, component: button);
			}
		}

	}
}
