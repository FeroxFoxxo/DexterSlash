using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.ModeratorCommands
{
    [Group("moderator", "A list of moderator-only commands.")]
    public class UserDMCommand : BaseCommand<UserDMCommand>
    {

		[SlashCommand("dm", "Sends a direct message to a user specified.")]
		[DefaultsAdmin]

		public async Task UserDM(IUser user, string message)
		{
			if (user is null)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to find given user!")
					.WithDescription("This may be due to caching! Try using their ID if you haven't.")
					.SendEmbed(Context.Interaction);

				return;
			}

			if (string.IsNullOrEmpty(message))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Empty message!")
					.WithDescription("I received an empty message. It would be rude for me to send that; I believe.")
					.SendEmbed(Context.Interaction);

				return;
			}

			await CreateEmbed(EmojiEnum.Love)
				.WithTitle("User DM")
				.WithDescription(message)
				.AddField("Recipient", user.GetUserInformation())
				.AddField("Sent By", Context.User.GetUserInformation())
				.SendDMAttachedEmbed(
					Context.Interaction,
					user,
					CreateEmbed(EmojiEnum.Unknown)
						.WithTitle($"Message From {Context.Guild.Name}")
						.WithDescription(message)
				);
		}

	}
}
