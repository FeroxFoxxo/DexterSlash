using DexterSlash.Databases.Enums;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

	/// <summary>
	/// Represents a set of restrictions that prevents a specific user from accessing certain Dexter Features.
	/// </summary>

	public class UserRestriction
	{

		/// <summary>
		/// The unique ID of the user this restriction affects.
		/// </summary>

		[Key]
		public ulong UserID { get; set; }

		/// <summary>
		/// The individual restrictions applied to the user this restriction represents.
		/// </summary>

		public Restriction RestrictionFlags { get; set; }

	}
}
