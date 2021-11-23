namespace DexterSlash.Databases.Enums
{
	/// <summary>
	/// Indicates which type of link a userLink represents.
	/// </summary>

	public enum LinkType : byte
	{

		/// <summary>
		/// Represents a friend request from one user to another.
		/// </summary>

		FriendRequest,

		/// <summary>
		/// Indicates that a user is a friend of another
		/// </summary>

		Friend,

		/// <summary>
		/// Blocks friend requests from one specific user
		/// </summary>

		Blocked,

		/// <summary>
		/// Auxiliary value used for temporary manipulation of requests.
		/// </summary>

		Invalid

	}

}
