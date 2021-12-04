using DexterSlash.Databases.Context;

namespace DexterSlash.Databases.Repositories
{
    public class BaseRepository
    {

        protected readonly DatabaseContext _context;

        public BaseRepository(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
