using DexterSlash.Attributes;
using DexterSlash.Enums;
using Discord.Interactions;

namespace DexterSlash.Commands.ModeratorCommands
{
    [Group("moderator", "A list of moderator-only commands.")]
    [Module(Modules.Moderator)]
    public partial class BaseModeratorCommand : BaseCommand<BaseModeratorCommand>
    {

	}
}
