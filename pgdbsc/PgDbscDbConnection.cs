using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Dapper;
using dbsc.Core;

namespace dbsc.Postgres
{
    class PgDbscDbConnection : IDbscDbConnection
    {
        public NpgsqlConnection Connection { get; private set; }
        
        public PgDbscDbConnection(string connectionString)
        {
            Connection = new NpgsqlConnection(connectionString);
            Connection.Open();
        }
        
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

            Connection.Notice += OnNotice;
            Connection.Notification += OnNotification;
            try
            {
                Connection.Execute(sql, dapperParams);
            }
            finally
            {
                Connection.Notice -= OnNotice;
                Connection.Notification -= OnNotification;
            }
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

            Connection.Notice += OnNotice;
            Connection.Notification += OnNotification;
            try
            {
                return Connection.Query<T>(sql, dapperParams);
            }
            finally
            {
                Connection.Notice -= OnNotice;
                Connection.Notification -= OnNotification;
            }
        }

        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            Console.WriteLine("NOTIFICATION: {0}", e.Condition);
        }

        private void OnNotice(object sender, NpgsqlNoticeEventArgs e)
        {
            Console.WriteLine("{0}: {1}", e.Notice.Severity, e.Notice.Message);
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
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