using Discord;

namespace DexterSlash.Extensions
{
    public static class UserExtensions
    {

		public static string GetAvatarUrl(this IUser user, ushort size = 128)
		{
			return string.IsNullOrEmpty(user.GetAvatarUrl(size: size)) ? user.GetDefaultAvatarUrl() : user.GetAvatarUrl(size: size);
		}

		public static string GetUserInformation(this IUser user)
		{
			return $"{user.Username}#{user.Discriminator}";
		}

	}
}
