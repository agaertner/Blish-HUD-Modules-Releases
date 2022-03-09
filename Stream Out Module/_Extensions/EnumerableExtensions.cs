using System;
using System.Collections.Generic;
using System.Linq;
namespace Nekres.Stream_Out
{
    public static class EnumerableExtensions
    {
        public static T MaxBy<T, U>(this IEnumerable<T> data, Func<T, U> f) where U : IComparable
        {
            return data.Aggregate((i1, i2) => f(i1).CompareTo(f(i2)) > 0 ? i1 : i2);
        }
        public static T MinBy<T, U>(this IEnumerable<T> data, Func<T, U> f) where U : IComparable
        {
            return data.MinBy(f, null);  //data.Aggregate((i1, i2) => f(i1).CompareTo(f(i2)) < 0 ? i1 : i2);
        }
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer ??= Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> data)
        {
            return data != null && data.Count() <= 0; // Any() fails silently for IEnumerable<dynamic> during runtime.
        }
    }
}
