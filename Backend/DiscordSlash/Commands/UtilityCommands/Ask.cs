using DexterSlash.Attributes;
using Discord.Interactions;
using Genbox.WolframAlpha;

namespace DexterSlash.Commands.UtilityCommands
{
	public partial class BaseUtilityCommand
	{

		public WolframAlphaClient WolframAlphaClient { get; set; }

		[SlashCommand("ask", "Evaluates mathematical expressions and answers questions!")]

		public async Task Ask([MaxLength(1250)] string question)
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

