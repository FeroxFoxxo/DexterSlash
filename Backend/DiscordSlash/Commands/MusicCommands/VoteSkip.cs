using DexterSlash.Attributes;
using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.MusicCommands
{
	public partial class BaseMusicCommand
	{

		[SlashCommand("voteskip", "Initializes a skip vote on the current song.")]
		[ComponentInteraction("voteskip-button")]

		public async Task Vote()
        {
			var player = AudioService.TryGetPlayer(Context, "vote skip song");

			var musicConfig = await new ConfigRepository(Services).GetGuildConfig<ConfigMusic>(Modules.Music, Context.Guild.Id);

			var votePercentage = .5f;

			if (musicConfig.VotePercentage.HasValue)
				votePercentage = musicConfig.VotePercentage.Value;

			var newVote = await player.VoteAsync(Context.User.Id, votePercentage);

			if (!newVote.WasSkipped)
			{
				var button = new ComponentBuilder().WithButton("⏭️ Vote Skip", "voteskip-button", ButtonStyle.Success);

				await CreateEmbed(EmojiEnum.Unknown)
					.WithTitle("Vote Skip")
					.WithDescription(
						$"The votes required to skip is {Math.Ceiling(newVote.TotalUsers * votePercentage)}.")
					.AddField("Current Votes:", $"{Math.Ceiling(newVote.Percentage * 100)}%")
					.SendEmbed(Context.Interaction, component: button);
			}
			else
            {
				await CreateEmbed(EmojiEnum.Love)
					.WithTitle("Song Has Been Skipped!")
					.WithDescription("The required vote percentage has been met!\n" +
						$"{newVote.Votes.Count} users have decided to skip the song!")
					.SendEmbed(Context.Interaction);
			}
		}
	}
}
