using Humanizer;
using Humanizer.Localisation;

namespace DexterSlash.Extensions
{
    public static class StringExtensions
	{
		public static string HumanizeTimeSpan(this TimeSpan t)
		{
			return t.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
		}
	}
}
