using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns the value converted to a string or an empty string if the value is null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringOrEmpty(this object value)
        {
            return value.ToStringOrDefault("");
        }

        /// <summary>
        /// If an object is null, returns null.  Otherwise, returns o.ToString().
        /// </summary>
        public static string ToStringOrNull(this object o)
        {
            return o.ToStringOrDefault(null);
        }

        /// <summary>
        /// Returns the value converted to a string or the given default value if the value is null.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ToStringOrDefault(this object value, string defaultValue)
        {
            if (value == null)
                return defaultValue;

            return value.ToString();
        }

        /// <summary>
        /// Gets an inner value from the object, or the default value if the object is null. Best used if the selector points to an internal property.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="value"></param>
        /// <param name="selector"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TReturn InnerValueOrDefault<TValue, TReturn>(this TValue value, Func<TValue, TReturn> selector, TReturn defaultValue)
            where TValue : class
        {
            if (value == null)
                return defaultValue;

            return selector(value);
        }

        public static TReturn InnerValueOrNull<TValue, TReturn>(this TValue value, Func<TValue, TReturn> selector)
            where TValue : class
            where TReturn : class
        {
            return value.InnerValueOrDefault<TValue, TReturn>(selector, null);
        }

        /// <summary>
        /// Throws a NullReferenceException if the source object is null.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="variableName"></param>
        public static void ThrowIfNull(this object source, string variableName)
        {
            if (source == null)
                throw new NullReferenceException(variableName);
        }

        /// <summary>
        /// Throws an exception of the given type if the source object is null.
        /// </summary>
        /// <typeparam name="TExecption"></typeparam>
        /// <param name="source"></param>
        public static void ThrowIfNull<TExecption>(this object source)
            where TExecption : Exception, new()
        {
            if (source == null)
                throw new TExecption();
        }

        /// <summary>
        /// Returns true if the source object is null.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNull(this object source)
        {
            return source == null;
        }

        /// <summary>
        /// Makes a copy from the object.
        /// Doesn't copy the reference memory, only data.
        /// </summary>
        /// <typeparam name="T">Type of the return object.</typeparam>
        /// <param name="item">Object to be copied.</param>
        /// <returns>Returns the copied object.</returns>
        public static T Clone<T>(this T item)
        {
            if (item != null)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                formatter.Serialize(stream, item);
                stream.Seek(0, SeekOrigin.Begin);

                T result = (T)formatter.Deserialize(stream);

                stream.Close();

                return result;
            }
            else
                return default(T);
        }
    }
}
