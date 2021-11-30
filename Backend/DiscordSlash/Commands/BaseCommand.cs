using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands
{
    public abstract class BaseCommand<T> : InteractionModuleBase<ShardedInteractionContext>
	{

        public ILogger<T> Logger { get; set; }

		public EmbedBuilder CreateEmbed(EmojiEnum thumbnail)
		{
			return new EmbedBuilder().CreateEmbed(thumbnail, EmbedCallingType.Command);
		}

	}
}
