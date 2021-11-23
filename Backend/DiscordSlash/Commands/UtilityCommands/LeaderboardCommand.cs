using Dexter.Enums;
using DexterSlash.Commands;
using Discord;
using Discord.Interactions;

namespace Dexter.Commands
{

    public class LeaderboardCommand : BaseCommand<LeaderboardCommand>
	{

		[SlashCommand("leaderboard", "Gets the link to the server's experience leaderboard.")]

		public async Task Invite()
		{
			var button = new ComponentBuilder()
				.WithButton(
					"Go to your leaderboard",
					style: ButtonStyle.Link,
					url: $"https://us-furries.com/leaderboards/{Context.Guild.Id}"
				)
				.Build();

			await RespondAsync("Leaderboard mode, activate! 💥", component: button, ephemeral: true);
		}

	}

}
