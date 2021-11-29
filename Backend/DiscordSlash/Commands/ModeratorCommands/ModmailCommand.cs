using Dexter.Enums;
using DexterSlash.Attributes;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

namespace DexterSlash.Commands.ModeratorCommands
{
	public class ModmailCommand : BaseCommand<ModmailCommand>
	{
		public GuildConfigRepository GuildConfigRepository { get; set; }
		public ModMailRepository ModMailRepository { get; set; }
		public DiscordShardedClient DiscordShardedClient { get; set; }

		/// <summary>
		/// Sends an anonymous message to the moderators in a specific channel prepared for this.
		/// </summary>
		/// <param name="Message">The string message to send to the channel as a modmail.</param>
		/// <returns>A <c>Task</c> object, which can be awaited until the method completes successfully.</returns>

		[SlashCommand("modmail", "Sends an anonymous message to the moderators, which will not show on the server.")]
		[EnabledBy(Modules.Modmail)]

		public async Task ModMail([MaxLength(1250)] string message)
		{
			var guildConfig = await GuildConfigRepository.GetGuildConfig(Context.Guild.Id);

			if (!guildConfig.ModMailChannelID.HasValue)
            {
				await RespondAsync("Oops! This commands hasn't been set up correctly! " +
                    "Please ask your server admins to disable, then re-enable the modmail module!",
					ephemeral: true);

				return;
            }

			var mail = await ModMailRepository.CreateModMail(
				message,
				Context.User.Id
			);

			IUserMessage usrMessage = await (DiscordShardedClient.GetChannel(guildConfig.ModMailChannelID.Value) as ITextChannel).SendMessageAsync(
				embed: CreateEmbed(EmojiEnum.Unknown)
					.WithTitle($"Anonymous Modmail #{mail.TrackerID}")
					.WithDescription(mail.Message)
					.WithFooter($"ID: {mail.TrackerID}")
					.Build()
			);

			await ModMailRepository.UpdateModMail(mail.TrackerID, usrMessage.Id);

			var mailEmbed = CreateEmbed(EmojiEnum.Love)
				.WithTitle("Successfully Sent Modmail")
				.WithDescription($"Haiya! Your message has been sent to the staff team.\n\n" +
					$"Your modmail token is: `{mail.TrackerID}`, which is what the moderators use to reply to you. " +
					$"Only give this out to a moderator if you wish to be identified.\n\n" +
					$"Thank you~! - {Context.Guild.Name} Staff Team <3")
				.WithFooter($"ID: {mail.TrackerID}");

			try
            {
				var dmChannel = await Context.User.CreateDMChannelAsync();

				await dmChannel.SendMessageAsync(embed: mailEmbed.Build());
            }
			catch (HttpException)
            {
				await RespondAsync(
					embed: mailEmbed
						.WithAuthor($"Psst, please unblock me or allow direct messages from {Context.Guild.Name}. <3")
						.Build(),
					ephemeral: true
				);
            }
		}

	}
}
