using DexterSlash.Databases.Enums;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// Represents a recorded instance of a name change for a username.
    /// </summary>

    public class NameRecord
	{

		/// <summary>
		/// A unique identifier for the name record.
		/// </summary>

		[Key]
		public int Index { get; set; }

		/// <summary>
		/// The unique identifier of the user who used this name.
		/// </summary>

		public ulong UserID { get; set; }

		/// <summary>
		/// The Name being recorded.
		/// </summary>

		public string Name { get; set; }

		/// <summary>
		/// Represents the time the name was first set, in seconds since UNIX time.
		/// </summary>

		public long SetTime { get; set; }

		/// <summary>
		/// Whether the name record represents a USERNAME or a NICKNAME.
		/// </summary>

		public NameType Type { get; set; }

	}
}
