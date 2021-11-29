using DexterSlash.Enums;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models
{
    public class GuildConfig
    {
        [Key]
        public ulong GuildId { get; set; }

        public Modules EnabledModules { get; set; }

        public string ModInternalNotificationWebhook { get; set; }

        public ulong? ModMailChannelID { get; set; }

    }
}
