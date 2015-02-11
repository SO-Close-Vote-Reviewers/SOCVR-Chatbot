﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    /// <summary>
    /// Extensions for Sql related objects.
    /// </summary>
    public static class SqlExtensions
    {
        /// <summary>
        /// Adds a parameter to the parameter collection with it's accompanying value.
        /// If the value passed in is null it will be converted to DBNull.Value.
        /// </summary>
        /// <param name="collection">The collection to add the parameter to.</param>
        /// <param name="parameterName">The name of the parameter. Like "@ParamName".</param>
        /// <param name="value">The value of the parameter.</param>
        public static void AddParam(this SqlParameterCollection collection, string parameterName, object value)
        {
            if (value == null)
            {
                collection.AddWithValue(parameterName, DBNull.Value);
            }
            else
            {
                collection.AddWithValue(parameterName, value);
            }
        }
    }
}