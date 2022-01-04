using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            return dict[key] = new TValue();
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> defaultValueFunc)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            return dict[key] = defaultValueFunc();
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defalutValue = default)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            return defalutValue;
        }
    }
}
