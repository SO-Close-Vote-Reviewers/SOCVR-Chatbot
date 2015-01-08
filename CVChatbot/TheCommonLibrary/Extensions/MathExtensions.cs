using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    /// <summary>
    /// Extension methods using the "Math" class, not tied to a particular data type
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds a decimal value to the nearest integral value.
        /// </summary>
        /// <param name="source">A decimal number to be rounded.</param>
        /// <returns></returns>
        public static decimal RoundValue(this decimal source)
        {
            return System.Math.Round(source);
        }

        /// <summary>
        /// Rounds a decimal value to a specified number of fractional digits.
        /// </summary>
        /// <param name="source">A decimal number to be rounded.</param>
        /// <param name="decimals">The number of decimal places in the return value.</param>
        /// <returns></returns>
        public static decimal RoundValue(this decimal source, int decimals)
        {
            return System.Math.Round(source, decimals);
        }

        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional digits.
        /// </summary>
        /// <param name="source">A double-precision floating-point number to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns></returns>
        public static double RoundValue(this double source, int digits)
        {
            return System.Math.Round(source, digits);
        }
    }
}
