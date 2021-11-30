using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Victoria.Node;

namespace DexterSlash.Commands.MusicCommands
{
    [Group("music", "A list of commands that play music in voice channels.")]
	public class JoinCommand : BaseCommand<JoinCommand>
	{

		public LavaNode LavaNode { get; set; }

		[SlashCommand("join", "Tells me to join the voice channel you are currently in.")]
		[EnabledBy(Modules.Music)]

		public async Task Join()
		{
			if (LavaNode.HasPlayer(Context.Guild))
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Unable to join channel!")
					.WithDescription("I'm already connected to a voice channel somewhere in this server.")
					.SendEmbed(Context.Interaction);

				return;
			}

			var voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel == null)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Unable to join channel!")
					.WithDescription("You must be connected to a voice channel to use this command.")
					.SendEmbed(Context.Interaction);

				return;
			}

			try
			{
				await LavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

				await CreateEmbed(EmojiEnum.Love)
					.WithTitle($"Joined {voiceState.VoiceChannel.Name}!")
					.WithDescription("Hope you have a blast!")
					.SendEmbed(Context.Interaction);
			}
			catch (Exception exception)
			{
				await CreateEmbed(EmojiEnum.Annoyed)
					.WithTitle($"Failed to join {voiceState.VoiceChannel.Name}.")
					.WithDescription("Error: " + exception.Message)
					.SendEmbed(Context.Interaction);
			}
		}
	}

}
