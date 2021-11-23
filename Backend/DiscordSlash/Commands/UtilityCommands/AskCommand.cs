using Discord;
using Discord.Commands;
using Discord.Interactions;
using Genbox.WolframAlpha;

namespace DexterSlash.Commands.UtilityCommands
{
	public class AskCommand : BaseCommand<AskCommand>
	{

		public WolframAlphaClient WolframAlphaClient;

		/// <summary>
		/// Evaluates a mathematical expression and gives a result or throws an error.
		/// </summary>
		/// <param name="question">A properly formatted stringified math expression.</param>
		/// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

		[SlashCommand("ask", "Evaluates mathematical expressions and answers questions!")]

		public async Task WolframCommand([Remainder] string question)
		{
			string Response = await WolframAlphaClient.SpokenResultAsync(question);

			Response = Response.Replace("Wolfram Alpha", Context.Client.CurrentUser.Username);
			Response = Response.Replace("Wolfram|Alpha", Context.Client.CurrentUser.Username);
			Response = Response.Replace("Stephen Wolfram", "the goat overlords");
			Response = Response.Replace("and his team", "and their team");

			if (Response == "DexterBot did not understand your input" || Response == "No spoken result available")
				await RespondAsync(text: Response, ephemeral: true);
			else
				await RespondAsync(text: Response);
		}

	}

}

