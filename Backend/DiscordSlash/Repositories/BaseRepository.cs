using DiscordSlash.Database;

namespace DiscordSlash.Repositories
{
    public class BaseRepository<T>
    {

        protected readonly DatabaseContext _context;

        public BaseRepository(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
