using Discord;

namespace DexterSlash.Extensions
{
    public static class UserExtensions
    {

		/// <summary>
		/// Returns the URL for a User's avatar, or the URL of the user's Default Discord avatar (Discord logo with a set background color) if they're using a default avatar.
		/// </summary>
		/// <param name="user">Target user whose avatar is being obtained.</param>
		/// <param name="size">The size of the image to return in. This can be any power of 2 in the range [16, 2048].</param>
		/// <returns>A string holding the URL of the target user's avatar.</returns>

		public static string GetAvatarUrl(this IUser user, ushort size = 128)
		{
			return string.IsNullOrEmpty(user.GetAvatarUrl(size: size)) ? user.GetDefaultAvatarUrl() : user.GetAvatarUrl(size: size);
		}

		/// <summary>
		/// The GetUserInformation method returns a string of the users username, followed by the discriminator, the mention and the ID.
		/// It is used as a standardized way throughout the bot to display information on a user.
		/// </summary>
		/// <param name="user">The user of which you want to create the standardized string of the user's information of.</param>
		/// <returns>A string which contains the user's username, discriminator, mention and ID.</returns>

		public static string GetUserInformation(this IUser user)
		{
			return $"{user.Username}#{user.Discriminator} ({user.Mention}) ({user.Id})";
		}

	}
}
