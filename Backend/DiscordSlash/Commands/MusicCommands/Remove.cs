using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("remove", "Removes a song at a given position in the queue.")]
		[DJMusic]

		public async Task Remove(int index)
		{
			var player = AudioService.TryGetPlayer(Context, "remove song");

			if (player.Queue.Count < index)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to remove song!")
					.WithDescription($"I couldn't find a song at the index of {index}. The length of the queue is {player.Queue.Count}.")
					.SendEmbed(Context.Interaction);

				return;
			}

			var rtrack = player.Queue.ToArray()[index];

			player.Queue.Remove(rtrack);

			await CreateEmbed(EmojiEnum.Love)
				.WithTitle($"📑 Removed {rtrack.Title}!")
				.WithDescription($"I successfully removed {rtrack.Title} by {rtrack.Author} at position {index}.")
				.SendEmbed(Context.Interaction);
		}
	}
}
