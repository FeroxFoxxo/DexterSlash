using System.ComponentModel.DataAnnotations;

namespace DiscordSlash.Models
{
    public class GuildConfig
    {
        [Key]
        public ulong GuildId { get; set; }

        public ulong[] DJRoles { get; set; }

        public ulong[] ElevatedRoles { get; set; }

        public ulong[] WelcomerRoles { get; set; }

        public ulong[] ModRoles { get; set; }

        public ulong[] AdminRoles { get; set; }

        public ulong[] MutedRoles { get; set; }

        public string ModInternalNotificationWebhook { get; set; }

    }
}
