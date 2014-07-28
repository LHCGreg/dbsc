using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.SqlServer
{
    class MsDbscDbConnection : BaseDbscDbConnection<SqlConnection, SqlTransaction>
    {
        public int ScriptTimeoutInSeconds { get; private set; }
        public int ImportTableTimeoutInSeconds { get; private set; }
        
        public MsDbscDbConnection(SqlServerConnectionSettings connectionInfo)
            : base(OpenConnection(connectionInfo))
        {
            ScriptTimeoutInSeconds = connectionInfo.ScriptTimeoutInSeconds;
            CommandTimeoutInSeconds = connectionInfo.CommandTimeoutInSeconds;
            ImportTableTimeoutInSeconds = connectionInfo.ImportTableTimeoutInSeconds;
        }

        private static SqlConnection OpenConnection(SqlServerConnectionSettings connectionInfo)
        {
            string connectionString = GetConnectionString(connectionInfo);
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static string GetConnectionString(SqlServerConnectionSettings connectionInfo)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ApplicationName = "msdbsc";
            builder.DataSource = connectionInfo.Server;
            builder.InitialCatalog = connectionInfo.Database;
            builder.ConnectTimeout = connectionInfo.ConnectTimeoutInSeconds;

            // No need to disable connection pooling in order to isolate scripts from each other.
            // The driver resets the connection settings when taking a connection from the pool.
            //builder.Pooling = false;

            if (connectionInfo.UseIntegratedSecurity)
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
            server.ConnectionContext.StatementTimeout = ScriptTimeoutInSeconds;
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
        /// <param name="table"></param>
        /// <param name="importSelect">SQL to use on the source database to get data.</param>
        /// <param name="sourceDbTransaction">Source database transaction to use. If null, don't use a transaction on the source database.</param>
        public void ImportTable(MsDbscDbConnection sourceConn, SqlServerTable table, string importSelect, SqlTransaction sourceDbTransaction = null)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock, externalTransaction: null);
            bulkCopy.BulkCopyTimeout = ImportTableTimeoutInSeconds;
            bulkCopy.DestinationTableName = table.ToString();

            SqlCommand importQuery;
            if (sourceDbTransaction != null)
            {
                importQuery = new SqlCommand(importSelect, sourceConn.Connection, sourceDbTransaction);
            }
            else
            {
                importQuery = new SqlCommand(importSelect, sourceConn.Connection);
            }
            importQuery.CommandTimeout = ImportTableTimeoutInSeconds;

            using (importQuery)
            {
                using (SqlDataReader reader = importQuery.ExecuteReader())
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
        }

        public static string QuoteSqlServerIdentifier(string identifier)
        {
            // Replace ] with ]] and enclose in []
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        public static string QuoteSqlServerIdentifier(string schema, string identifier)
        {
            return QuoteSqlServerIdentifier(schema) + "." + QuoteSqlServerIdentifier(identifier);
        }
    }
}

/*
 Copyright 2014 Greg Najda

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