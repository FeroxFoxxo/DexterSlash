using Dexter.Enums;
using DexterSlash.Attributes;
using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.ModeratorCommands
{
    public class UserDMCommand : BaseCommand<UserDMCommand>
    {

		/// <summary>
		/// Sends a direct message to a target user.
		/// </summary>
		/// <param name="user">The target user</param>
		/// <param name="message">The full message to send the target user</param>
		/// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

		[SlashCommand("userdm", "Sends a direct message to a user specified.")]
		[DefaultsAdmin]

		public async Task UserDM(IUser user, string message)
		{
			if (user is null)
			{
				await RespondAsync(
					embed: CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Unable to find given user!")
					.WithDescription("This may be due to caching! Try using their ID if you haven't.")
					.Build(),

					ephemeral: true
				);
				return;
			}

			if (string.IsNullOrEmpty(message))
			{
				await RespondAsync(
					embed: CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle("Empty message!")
					.WithDescription("I received an empty message. It would be rude for me to send that; I believe.")
					.Build(),

					ephemeral: true
				);
				return;
			}

			await CreateEmbed(EmojiEnum.Love)
				.WithTitle("User DM")
				.WithDescription(message)
				.AddField("Recipient", user.GetUserInformation())
				.AddField("Sent By", Context.User.GetUserInformation())
				.SendDMAttachedEmbed(user,
					CreateEmbed(EmojiEnum.Unknown)
					.WithTitle($"Message From {Context.Guild.Name}")
					.WithDescription(message)
				);
		}

    }
}
