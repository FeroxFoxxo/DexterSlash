using DexterSlash.Databases.Enums;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// Represents a particular instance of a game or session in the Dexter Games subsystem.
    /// </summary>

    public class GameInstance
	{

		/// <summary>
		/// Unique identifier of the game instance.
		/// </summary>

		[Key]
		public int GameID { get; set; }

		/// <summary>
		/// The last time this game instance received any interactions, game instances should be closed after a while without interaction.
		/// Measured in seconds since UNIX Time.
		/// </summary>

		public long LastInteracted { get; set; }

		/// <summary>
		/// The unique ID of the last user who interacted with the game system.
		/// </summary>

		public ulong LastUserInteracted { get; set; }

		/// <summary>
		/// The game type that's being played in this instance.
		/// </summary>

		public GameType Type { get; set; }

		/// <summary>
		/// This session's title, generally shown on Embed Titles.
		/// </summary>

		public string Title { get; set; }

		/// <summary>
		/// This session's description.
		/// </summary>

		public string Description { get; set; }

		/// <summary>
		/// A password for this game instance, required for joining if different from <see cref="string.Empty"/>.
		/// </summary>

		public string Password { get; set; }

		/// <summary>
		/// This game's Game Master (or Host) who has control over it.
		/// </summary>

		public ulong Master { get; set; }

		/// <summary>
		/// A list of Comma-separated UserIDs for users that are banned from joining this instance.
		/// </summary>

		public string Banned { get; set; }

		/// <summary>
		/// Any additional game-specific data managed at a per-<see cref="GameTemplate"/> level.
		/// </summary>

		public string Data { get; set; }

	}

}
