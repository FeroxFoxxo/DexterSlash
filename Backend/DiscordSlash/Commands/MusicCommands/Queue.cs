using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Fergun.Interactive;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("queue", "Displays the current queue of songs.")]

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
