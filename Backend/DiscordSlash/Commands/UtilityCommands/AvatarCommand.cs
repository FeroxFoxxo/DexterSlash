using Dexter.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Commands;
using System.Runtime.InteropServices;

namespace DexterSlash.Commands.UtilityCommands
{

	public class AvatarCommand : BaseCommand<AvatarCommand>
	{

		/// <summary>
		/// Sends in the target user's profile picture as a full-resolution image. If no user is provided, defaults to Context.User.
		/// </summary>
		/// <param name="User">The target user, default to Context.User.</param>
		/// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

		[Command("avatar")]
		[Summary("Gets the avatar of a user mentioned or yours.")]
		
		public async Task SendAvatar([Optional] IUser User)
		{
			if (User == null)
				User = Context.User;

			await RespondAsync(
				embed:
					CreateEmbed(EmojiEnum.Unknown)
					.WithImageUrl(User.GetAvatarUrl(1024))
					.WithUrl(User.GetAvatarUrl(1024))
					.WithAuthor(User)
					.WithTitle("Get Avatar URL")
					.Build()
				);
		}

	}

}
