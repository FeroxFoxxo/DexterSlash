using DSharpPlus.SlashCommands;

namespace DiscordSlash.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public abstract class BaseCommand<T> : ApplicationCommandModule
    {

        public ILogger<T> Logger { get; set; }

    }
}
