using Microsoft.EntityFrameworkCore;

namespace DiscordSlash.Services
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    }
}
