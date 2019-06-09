﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dapper;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// A SQL connection that can run queries, commands, and scripts.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    /// <typeparam name="TTransaction"></typeparam>
    public abstract class BaseDbscDbConnection<TConnection, TTransaction> : IDbscDbConnection
        where TConnection : IDbConnection
        where TTransaction : class, IDbTransaction
    {
        protected TConnection Connection { get; private set; }
        public int CommandTimeoutInSeconds { get; set; }

        private const int DefaultCommandTimeoutInSeconds = 15;

        protected BaseDbscDbConnection(TConnection connection)
        {
            Connection = connection;
            CommandTimeoutInSeconds = DefaultCommandTimeoutInSeconds;
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

            int realTimeoutInSeconds = timeoutInSeconds ?? CommandTimeoutInSeconds;

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

            int realTimeoutInSeconds = timeoutInSeconds ?? CommandTimeoutInSeconds;

            return Connection.Query<T>(sql, param: dapperParams, transaction: transaction, commandTimeout: realTimeoutInSeconds);
        }

        public virtual void Dispose()
        {
            Connection.Dispose();
        }
    }
}
