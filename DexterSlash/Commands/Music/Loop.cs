using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Lavalink4NET.Player;

namespace DexterSlash.Commands.MusicCommands
{
    public partial class BaseMusicCommand
	{

		[SlashCommand("loop", "Toggles looping of the current playlist between on and off.")]
		[ComponentInteraction("loop-button")]
		[AttributeDJ]

		public async Task Loop()
		{
			var player = AudioService.TryGetPlayer(Context, "loop song");

			if (player.State != PlayerState.Playing)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to loop songs!")
					.WithDescription("The player must be actively playing a track in order to loop it.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (!player.IsLooping)
			{
				player.IsLooping = true;

				var button = new ComponentBuilder().WithButton("Stop Looping", "loop-button", ButtonStyle.Secondary);

				await CreateEmbed(EmojiEnum.Unknown)
						.WithTitle($"🔂 Looping Tracks")
						.WithDescription($"Successfully started looping **{player.Queue.Count + 1} tracks**.")
						.SendEmbed(Context.Interaction, component: button);
			}
			else
			{
				player.IsLooping = false;

				var button = new ComponentBuilder().WithButton("Loop", "loop-button", ButtonStyle.Secondary);

				await CreateEmbed(EmojiEnum.Unknown)
					.WithTitle($"🔂 Stopped Looping Tracks")
					.WithDescription($"Successfully stopped looping the current queue.")
					.SendEmbed(Context.Interaction, component: button);
			}
		}
	}
}
