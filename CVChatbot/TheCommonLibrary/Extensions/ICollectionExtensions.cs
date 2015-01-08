using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class ICollectionExtensions
    {
        /// <summary>
        /// Adds the param values to the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="values"></param>
        public static void AddRange<T>(this ICollection<T> list, params T[] values)
        {
            foreach (T item in values)
                list.Add(item);
        }

    }
}
