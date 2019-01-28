using System;
using System.Collections.Generic;

namespace MiddlewareSharp.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> ReplaceFirst<T>(this IEnumerable<T> source, T old, T @new)
		{
			var replaced = false;
			foreach (var obj in source)
			{
				if (!replaced && obj.Equals(old))
				{
					replaced = true;
					yield return @new;
				}
				else
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<T> ReplaceFirst<T>(this IEnumerable<T> source, T old, T @new, IEqualityComparer<T> comparer)
		{
			var replaced = false;
			foreach (var obj in source)
			{
				if (!replaced && comparer.Equals(old, obj))
				{
					replaced = true;
					yield return @new;
				}
				else
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<T> ReplaceFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, T @new)
		{
			var replaced = false;
			foreach (var obj in source)
			{
				if (!replaced && predicate(obj))
				{
					replaced = true;
					yield return @new;
				}
				else
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> source, T old)
		{
			var removed = false;
			foreach (var obj in source)
			{
				if (!removed && obj.Equals(old))
				{
					removed = true;
				}
				else
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> source, T old, IEqualityComparer<T> comparer)
		{
			var removed = false;
			foreach (var obj in source)
			{
				if (!removed && comparer.Equals(old, obj))
				{
					removed = true;
				}
				else
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			var removed = false;
			foreach (var obj in source)
			{
				if (!removed && predicate(obj))
				{
					removed = true;
				}
				else
				{
					yield return obj;
				}
			}
		}
	}
}
