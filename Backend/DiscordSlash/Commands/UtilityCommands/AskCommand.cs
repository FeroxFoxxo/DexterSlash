using Discord.Interactions;
using Genbox.WolframAlpha;

namespace DexterSlash.Commands.UtilityCommands
{
    public class AskCommand : BaseCommand<AskCommand>
	{

		public WolframAlphaClient WolframAlphaClient { get; set; }

		/// <summary>
		/// Evaluates a mathematical expression and gives a result or throws an error.
		/// </summary>
		/// <param name="question">A properly formatted stringified math expression.</param>
		/// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

		[SlashCommand("ask", "Evaluates mathematical expressions and answers questions!")]

		public async Task WolframCommand(string question)
		{
			string response = await WolframAlphaClient.SpokenResultAsync(question);

			response = response.Replace("Wolfram Alpha", Context.Client.CurrentUser.Username);
			response = response.Replace("Wolfram|Alpha", Context.Client.CurrentUser.Username);
			response = response.Replace("Stephen Wolfram", "the goat overlords");
			response = response.Replace("and his team", "and their team");

			if (response == "DexterBot did not understand your input" || response == "No spoken result available")
				await RespondAsync(text: response, ephemeral: true);
			else
				await RespondAsync(text: $"**{question}**\n{response}");
		}

	}

}

