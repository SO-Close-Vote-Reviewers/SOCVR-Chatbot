using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue outValue;
            if (dictionary.TryGetValue(key, out outValue))
            {
                return outValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public static TValue GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.GetValueOrDefault(key, null);
        }
    }
}
