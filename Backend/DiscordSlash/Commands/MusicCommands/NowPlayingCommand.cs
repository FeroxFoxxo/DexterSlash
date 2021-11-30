using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;

namespace DexterSlash.Commands.MusicCommands
{
	[Group("music", "A list of commands that play music in voice channels.")]
	public class NowPlayingCommand : BaseCommand<NowPlayingCommand>
	{

		public LavaNode LavaNode { get; set; }

		[SlashCommand("nowplaying", "Display the currently playing song.")]
		[EnabledBy(Modules.Music)]

		public async Task NowPlaying()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to find current song!")
					.WithDescription("I couldn't find the music player for this server.\n" +
					"Please ensure I am connected to a voice channel before using this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to find current song!")
					.WithDescription("The player must be actively playing a track in order to see its information.")
					.SendEmbed(Context.Interaction);

				return;
			}

			await CreateEmbed(EmojiEnum.Unknown)
				.GetNowPlaying(player.Track)
				.SendEmbed(Context.Interaction);
		}
	}
}
