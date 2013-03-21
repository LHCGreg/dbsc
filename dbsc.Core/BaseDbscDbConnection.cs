using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dapper;

namespace dbsc.Core
{
    public abstract class BaseDbscDbConnection : IDbscDbConnection
    {
        protected IDbConnection BaseConnection { get; set; }

        protected BaseDbscDbConnection(IDbConnection baseConnection)
        {
            BaseConnection = baseConnection;
        }

        public abstract void ExecuteSqlScript(string sql);

        public void ExecuteSql(string sql)
        {
            ExecuteSql(sql, null);
        }

        public void ExecuteSql(string sql, IDictionary<string, object> sqlParams)
        {
            DynamicParameters dapperParams = null;
            if (sqlParams != null)
            {
                dapperParams = new DynamicParameters(sqlParams);
            }

            BaseConnection.Execute(sql, dapperParams);
        }

        public IEnumerable<T> Query<T>(string sql)
        {
            return Query<T>(sql, null);
        }

        public IEnumerable<T> Query<T>(string sql, IDictionary<string, object> sqlParams)
        {
            DynamicParameters dapperParams = null;
            if (sqlParams != null)
            {
                dapperParams = new DynamicParameters(sqlParams);
            }

            return BaseConnection.Query<T>(sql, dapperParams);
        }

        public abstract void Dispose();
    }
}
