using Dexter.Enums;
using DexterSlash.Commands;
using Discord;
using Discord.Interactions;

namespace Dexter.Commands
{

    public class InviteCommand : BaseCommand<InviteCommand>
	{

		[SlashCommand("invite", "Gets the bot's invite link.")]

		public async Task Invite()
		{
			var button = new ComponentBuilder()
				.WithButton(
					"Invite me",
					style: ButtonStyle.Link,
					url: $"https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=2048&redirect_uri=https%3A%2F%2Fus-furries.com&scope=bot%20applications.commands"
				)
				.Build();

			await RespondAsync("Try not to hurt me! ✨", component: button, ephemeral: true);
		}

	}

}
