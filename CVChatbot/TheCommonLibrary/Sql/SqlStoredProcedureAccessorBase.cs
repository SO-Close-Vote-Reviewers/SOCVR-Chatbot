using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Sql
{
    public abstract class SqlStoredProcedureAccessorBase : SqlAccessorBase
    {
        public SqlStoredProcedureAccessorBase(string cs) : base(cs) { }

        protected override void ConfigureSqlCommand(System.Data.SqlClient.SqlCommand cmd, string value)
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = value;
        }
    }
}
