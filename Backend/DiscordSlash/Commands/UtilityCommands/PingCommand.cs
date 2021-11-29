using Dexter.Enums;
using DexterSlash.Attributes;
using DexterSlash.Commands;
using Discord.Interactions;

namespace Dexter.Commands.UtilityCommands
{

    public class PingCommand : BaseCommand<PingCommand>
	{

		/// <summary>
		/// Displays the latency between Discord's API and the bot.
		/// </summary>
		/// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

		[SlashCommand("latency", "Gets the estimate round-trip latency to the gateway server.")]
		[Global]

		public async Task Ping()
		{
			await RespondAsync(
				embed:
					CreateEmbed(EmojiEnum.Love)
						.WithTitle("Gateway Ping")
						.WithDescription($"Current latency:\n**{Context.Client.Latency}ms**")
						.Build()
			);
		}

	}

}
