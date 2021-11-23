using DexterSlash.Databases.Enums;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// A record for a specific GreetFur and a specific day
    /// </summary>

    public class GreetFurRecord
	{
		/// <summary>
		/// Unique identifier for the record object.
		/// </summary>
		[Key]
		public uint RecordId { get; set; }

		/// <summary>
		/// Unique user identifier for the user this record refers to.
		/// </summary>
		public ulong UserId { get; set; }

		/// <summary>
		/// The amount of days since UNIX time before the day that this record represents.
		/// </summary>
		public int Date { get; set; }
		
		/// <summary>
		/// The amount of GreetFur-eligible messages sent by the user in the given day.
		/// </summary>
		public int MessageCount { get; set; }

		/// <summary>
		/// Marks the kind of activity that this user has had for the given day.
		/// </summary>
		public ActivityFlags Activity { get; set; }

	}
}
