using DSharpPlus.SlashCommands;

namespace DiscordSlash.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public abstract class Base<T> : ApplicationCommandModule
    {

        public ILogger<T> Logger { get; set; }

    }
}
