using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord.Interactions;

namespace DexterSlash.Commands.UtilityCommands
{

	public partial class BaseUtilityCommand
	{

		[SlashCommand("latency", "Gets the estimate round-trip latency to the gateway server.")]
		[Global]

		public async Task Ping()
		{
			await CreateEmbed(EmojiEnum.Love)
				.WithTitle("Gateway Ping")
				.WithDescription($"Current latency:\n**{Context.Client.Latency}ms**")
				.SendEmbed(Context.Interaction);
		}

	}

}
