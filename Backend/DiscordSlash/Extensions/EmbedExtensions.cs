using Dexter.Enums;
using Discord;
using System.Diagnostics;
using System.Reflection;

namespace DexterSlash.Extensions
{
    public static class EmbedExtensions
	{
		/// <summary>
		/// Builds an embed with the attributes specified by the emoji enum.
		/// </summary>
		/// <param name="embedBuilder">The EmbedBuilder which you wish to be built upon.</param>
		/// <param name="thumbnails">The type of EmbedBuilder you wish it to be, specified by an enum of possibilities.</param>
		/// <param name="botConfiguration">The BotConfiguration which is used to find the thumbnail of the embed.</param>
		/// <param name="calledType">The EmbedCallingType that the embed was called to be made from.</param>
		/// <returns>The built embed, with the thumbnail and color applied.</returns>

		public static EmbedBuilder CreateEmbed(this EmbedBuilder embedBuilder, EmojiEnum thumbnails, EmbedCallingType calledType)
		{
			Color Color = thumbnails switch
			{
				EmojiEnum.Annoyed => Color.Red,
				EmojiEnum.Love => Color.Green,
				EmojiEnum.Sign => Color.Blue,
				EmojiEnum.Wut => Color.Teal,
				EmojiEnum.Unknown => Color.Magenta,
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


		/// <summary>
		/// Gets the class of the last method that had been called.
		/// </summary>
		/// <param name="searchHeight">The height backwards that you would like to see the call come from.</param>
		/// <returns>The last called class + method</returns>

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
				methodName = methodName.Substring(0, Index).Replace("<", "");

			return new KeyValuePair<string, string>(name, methodName);
		}

	}
}
