using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerplexUmbraco.Forms.Code
{
    public static class Sql
    {
        /// <summary>
        /// Executes any SQL query as a sacaler query, returning only the first row of the first record.
        /// </summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="c">Explicit SQL connection to use</param>
        /// <param name="sql">The SQL query or stored procedure name</param>
        /// <param name="commandType">The SQL command type (generally speaking, Text or StoredProcedure)</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>The results of the scalar query (first row of the first record) of the resulting response</returns>
        public static string ExecuteSql(string sql, CommandType type = CommandType.Text, object parameters = null)
        {
            var db = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database;
            db.OpenSharedConnection();
            try
            {
                using (var command = db.CreateCommand(db.Connection, sql, parameters))
                {
                    command.CommandType = type;
                    var data = command.ExecuteScalar();
                    if (data != null)
                        return data.ToString();
                    else
                        return null;
                }
            }
            finally
            {
                db.CloseSharedConnection();
            }
        }

        /// <summary>
        /// Provides direct access to the data records returned by the SqlDataReader.
        /// This method returns an enumerator, so you should iterate over the results of this function in a foreach loop.
        /// Example ==> foreach (IDataRecord r in createSqlDataEnumerator("sPMyStoredProcedure, CommandType.StoredProcedure))
        /// </summary>
        /// <param name="c">Explicit SQL connection to use</param>
        /// <param name="sql">Either a stored procedure name or SQL query text</param>
        /// <param name="type">Stored procedure or text</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>The number of rows affeceted</returns>
        public static IEnumerable<IDataRecord> CreateSqlDataEnumerator(string sql, CommandType type, object parameters = null)
        {
            var db = Umbraco.Core.ApplicationContext.Current.DatabaseContext.Database;
            db.OpenSharedConnection();
            using (var command = db.CreateCommand(db.Connection, sql, parameters))
            {
                command.CommandType = type;
                var rdr = command.ExecuteReader();
                while (rdr.Read())
                    yield return rdr as IDataRecord;
            }
            db.CloseSharedConnection();
        }
    }
}
