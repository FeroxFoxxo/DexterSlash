using DiscordSlash.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordSlash.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<GuildConfig> GuildConfigs { get; set; }

    }
}
