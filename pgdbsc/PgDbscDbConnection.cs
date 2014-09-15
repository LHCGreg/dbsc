using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    class PgDbscDbConnection : BaseDbscDbConnection<NpgsqlConnection, NpgsqlTransaction>
    {
        public int ScriptTimeoutInSeconds { get; private set; }
        public int ImportTableTimeoutInSeconds { get; private set; }
        
        public PgDbscDbConnection(DbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo))
        {
            ScriptTimeoutInSeconds = connectionInfo.ScriptTimeoutInSeconds;
            CommandTimeoutInSeconds = connectionInfo.CommandTimeoutInSeconds;
            ImportTableTimeoutInSeconds = connectionInfo.ImportTableTimeoutInSeconds;
        }

        private static NpgsqlConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            string connectionString = GetConnectionString(connectionInfo);
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static string GetConnectionString(DbConnectionInfo connectionInfo)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.ApplicationName = "pgdbsc";

            // Don't use connection pooling because scripts could potentially change options with SET and expect later
            // scripts to have a clean environment.
            builder.Pooling = false;
            builder.Database = connectionInfo.Database;
            builder.Host = connectionInfo.Server;
            builder.Timeout = connectionInfo.ConnectTimeoutInSeconds;

            if (connectionInfo.Port != null)
            {
                builder.Port = connectionInfo.Port.Value;
            }

            // Do not set this to true! Imports get weird threading issues if you do - probably a bug in Npgsql
            builder.SyncNotification = false;

            builder.UserName = connectionInfo.Username;

            if (connectionInfo.UseIntegratedSecurity)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.Password = connectionInfo.Password;
            }

            string connectionString = builder.ToString();
            return connectionString;
        }
        
        public override void ExecuteSqlScript(string sql)
        {
            Connection.Notice += OnNotice;
            Connection.Notification += OnNotification; // When is this fired?
            try
            {
                Connection.Execute(sql, commandTimeout: ScriptTimeoutInSeconds);
            }
            catch (NpgsqlException ex)
            {
                // DbscExceptions show error message as is when caught at the top layer
                if (!string.IsNullOrEmpty(ex.Detail))
                {
                    throw new DbscException(string.Format("Error: {0}. {1}", ex.BaseMessage, ex.Detail), ex);
                }
                else
                {
                    throw new DbscException(string.Format("Error: {0}", ex.BaseMessage), ex);
                }
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
            if (!NoticeIsNoise(e))
            {
                Console.WriteLine("{0}: {1}", e.Notice.Severity, e.Notice.Message);
            }
        }

        private bool NoticeIsNoise(NpgsqlNoticeEventArgs e)
        {
            // Npgsql does not seem to actually return any sort of message number, so go by message text.
            return e.Notice.Message.Contains("CREATE TABLE will create implicit sequence")
                || e.Notice.Message.Contains("CREATE TABLE / PRIMARY KEY will create implicit index");
        }

        public NpgsqlTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        private class Table
        {
            public string table_schema { get; set; }
            public string table_name { get; set; }
        }

        public ICollection<PgTable> GetTablesExceptMetadata()
        {
            string sql = @"SELECT table_schema, table_name FROM information_schema.tables
WHERE table_schema NOT LIKE 'pg_%' AND table_schema <> 'information_schema'
AND table_type = 'BASE TABLE'
AND table_name <> 'dbsc_metadata'";

            List<PgTable> tables = Query<Table>(sql).Select(t => new PgTable(t.table_schema, t.table_name)).ToList();
            return tables;
        }

        public static string QuotePgIdentifier(string identifier)
        {
            // Replace quotes with quotequote and enclose in quotes
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        public static string QuotePgIdentifier(string schema, string identifier)
        {
            return QuotePgIdentifier(schema) + "." + QuotePgIdentifier(identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceConn"></param>
        /// <param name="sourceDbTransaction">Required.</param>
        /// <param name="targetDbTransaction">Required.</param>
        public void ImportTable(PgDbscDbConnection sourceConn, PgTable table, string select, NpgsqlTransaction targetDbTransaction, NpgsqlTransaction sourceDbTransaction)
        {
            string copyOutSql = string.Format("COPY ({0}) TO STDOUT WITH (FORMAT 'text', ENCODING 'utf-8')", select);
            NpgsqlCommand copyOutCommand = new NpgsqlCommand(copyOutSql, sourceConn.Connection, sourceDbTransaction);
            copyOutCommand.CommandTimeout = ImportTableTimeoutInSeconds;
            NpgsqlCopyOut source = new NpgsqlCopyOut(copyOutCommand, sourceConn.Connection);
            source.Start();

            NpgsqlCommand targetCmd = new NpgsqlCommand();
            targetCmd.CommandText = string.Format("COPY {0} FROM STDIN WITH (FORMAT 'text', ENCODING 'utf-8')", table);
            targetCmd.Connection = this.Connection;
            targetCmd.Transaction = targetDbTransaction;
            targetCmd.CommandTimeout = ImportTableTimeoutInSeconds;

            NpgsqlCopyIn target = new NpgsqlCopyIn(targetCmd, this.Connection);
            target.Start();

            source.CopyStream.CopyTo(target.CopyStream);

            target.End();
            source.End();
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