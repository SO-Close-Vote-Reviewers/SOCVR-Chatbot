using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Sql
{
    public abstract class SqlAccessorBase
    {
        public string ConnectionString { get; private set; }

        public SqlAccessorBase(string cs)
        {
            if (string.IsNullOrWhiteSpace(cs))
                throw new ArgumentNullException("cs", "Connection string null or empty");

            ConnectionString = cs;
        }

        protected abstract void ConfigureSqlCommand(SqlCommand cmd, string value);

        /// <summary>
        /// Runs a script and uses the full dataset of what is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlScript"></param>
        /// <param name="getResults"></param>
        /// <returns></returns>
        protected T RunCommand<T>(string value, Func<DataSet, T> getResults)
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

                                ConfigureSqlCommand(cmd, value);

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

        /// <summary>
        /// Runs a script and uses the full dataset of what is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlScript"></param>
        /// <param name="getResults"></param>
        /// <returns></returns>
        protected async Task<T> RunCommandAsync<T>(string value, Func<DataSet, T> getResults)
        {
            return await Task.Run(() => RunCommand<T>(value, getResults));
        }
    }
}
