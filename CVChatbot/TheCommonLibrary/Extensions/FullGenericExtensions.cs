using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class FullGenericExtensions
    {
        /// <summary>
        /// Returns true if the source object is contained within the list of arguments
        /// </summary>
        /// <typeparam name="TSoruce"></typeparam>
        /// <param name="source"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool In<TSoruce>(this TSoruce source, params TSoruce[] list)
        {
            return source.In(list.AsEnumerable());
        }

        /// <summary>
        /// Returns true if the source object is contained within the list of arguments
        /// </summary>
        /// <typeparam name="TSoruce"></typeparam>
        /// <param name="source"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool In<TSource>(this TSource source, IEnumerable<TSource> list)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return list.Contains(source);
        }

        /// <summary>
        /// Returns the value of the object or the default value for the type if the object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ValueOrTypeDefault<T>(this T source)
        {
            if (source == null)
                return default(T);

            return source;
        }
    }
}
