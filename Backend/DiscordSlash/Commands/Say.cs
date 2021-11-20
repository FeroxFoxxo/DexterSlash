using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace DiscordSlash.Commands
{
    public class Say : Base<Say>
    {

        [SlashCommand("say", "Let the bot send a message.")]

        public async Task SayCommand(InteractionContext ctx, [Option("message", "message content the bot shall write")] string message,
            [Option("channel", "channel to write the message in, defaults to current")] DiscordChannel channel = null)
        {
            if (channel == null)
            {
                channel = ctx.Channel;
            }

            if (channel.Type != ChannelType.Text)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Only text channels are allowed.").AsEphemeral(true));
                return;
            }

            try
            {
                await channel.SendMessageAsync(message);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Message sent.").AsEphemeral(true));

            }
            catch (UnauthorizedException)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("I'm not allowed to view or send messages in this channel!").AsEphemeral(true));
                return;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while writing message in channel {channel.Id}.");

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Failed to send message.").AsEphemeral(true));
                return;
            }
        }

    }
}
