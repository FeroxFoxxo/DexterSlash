using Discord.Commands;

namespace DexterSlash.Commands
{
    public abstract class BaseCommand<T> : ModuleBase<SocketCommandContext>
    {

        public ILogger<T> Logger { get; set; }

    }
}
