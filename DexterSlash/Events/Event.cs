using DexterSlash.Enums;
using DexterSlash.Extensions;
using Discord;

namespace DexterSlash.Events
{
    public abstract class Event
    {

        public abstract void Initialize();

        public EmbedBuilder CreateEmbed(EmojiEnum thumbnail)
        {
            return new EmbedBuilder().CreateEmbed(thumbnail, EmbedCallingType.Command);
        }
    }
}
