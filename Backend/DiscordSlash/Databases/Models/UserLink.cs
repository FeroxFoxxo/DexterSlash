using System.ComponentModel.DataAnnotations;
using DexterSlash.Databases.Enums;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// Represents a social connection between two users
    /// </summary>

    public class UserLink
	{

		/// <summary>
		/// Represents the unique ID assigned to this link.
		/// </summary>

		[Key]
		public ulong ID { get; set; }

		/// <summary>
		/// The type of link this object represents.
		/// </summary>

		public LinkType LinkType { get; set; }

		/// <summary>
		/// The ID of the user that effected this link.
		/// </summary>

		public ulong Sender { get; set; }

		/// <summary>
		/// The ID of the user affected by this link.
		/// </summary>

		public ulong Sendee { get; set; }

		/// <summary>
		/// The stringified representation of the settings object.
		/// </summary>

		public string SettingsStr { get; set; }

	}

}
