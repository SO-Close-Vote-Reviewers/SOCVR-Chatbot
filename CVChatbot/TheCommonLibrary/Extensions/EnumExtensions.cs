using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Finds the attribute of the given type and returns the entire attribute, or null if the attribute cannot be found.
        /// Expects that that there is only one attribute of the given type on the enum value.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="enumeration">The enum value to pull the attribute from.</param>
        /// <returns></returns>
        public static TAttribute GetAttributeValue<TAttribute>(this Enum enumeration)
        where TAttribute : Attribute
        {
            return enumeration.GetAttributeValue<TAttribute, TAttribute>(x => x);
        }

        /// <summary>
        /// Finds the attribute of the given type and returns the value from the expressions, or null if the attribute cannot be found.
        /// Useful for only pulling a particular value from the attribute, instead of the entire attribute.
        /// Expects that there is only one attribute of the given type on the enum value.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TExpected">The return type from the expression. "What datatype would you like to return?"</typeparam>
        /// <param name="enumeration"></param>
        /// <param name="expression">A function that takes the attribute and returns a value from within.</param>
        /// <returns></returns>
        public static TExpected GetAttributeValue<TAttribute, TExpected>(this Enum enumeration, Func<TAttribute, TExpected> expression)
        {
            var attr = enumeration.GetAttributeValues(expression).SingleOrDefault();
            return attr.ValueOrTypeDefault();
        }

        /// <summary>
        /// Finds all attributes of the given type on the enum value and returns the value from the expressions argument foreach attribute found.
        /// Useful for only pulling a particular value from the attribute, instead of the entire attribute.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TExpected"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="expression">A function that takes the attribute and returns a value from within.</param>
        /// <returns></returns>
        public static IEnumerable<TExpected> GetAttributeValues<TAttribute, TExpected>(this Enum enumeration, Func<TAttribute, TExpected> expression)
        {
            var attributes = enumeration.GetType()
                .GetMember(enumeration.ToString())[0]
                .GetCustomAttributes(typeof(TAttribute), false)
                .Cast<TAttribute>();

            foreach (var attr in attributes)
            {
                yield return expression(attr);
            }
        }

        /// <summary>
        /// Finds all attributes of the given type on the enum value and returns each attribute found.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributeValues<TAttribute>(this Enum enumeration)
        {
            return enumeration.GetAttributeValues<TAttribute, TAttribute>(x => x);
        }
    }
}
