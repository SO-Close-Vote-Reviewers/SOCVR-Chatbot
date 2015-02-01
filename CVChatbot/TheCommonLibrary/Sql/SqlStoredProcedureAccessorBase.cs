using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheCommonLibrary.Sql
{
    public abstract class SqlStoredProcedureAccessorBase : SqlAccessorBase
    {
        public SqlStoredProcedureAccessorBase(string cs) : base(cs) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected override void ConfigureSqlCommand(System.Data.SqlClient.SqlCommand cmd, string value)
        {
            cmd.CommandType = CommandType.StoredProcedure;   
            cmd.CommandText = value;
        }
    }
}
