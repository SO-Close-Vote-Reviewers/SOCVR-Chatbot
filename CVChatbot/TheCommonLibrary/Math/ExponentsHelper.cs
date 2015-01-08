using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Math
{
    public static class ExponentsHelper
    {
        /// <summary>
        /// Converts a string number input into a decimal. This will allow string using scientific notation, like "3.4E-1".
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static decimal ConvertFromString(string input)
        {
            return Decimal.Parse(input, System.Globalization.NumberStyles.Float);
        }

        public static bool TryConvertFromString(string input, out decimal output)
        {
            return Decimal.TryParse(input, System.Globalization.NumberStyles.Float, 
                CultureInfo.InvariantCulture, out output);
        }

        /// <summary>
        /// Creates a scientific notation string from the given decimal input. An example is "3.4E-1".
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertFromDecimal(decimal input)
        {
            return input.ToString("0.###E+0", CultureInfo.InvariantCulture);
        }
    }
}
