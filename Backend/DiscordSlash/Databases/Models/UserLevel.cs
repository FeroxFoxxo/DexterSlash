using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// Holds relevant information about user XP for a given user
    /// </summary>

    public class UserLevel
	{

		/// <summary>
		/// The unique ID of the user this object represents and tracks
		/// </summary>

		[Key]

		public ulong UserID { get; set; }

		/// <summary>
		/// The total Voice XP of the user
		/// </summary>

		public long VoiceXP { get; set; } = 0;

		/// <summary>
		/// The total Text XP of the user 
		/// </summary>

		public long TextXP { get; set; } = 0;

	}

}
