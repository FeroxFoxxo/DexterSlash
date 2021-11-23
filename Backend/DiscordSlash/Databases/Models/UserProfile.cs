using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{

    /// <summary>
    /// The Borkday class contains information on a user's last borkday.
    /// </summary>

    public class UserProfile
    {

        /// <summary>
        /// The UserID is the KEY of the table.
        /// It is the snowflake ID of the user that has had the borkday.
        /// </summary>

        [Key]

        public ulong UserID { get; set; }

        /// <summary>
        /// The UNIX time of when the borkday role was added last.
        /// </summary>

        public long BorkdayTime { get; set; }

        /// <summary>
        /// The time the user joined for the first time, expressed in seconds since UNIX time.
        /// </summary>

        public long DateJoined { get; set; }

        /// <summary>
        /// The user's gender and pronouns.
        /// </summary>

        public string? Gender { get; set; }

        /// <summary>
        /// The user's sexual and romantic orientation.
        /// </summary>

        public string? Orientation { get; set; }

        /// <summary>
        /// The Unique Integer representation of the Borkday object.
        /// </summary>

        public short? BorkdayValue { get; set; }

        /// <summary>
        /// The user's birth year.
        /// </summary>

        public int? BirthYear { get; set; }

        /// <summary>
        /// The token for the timer that control the borkday role event for the user attached to this profile.
        /// </summary>

        public string? BorkdayTimerToken { get; set; }

        /// <summary>
        /// The user's time zone abbreviation for non-daylight saving time.
        /// </summary>

        public string TimeZone { get; set; }

        /// <summary>
        /// The user's time zone abbreviation for daylight saving time.
        /// </summary>

        public string? TimeZoneDST { get; set; }

        /// <summary>
        /// The Unique Integer representation of the DSTRules object. 
        /// </summary>

        public int? DstRulesValue { get; set; }

        /// <summary>
        /// The user's sona information provided by the user.
        /// </summary>

        public string? SonaInfo { get; set; }

        /// <summary>
        /// The user's location, up to the specificity that the user wishes to set.
        /// </summary>

        public string? Nationality { get; set; }

        /// <summary>
        /// The user's known languages. 
        /// </summary>

        public string? Languages { get; set; }

        /// <summary>
        /// Miscellaneous user information.
        /// </summary>

        public string? Info { get; set; }

        /// <summary>
        /// Represents the compressed binary representation of the per-user specific profile preferences. 
        /// </summary>

        public long? SettingsValue { get; set; }

    }

}
