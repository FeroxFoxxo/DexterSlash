using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
	[Group("music", "A list of commands that play music in voice channels.")]
	public class RemoveCommand : BaseCommand<RemoveCommand>
	{

		public LavaNode LavaNode { get; set; }

		[SlashCommand("remove", "Removes a song at a given position in the queue.")]
		[EnabledBy(Modules.Music)]
		[DJMusic]

		public async Task Remove(int index)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to remove song!")
					.WithDescription("I couldn't find the music player for this server.\n" +
						"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (player.Vueue.Count < index)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to remove song!")
					.WithDescription($"I couldn't find a song at the index of {index}. The length of the queue is {player.Vueue.Count}.")
					.SendEmbed(Context.Interaction);

				return;
			}

			var rtrack = player.Vueue.ToArray()[index];

			player.Vueue.Remove(rtrack);

			await CreateEmbed(EmojiEnum.Love)
				.WithTitle($"📑 Removed {rtrack.Title}!")
				.WithDescription($"I successfully removed {rtrack.Title} by {rtrack.Author} at position {index}.")
				.SendEmbed(Context.Interaction);
		}
	}
}
