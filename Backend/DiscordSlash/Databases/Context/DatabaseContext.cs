using DexterSlash.Databases.Models;
using DexterSlash.Databases.Models.GuildConfiguration;
using Microsoft.EntityFrameworkCore;

namespace DexterSlash.Databases.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // Guild Configurations

        public DbSet<ConfigModMail> ConfigModMail { get; set; }

        public DbSet<ConfigMusic> ConfigMusic { get; set; }

        public DbSet<ConfigLeveling> ConfigLeveling { get; set; }

        // Models 

        public DbSet<ModMail> ModMails { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<ulong[]>().HaveConversion<ULAConverter>();
        }

        public async Task<bool> CanConnectAsync()
        {
            return await Database.CanConnectAsync();
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
