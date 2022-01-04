using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator.Extensions
{
    public static class EnumerableExtension
    {
        public static bool All<T>(this IEnumerable<T> collection, Func<T, int, bool> predicate)
        {
            var i = 0;
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                predicate(enumerator.Current, i);
                i++;
            }
            return true;
        }

        public static TReturn SelectFirstOrDefault<TValue, TReturn>(this IEnumerable<TValue> collection, Func<TValue, TReturn> predicate, IEqualityComparer<TReturn> comparer = null)
        {
            comparer ??= EqualityComparer<TReturn>.Default;
            foreach (var element in collection)
            {
                var result = predicate(element);
                if (!comparer.Equals(result, default))
                {
                    return result;
                }
            }

            return default;
        }

        public static TReturn SelectFirstOrDefault<TValue, TReturn>(this IEnumerable<TValue> collection, Func<TValue, int, TReturn> predicate, IEqualityComparer<TReturn> comparer = null)
        {
            comparer ??= EqualityComparer<TReturn>.Default;
            var i = 0;
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var result = predicate(enumerator.Current, i);
                if (!comparer.Equals(result, default))
                {
                    return result;
                }
                i++;
            }

            return default;
        }
    }
}
