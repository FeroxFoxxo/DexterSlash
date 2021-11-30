using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Databases.Models.GuildConfiguration
{
    public abstract class ConfigBase
    {
        [Key]
        public ulong GuildId { get; set; }

    }
}
