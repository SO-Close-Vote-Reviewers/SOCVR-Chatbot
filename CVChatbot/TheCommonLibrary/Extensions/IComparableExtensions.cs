using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheCommonLibrary.Extensions
{
    public static class IComparableExtensions
    {
        /// <summary>
        /// Tells if the value is between the inclusive min and max bounds.
        /// </summary>
        /// <typeparam name="T">The data type of the comparison. Must implement <see cref="IComparable"/></typeparam>
        /// <param name="value"></param>
        /// <param name="min">The minimum inclusive bound.</param>
        /// <param name="max">The maximum inclusive bound.</param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T min, T max) 
            where T : IComparable<T>
        {
            return value.Between(min, max, true, true);
        }

        /// <summary>
        /// Tells if the value is between the min and max bounds. Min and Max can be inclusive or exclusive.
        /// </summary>
        /// <typeparam name="T">The data type of the comparison. Must implement <see cref="IComparable"/></typeparam>
        /// <param name="value"></param>
        /// <param name="min">The min bound.</param>
        /// <param name="max">The max bound.</param>
        /// <param name="minInclusive">Should the min bound be inclusive or exclusive?</param>
        /// <param name="maxInclusive">Should the max bound be inclusive or exclusive?</param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T min, T max, bool minInclusive, bool maxInclusive) 
            where T : IComparable<T>
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            int minCompare = min.CompareTo(value);
            int maxCompare = max.CompareTo(value);

            bool minCheck = minInclusive
                ? minCompare <= 0
                : minCompare == -1;

            bool maxCheck = maxInclusive
                ? maxCompare >= 0
                : maxCompare == 1;

            return minCheck && maxCheck;
        }
    }
}
