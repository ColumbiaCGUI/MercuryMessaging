using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MercuryMessaging.Support.Extensions
{
	public static class IEnumerableExtensions   {
		/// <summary>
		/// Orders input: alphanumeric.
		/// http://stackoverflow.com/questions/248603/natural-sort-order-in-c-sharp
		/// </summary>
		public static IEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector)
		{
			int max = source
				.SelectMany(i => Regex.Matches(selector(i), @"\d+([\.,]\d)").Cast<Match>().Select(m => (int?)m.Value.Length))
				.Max() ?? 0;

			return source.OrderBy(i => Regex.Replace(selector(i), @"\d+([\.,]\d)", m => m.Value.PadLeft(max, '0')));
		}
	}
}