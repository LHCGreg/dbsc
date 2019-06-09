using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// A SQL connection that can run queries, commands, and scripts.
    /// </summary>
    public interface IDbscDbConnection : IDisposable
    {
        /// <summary>
        /// Executes a user-provided script - an upgrade script or a DB creation script.
        /// </summary>
        /// <param name="sql"></param>
        void ExecuteSqlScript(string sql);

        /// <summary>
        /// Executes SQL requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParams"></param>
        void ExecuteSql(string sql, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null);

        /// <summary>
        /// Runs a query requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null);
    }
}
