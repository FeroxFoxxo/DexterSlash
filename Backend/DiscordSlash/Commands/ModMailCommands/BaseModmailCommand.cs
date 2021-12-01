using Discord.Interactions;
using Discord.WebSocket;

namespace DexterSlash.Commands.ModeratorCommands
{
    [Group("modmail", "A list of commands that allows modmail messages to be sent and recieved.")]
	public partial class BaseModmailCommand : BaseCommand<BaseModmailCommand>
	{
		public DiscordShardedClient DiscordShardedClient { get; set; }
		public IServiceProvider Services { get; set; }

	}
}
