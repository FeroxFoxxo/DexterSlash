﻿using DexterSlash.Enums;
using DexterSlash.Attributes;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;

namespace DexterSlash.Commands.UtilityCommands
{

    public class AvatarCommand : BaseCommand<AvatarCommand>
	{

		[SlashCommand("avatar", "Gets the avatar of a user mentioned or yours.")]
		[Global]

		public async Task Avatar(IUser user = null)
		{
			if (user == null)
				user = Context.User;

			await CreateEmbed(EmojiEnum.Unknown)
				.WithImageUrl(user.GetAvatarUrl(1024))
				.WithUrl(user.GetAvatarUrl(1024))
				.WithAuthor(user)
				.WithTitle("Get Avatar URL")
				.SendEmbed(Context.Interaction);
		}

	}

}