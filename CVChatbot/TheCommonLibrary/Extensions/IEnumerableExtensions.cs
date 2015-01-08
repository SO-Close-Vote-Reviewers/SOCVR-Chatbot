using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns a single random element from a List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        /// <summary>
        /// Helper function for PickRandom
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        /// <summary>
        /// Helper function for PickRandom
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        /// <summary>
        /// Builds a string of a CSV for a string, using a defined delimiter.
        /// Returns null if the source is null.
        /// Returns an empty string if the source contains no elements.
        /// Returns the only element if the source contains 1 element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="delimitor">The value to separate the elements with.</param>
        /// <returns></returns>
        public static string ToCSV<T>(this IEnumerable<T> source, string delimitor)
        {
            return source.ToCSV(delimitor, x => x.ToString());
        }

        /// <summary>
        /// Builds a string of a CSV for a string, using a defined delimiter.
        /// Returns null if the source is null.
        /// Returns an empty string if the source contains no elements.
        /// Returns the only element if the source contains 1 element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="delimitor">The value to separate the elements with.</param>
        /// <param name="toStringConversionAction">The function to convert each element in the source to a string.</param>
        /// <returns></returns>
        public static string ToCSV<T>(this IEnumerable<T> source, string delimitor, Func<T, string> toStringConversionAction)
        {
            if (source == null)
                return null;

            if (source.Count() == 0)
                return string.Empty;

            if (toStringConversionAction == null)
                throw new ArgumentNullException("Specified conversion action is null");

            if (source.Count() == 1)
                return toStringConversionAction(source.Single());

            if (delimitor == null)
                throw new ArgumentNullException("Delimiter value is null");

            return string.Join(delimitor, source.Select(x => toStringConversionAction(x)));
        }

        /// <summary>
        /// Creates a conditional Where statement for each item in the source. This is a replacement to an in-line ternary operator inside a Where clause.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="truePredicate">The predicate to preform if the condition is true.</param>
        /// <param name="falsePredicate">The predicate to preform if the condition is false.</param>
        /// <returns></returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, bool> condition, Func<T, bool> truePredicate, Func<T, bool> falsePredicate)
        {
            foreach (T item in source)
            {
                Func<T, bool> predicateToUse;

                if (condition(item))
                    predicateToUse = truePredicate;
                else
                    predicateToUse = falsePredicate;

                if (predicateToUse != null)
                {
                    if (predicateToUse(item))
                        yield return item;
                }
                else
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> SplitByPartSize<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .Where((x, i) => i % chunkSize == 0)
                .Select((x, i) => source.Skip(i * chunkSize)
                .Take(chunkSize));
        }

        public static IEnumerable<IEnumerable<T>> SplitByPartNumber<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }

        /// <summary>
        /// Returns the sequence or an empty sequence if the list is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list)
        {
            return list ?? Enumerable.Empty<T>();
        }
    }
}
