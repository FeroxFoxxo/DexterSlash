using DiscordSlash.Database;

namespace DiscordSlash.Repositories
{
    public class BaseRepository<T>
    {

        protected ILogger<T> Logger { get; set; }

        protected DatabaseContext Context { get; set; }

        protected readonly IServiceProvider ServiceProvider;

        public BaseRepository(IServiceProvider serviceProvider)
        {
            Logger = (ILogger<T>)serviceProvider.GetService(typeof(ILogger<T>));

            Context = (DatabaseContext)serviceProvider.GetService(typeof(DatabaseContext));

            ServiceProvider = serviceProvider;
        }
    }
}
