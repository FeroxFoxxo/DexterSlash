using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

	/// <summary>
	/// The Mod Mail class contains information on a message sent to the moderators.
	/// </summary>

	public class ModMail
	{

		/// <summary>
		/// The TrackerID field is the KEY of the table. It is unique per mail.
		/// </summary>

		[Key]

		public int TrackerID { get; set; }

		/// <summary>
		/// The UserID field is the snowflake ID of the user who had sent the modmail message.
		/// </summary>

		public ulong UserID { get; set; }

		/// <summary>
		/// The Message field is the contents of the message sent to the moderators.
		/// </summary>

		public string Message { get; set; }

		/// <summary>
		/// The Message ID field is the ID of the embed that has been sent into the modmail channel.
		/// </summary>

		public ulong? MessageID { get; set; }

	}

}
