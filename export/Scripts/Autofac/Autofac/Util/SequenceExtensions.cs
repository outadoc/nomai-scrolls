using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Util
{
	internal static class SequenceExtensions
	{
		public static string JoinWith(this IEnumerable<string> elements, string separator)
		{
			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}
			if (separator == null)
			{
				throw new ArgumentNullException("separator");
			}
			return string.Join(separator, elements.ToArray());
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, T trailingItem)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}
			foreach (T item in sequence)
			{
				yield return item;
			}
			yield return trailingItem;
		}

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T leadingItem)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}
			yield return leadingItem;
			foreach (T item in sequence)
			{
				yield return item;
			}
		}

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				collection.Add(item);
			}
		}
	}
}
