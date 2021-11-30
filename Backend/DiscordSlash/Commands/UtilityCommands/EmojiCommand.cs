using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.UtilityCommands
{

    public class EmojiCommand : BaseCommand<EmojiCommand>
	{

		[SlashCommand("emoji", "Gets the full image of an emoji.")]
		[Global]

		public async Task Emoji(string Emoji)
		{
			if (Emote.TryParse(Emoji, out Emote Emojis))
				await CreateEmbed(EmojiEnum.Unknown)
					.WithImageUrl(Emojis.Url)
					.WithUrl(Emojis.Url)
					.WithAuthor(Emojis.Name)
					.WithTitle("Get Emoji URL")
					.SendEmbed(Context.Interaction);
			else
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unknown Emoji")
					.WithDescription("An invalid emoji was specified! Please make sure that what you have sent is a valid emoji. " +
						"Please make sure this is a **custom emoji** aswell, and that it does not fall under the unicode specification.")
					.SendEmbed(Context.Interaction);
		}

	}

}
