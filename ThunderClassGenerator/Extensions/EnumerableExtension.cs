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

        public static bool StartsWith<TValue>(this IEnumerable<TValue> collection, IEnumerable<TValue> other, IEqualityComparer<TValue> comparer = null)
        {
            comparer ??= EqualityComparer<TValue>.Default;
            var firstEnumerator = collection.GetEnumerator();
            var secondEnumerator = other.GetEnumerator();

            var firstMoved = firstEnumerator.MoveNext();
            var secondMoved = secondEnumerator.MoveNext();

            while (firstMoved && secondMoved)
            {
                if (!comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                {
                    return false;
                }

                firstMoved = firstEnumerator.MoveNext();
                secondMoved = secondEnumerator.MoveNext();
            }

            return firstMoved || (firstMoved == secondMoved);
        }

        public static IEnumerable<TValue> Insert<TValue>(this IEnumerable<TValue> collection, TValue item, int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            var i = 0;
            
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (i == index)
                {
                    yield return item;
                }
                yield return enumerator.Current;
                i++;
            }

            if (index == i)
            {
                yield return item;
            }
            else if (index > i)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public static IEnumerable<TValue> InsertRange<TValue>(this IEnumerable<TValue> collection, IEnumerable<TValue> other, int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            var i = 0;

            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (i == index)
                {
                    foreach (var item in other)
                    {
                        yield return item;
                    }
                }
                yield return enumerator.Current;
                i++;
            }

            if (index == i)
            {
                foreach (var item in other)
                {
                    yield return item;
                }
            }
            else if (index > i)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
