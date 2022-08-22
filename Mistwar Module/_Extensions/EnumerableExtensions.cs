using System;
using System.Collections.Generic;
using System.Linq;
namespace Nekres.Mistwar
{
    internal static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return !source?.Any() ?? true;
        }

        public static IEnumerable<ValueTuple<T, T>> By2<T>(this IEnumerable<T> source)
        {
            return source.Zip(source.Skip(1));
        }

        public static IEnumerable<ValueTuple<TA, TB>> Zip<TA, TB>(this IEnumerable<TA> source, IEnumerable<TB> other)
        {
            return source.Zip(other, (TA a, TB b) => new ValueTuple<TA, TB>(a, b));
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return source.MostBy(selector, comparer, true);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return source.MostBy(selector, comparer, false);
        }

        private static TSource MostBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, bool max)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            comparer ??= Comparer<TKey>.Default;
            var factor = max ? -1 : 1;
            using var sourceIterator = source.GetEnumerator();
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var most = sourceIterator.Current;
            var mostKey = selector(most);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, mostKey) * factor >= 0) continue;
                most = candidate;
                mostKey = candidateProjected;
            }
            var result = most;
            return result;
        }

        public static T GetItem<T>(this List<T> array, int index)
        {
            if (index >= array.Count)
            {
                return array[index % array.Count];
            }
            else if (index < 0)
            {
                return array[index % array.Count + array.Count];
            }
            else
            {
                return array[index];
            }
        }
    }
}