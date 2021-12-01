﻿using DexterSlash.Enums;
using DexterSlash.Exceptions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Lavalink4NET;
using Lavalink4NET.Player;
using System.Diagnostics;
using System.Reflection;

namespace DexterSlash.Extensions
{
    public static class EmbedExtensions
	{
		public static EmbedBuilder CreateEmbed(this EmbedBuilder embedBuilder, EmojiEnum thumbnails, EmbedCallingType calledType)
		{
			Color Color = thumbnails switch
			{
				EmojiEnum.Annoyed => Color.Red,
				EmojiEnum.Love => Color.Green,
				EmojiEnum.Sign => Color.Blue,
				EmojiEnum.Wut => Color.Gold,
				EmojiEnum.Unknown => Color.Teal,
				_ => Color.Magenta
			};

			string name;
			try
			{
				name = GetLastMethodCalled(2).Key;

				string toDelete = calledType switch
				{
					EmbedCallingType.Command => "Command",
					EmbedCallingType.Service => "Service",
					EmbedCallingType.Game => "Game",
					_ => ""
				};

				name = name.Replace(toDelete, "");

				name = string.Concat(name.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
			}
			catch
			{
				name = "Unknown";
			}

			string thumbnailURL = thumbnails switch
			{
				EmojiEnum.Annoyed => "https://cdn.discordapp.com/attachments/781077443338960926/808664878977646602/DexAnnoyed.png",
				EmojiEnum.Love => "https://cdn.discordapp.com/attachments/781077443338960926/807479083297931264/DexLove.png",
				EmojiEnum.Sign => "https://cdn.discordapp.com/attachments/781077443338960926/808664325014290462/DexterSignAwesome.png",
				EmojiEnum.Wut => "https://cdn.discordapp.com/attachments/781077443338960926/808664061440294933/DexterWut.png",
				EmojiEnum.Unknown => "",
				_ => ""
			};

			return embedBuilder
				.WithThumbnailUrl(thumbnailURL)
				.WithColor(Color)
				.WithCurrentTimestamp()
				.WithFooter($"USFurries {name} Module");
		}

		public static KeyValuePair<string, string> GetLastMethodCalled(int searchHeight)
		{
			searchHeight += 1;

			Type mBase = new StackTrace().GetFrame(searchHeight).GetMethod().DeclaringType;

			if (mBase.Assembly != Assembly.GetExecutingAssembly() || mBase.Namespace == typeof(EmbedExtensions).Namespace)
				return GetLastMethodCalled(searchHeight + 1);

			string name;

			if (mBase.DeclaringType != null)
				name = mBase.DeclaringType.Name;
			else
				name = mBase.Name;

			string methodName = mBase.Name;

			int Index = methodName.IndexOf(">d__");

			if (Index != -1)
				methodName = methodName[..Index].Replace("<", "");

			return new KeyValuePair<string, string>(name, methodName);
		}

		public static async Task SendDMAttachedEmbed(this EmbedBuilder embedBuilder, SocketInteraction interaction, IUser user, EmbedBuilder dmEmbedBuilder)
		{
			if (user == null)
				embedBuilder.AddField("Failed", "I cannot notify this fluff as they have left the server!");
			else
			{
				try
				{
					IMessageChannel dmChannel = await user.CreateDMChannelAsync();
					await dmChannel.SendMessageAsync(embed: dmEmbedBuilder.Build());
				}
				catch
				{
					embedBuilder.CreateEmbed(EmojiEnum.Annoyed, EmbedCallingType.Command);
					embedBuilder.AddField("Failed", "This fluff may have either blocked DMs from the server or me!");
				}
			}

			await SendEmbed(embedBuilder, interaction, false);
		}

		public static async Task SendEmbed(this EmbedBuilder embedBuilder, SocketInteraction interaction,
			bool? overrideEphemeral = null, ComponentBuilder component = null)
		{
			bool ephemeral = overrideEphemeral ?? (embedBuilder.Color == Color.Red);

			await interaction.RespondAsync(
				embed: embedBuilder.Build(),
				ephemeral: ephemeral,
				component: component?.Build());
		}

		public static EmbedBuilder GetNowPlaying(this EmbedBuilder builder, LavalinkTrack track)
		{
			return builder.WithTitle("🎵 Now playing").WithDescription(
								$"Title: **{track.Title}**\n" +
								$"Duration: **[{track.Position.HumanizeTimeSpan()}/{track.Duration.HumanizeTimeSpan()}]**");
		}

		public static EmbedBuilder GetQueuedTrack(this EmbedBuilder builder, LavalinkTrack track, int queueSize)
		{
			return builder.WithTitle("⏳ Queued").WithDescription(
								$"Title: **{track.Title}**\n" +
								$"Duration: **{track.Duration.HumanizeTimeSpan()}**\n" +
								$"Queue Position: **{queueSize}**.");
		}

		public static VoteLavalinkPlayer TryGetPlayer (this IAudioService audioService, ShardedInteractionContext context, string commandEntry)
        {
			
			var player = audioService.GetPlayer<VoteLavalinkPlayer>(context.Guild.Id);

			if (player == null)
				throw new MusicException(commandEntry);

			return player;
		}

		public static List<EmbedBuilder> GetQueue(this VoteLavalinkPlayer player, string title)
		{
			var embeds = player.Queue.ToArray().GetQueueFromTrackArray(title);

			if (player.CurrentTrack != null)
			{
				var timeRem = player.CurrentTrack.Duration - player.CurrentTrack.Position +
					TimeSpan.FromSeconds(player.Queue.Select(x => x.Duration.TotalSeconds).Sum());

				embeds
					.First()
					.WithDescription($"**Now {(!player.IsLooping ? "Playing" : "Looping")}:**\n" +
						$"Title: **{player.CurrentTrack.Title}** " +
						$"[{player.CurrentTrack.Position.HumanizeTimeSpan()} / {player.CurrentTrack.Duration.HumanizeTimeSpan()}]\n\n" +
						$"**Duration Left:** \n" +
						$"{player.Queue.Count + 1} Tracks [{timeRem.HumanizeTimeSpan()}]\n\n" +
						"Up Next ⬇️" + embeds.First().Description);
			}

			return embeds;
		}

		public static async Task CreateReactionMenu(this InteractiveService interactiveService, List<EmbedBuilder> embeds, ShardedInteractionContext context)
		{
			if (embeds.Count > 1)
			{
				PageBuilder[] pageBuilderMenu = new PageBuilder[embeds.Count];

				for (int i = 0; i < embeds.Count; i++)
					pageBuilderMenu[i] = PageBuilder.FromEmbedBuilder(embeds[i]);

				Paginator paginator = new StaticPaginatorBuilder()
					.WithPages(pageBuilderMenu)
					.WithDefaultEmotes()
					.WithFooter(PaginatorFooter.PageNumber)
					.WithActionOnCancellation(ActionOnStop.DeleteInput)
					.WithActionOnTimeout(ActionOnStop.DeleteInput)
										.Build();

				await context.Interaction.DeferAsync();

				await interactiveService.SendPaginatorAsync(paginator, context.Channel, TimeSpan.FromMinutes(10));
			}
			else
				await embeds.FirstOrDefault().SendEmbed(context.Interaction);
		}

		public static List<EmbedBuilder> GetQueueFromTrackArray(this LavalinkTrack[] tracks, string title)
		{
			EmbedBuilder currentBuilder = new EmbedBuilder()
				.CreateEmbed(EmojiEnum.Unknown, EmbedCallingType.Command)
				.WithTitle(title);

			List<EmbedBuilder> embeds = new();

			if (tracks.Length == 0)
			{
				currentBuilder.WithDescription(currentBuilder.Description += "\n\n*No tracks enqueued.*");
			}

			for (int index = 0; index < tracks.Length; index++)
			{
				EmbedFieldBuilder field = new EmbedFieldBuilder()
					.WithName($"#{index + 1}. **{tracks[index].Title}**")
					.WithValue($"{tracks[index].Author} ({tracks[index].Duration:mm\\:ss})");

				if (index % 5 == 0 && index != 0)
				{
					embeds.Add(currentBuilder);

					currentBuilder = new EmbedBuilder()
						.CreateEmbed(EmojiEnum.Unknown, EmbedCallingType.Command)
						.AddField(field);
				}
				else
				{
					try
					{
						currentBuilder.AddField(field);
					}
					catch (Exception)
					{
						embeds.Add(currentBuilder);

						currentBuilder = new EmbedBuilder()
							.CreateEmbed(EmojiEnum.Unknown, EmbedCallingType.Command)
							.AddField(field);
					}
				}
			}

			embeds.Add(currentBuilder);

			return embeds;
		}

	}
}
