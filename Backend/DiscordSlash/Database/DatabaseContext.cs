using DiscordSlash.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordSlash.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<GuildConfig> GuildConfigs { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<ulong[]>().HaveConversion<ULAConverter>();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ULAComparer ulongArrayComparer = new();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                    if (property.ClrType == typeof(ulong[]))
                        property.SetValueComparer(ulongArrayComparer);
        }
    }

}
