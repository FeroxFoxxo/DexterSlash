using Dexter.Enums;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands
{
    public abstract class BaseCommand<T> : InteractionModuleBase<SocketInteractionContext>
	{

        public ILogger<T> Logger { get; set; }

		/// <summary>
		/// The Create Embed method is a generic method that simply calls upon the EMBED BUILDER extension method.
		/// </summary>
		/// <param name="thumbnail">The thumbnail that you would like to be applied to the embed.</param>
		/// <returns>A new embed builder with the specified attributes applied to the embed.</returns>

		public EmbedBuilder CreateEmbed(EmojiEnum thumbnail)
		{
			return new EmbedBuilder().CreateEmbed(thumbnail, EmbedCallingType.Command);
		}

	}
}
