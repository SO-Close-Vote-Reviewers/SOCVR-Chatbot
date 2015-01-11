using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Sql
{
    public abstract class SqlScriptAccessorBase : SqlAccessorBase
    {
        public SqlScriptAccessorBase(string cs) : base(cs) { }

        /// <summary>
        /// Runs a script and uses the full dataset of what is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlScript"></param>
        /// <param name="getResults"></param>
        /// <returns></returns>
        protected T RunScript<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataSet, T> getResults)
        {
            using (DataSet ds = new DataSet())
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = trans;

                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.CommandText = sqlScript;

                                if (parametersAction != null)
                                    parametersAction(cmd.Parameters);

                                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                {
                                    da.Fill(ds);
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }

                if (getResults != null)
                    return getResults(ds);
                else
                    return default(T);
            }
        }

        protected async Task<T> RunScriptAsync<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataSet, T> getResults)
        {
            return await Task.Run(() => RunScript<T>(sqlScript, parametersAction, getResults));
        }

        /// <summary>
        /// Runs the script and gets the first table returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlScript"></param>
        /// <param name="getResults"></param>
        /// <returns></returns>
        protected T RunScript<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataTable, T> getResults)
        {
            return RunScript(sqlScript, parametersAction, new Func<DataSet, T>((ds) => getResults(ds.Tables[0])));
        }

        protected async Task<T> RunScriptAsync<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataTable, T> getResults)
        {
            return await Task.Run(() => RunScript<T>(sqlScript, parametersAction, getResults));
        }

        /// <summary>
        /// Runs the script and gets the first row in the first table returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlScript"></param>
        /// <param name="getResults"></param>
        /// <returns></returns>
        protected T RunScript<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataRow, T> getResults)
        {
            return RunScript(sqlScript, parametersAction, new Func<DataTable, T>((dt) => getResults(dt.Rows[0])));
        }

        protected async Task<T> RunScriptAsync<T>(string sqlScript, Action<SqlParameterCollection> parametersAction, Func<DataRow, T> getResults)
        {
            return await Task.Run(() => RunScript<T>(sqlScript, parametersAction, getResults));
        }

        /// <summary>
        /// Runs a script, and does not get the results
        /// </summary>
        /// <param name="sqlScript"></param>
        protected void RunScript(string sqlScript, Action<SqlParameterCollection> parametersAction)
        {
            RunScript(sqlScript, parametersAction, (Func<DataSet, object>)null);
        }

        protected async Task RunScriptAsync(string sqlScript, Action<SqlParameterCollection> parametersAction)
        {
            await Task.Run(() => RunScript(sqlScript, parametersAction, (Func<DataSet, object>)null));
        }

        protected override void ConfigureSqlCommand(SqlCommand cmd, string value)
        {
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = value;
        }
    }
}
