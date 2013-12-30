using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dapper;

namespace dbsc.Core
{
    public abstract class BaseDbscDbConnection<TConnection, TTransaction> : IDbscDbConnection
        where TConnection : IDbConnection
        where TTransaction : class, IDbTransaction
    {
        protected TConnection Connection { get; set; }
        public DbConnectionInfo ConnectionInfo { get; private set; }

        protected BaseDbscDbConnection(TConnection connection, DbConnectionInfo connectionInfo)
        {
            Connection = connection;
            ConnectionInfo = connectionInfo;
        }

        public abstract void ExecuteSqlScript(string sql);

        public void ExecuteSql(string sql, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null)
        {
            ExecuteSql(sql, transaction: null, timeoutInSeconds: timeoutInSeconds, sqlParams: sqlParams);
        }

        public void ExecuteSql(string sql, TTransaction transaction, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null)
        {
            DynamicParameters dapperParams = null;
            if (sqlParams != null)
            {
                dapperParams = new DynamicParameters(sqlParams);
            }

            int realTimeoutInSeconds = timeoutInSeconds ?? ConnectionInfo.CommandTimeoutInSeconds;

            Connection.Execute(sql, param: dapperParams, transaction: transaction, commandTimeout: realTimeoutInSeconds);
        }

        public IEnumerable<T> Query<T>(string sql, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null)
        {
            return Query<T>(sql, transaction: null, timeoutInSeconds: timeoutInSeconds, sqlParams: sqlParams);
        }

        public IEnumerable<T> Query<T>(string sql, TTransaction transaction, IDictionary<string, object> sqlParams = null, int? timeoutInSeconds = null)
        {
            DynamicParameters dapperParams = null;
            if (sqlParams != null)
            {
                dapperParams = new DynamicParameters(sqlParams);
            }

            int realTimeoutInSeconds = timeoutInSeconds ?? ConnectionInfo.CommandTimeoutInSeconds;

            return Connection.Query<T>(sql, param: dapperParams, transaction: transaction, commandTimeout: realTimeoutInSeconds);
        }

        public abstract void Dispose();
    }
}

/*
 Copyright 2013 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/