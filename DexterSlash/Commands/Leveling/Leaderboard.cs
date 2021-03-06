using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.LevelingCommands
{

	public partial class BaseUtilityCommand
	{

		[SlashCommand("leaderboard", "Gets the link to the server's experience leaderboard.")]

		public async Task Leaderboard()
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
