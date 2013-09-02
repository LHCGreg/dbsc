using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using dbsc.Core;
using Dapper;
using System.Data;

namespace dbsc.SqlServer
{
    class MsDbscDbConnection : BaseDbscDbConnection<SqlConnection, SqlTransaction>
    {
        public MsDbscDbConnection(DbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo), connectionInfo)
        {
            ;
        }

        private static SqlConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            string connectionString = GetConnectionString(connectionInfo);
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static string GetConnectionString(DbConnectionInfo connectionInfo)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ApplicationName = "msdbsc";
            builder.DataSource = connectionInfo.Server;
            builder.InitialCatalog = connectionInfo.Database;
            builder.ConnectTimeout = connectionInfo.ConnectTimeoutInSeconds;

            // No need to disable connection pooling in order to isolate scripts from each other.
            // The driver resets the connection settings when taking a connection from the pool.
            //builder.Pooling = false;

            if (connectionInfo.Username == null)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = connectionInfo.Username;
                builder.Password = connectionInfo.Password;
            }

            builder.MultipleActiveResultSets = false;

            string connectionString = builder.ToString();
            return connectionString;
        }

        public override void ExecuteSqlScript(string sql)
        {
            ServerConnection serverConnection = new ServerConnection(Connection);
            Server server = new Server(serverConnection);
            server.ConnectionContext.ServerMessage += OnServerMessage;
            server.ConnectionContext.StatementTimeout = ConnectionInfo.ScriptTimeoutInSeconds;
            try
            {
                server.ConnectionContext.ExecuteNonQuery(sql);
            }
            finally
            {
                // Need to unsubscribe because subscribing does something to the actual underlying connection
                server.ConnectionContext.ServerMessage -= OnServerMessage;
            }
        }

        private void OnServerMessage(object sender, ServerMessageEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Error.Procedure))
            {
                Console.WriteLine("{0}:{1}: {2}", e.Error.Procedure, e.Error.LineNumber, e.Error.Message);
            }
            else
            {
                Console.WriteLine("Line {0}: {1}", e.Error.LineNumber, e.Error.Message);
            }
        }

        public SqlTransaction BeginSnapshotTransaction()
        {
            return Connection.BeginTransaction(IsolationLevel.Snapshot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceConn"></param>
        /// <param name="table">Table name already with brackets and schema-qualified if needed</param>
        /// <param name="sourceDbTransaction">Source database transaction to use. If null, don't use a transaction on the source database.</param>
        public void ImportTable(MsDbscDbConnection sourceConn, string table, SqlTransaction sourceDbTransaction = null)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock, externalTransaction: null);
            bulkCopy.BulkCopyTimeout = ConnectionInfo.ImportTableTimeoutInSeconds;
            bulkCopy.DestinationTableName = table;

            string importSql = string.Format("SELECT * FROM {0}", table);
            SqlCommand importQuery;
            if (sourceDbTransaction != null)
            {
                importQuery = new SqlCommand(importSql, sourceConn.Connection, sourceDbTransaction);
            }
            else
            {
                importQuery = new SqlCommand(importSql, sourceConn.Connection);
            }
            importQuery.CommandTimeout = ConnectionInfo.ImportTableTimeoutInSeconds;

            using (importQuery)
            {
                using (SqlDataReader reader = importQuery.ExecuteReader())
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
        }

        public override void Dispose()
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