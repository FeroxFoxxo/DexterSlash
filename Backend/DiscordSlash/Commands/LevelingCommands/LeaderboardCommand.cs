using DexterSlash.Attributes;
using DexterSlash.Commands;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;

namespace Dexter.Commands.LevelingCommands
{

    public class LeaderboardCommand : BaseCommand<LeaderboardCommand>
	{

		/// <summary>
		/// Returns the link to the guild's leaderboard and posts it in chat.
		/// </summary>
		/// <returns>A <c>Task</c> object, which can be awaited until the method completes successfully.</returns>

		[SlashCommand("leaderboard", "Gets the link to the server's experience leaderboard.")]
		[EnabledBy(Modules.Leveling)]

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
