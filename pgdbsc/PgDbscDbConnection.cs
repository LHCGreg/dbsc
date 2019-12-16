using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;
using System.Globalization;
using System.IO;

namespace dbsc.Postgres
{
    class PgDbscDbConnection : BaseDbscDbConnection<NpgsqlConnection, NpgsqlTransaction>
    {
        public int ScriptTimeoutInSeconds { get; private set; }

        public PgDbscDbConnection(DbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo))
        {
            ScriptTimeoutInSeconds = connectionInfo.ScriptTimeoutInSeconds;
            CommandTimeoutInSeconds = connectionInfo.CommandTimeoutInSeconds;
            // It seems there is now no way to set a timeout on the COPY command so ImportTimeoutInSeconds is not used.
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

            builder.Username = connectionInfo.Username;

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
            try
            {
                Connection.Execute(sql, commandTimeout: ScriptTimeoutInSeconds);
            }
            catch (PostgresException ex)
            {
                // DbscExceptions show error message as is when caught at the top layer
                string message = NpgsqlExceptionToErrorMessage(ex);
                throw new DbscException(message, ex);
            }
            finally
            {
                Connection.Notice -= OnNotice;
            }
        }

        private static string NpgsqlExceptionToErrorMessage(PostgresException ex)
        {
            // DON'T THROW FROM HERE
            string lineNumberText = "";
            if (ex.Line != null)
            {
                lineNumberText = " on line " + ex.Line;
            }

            string detailText = "";
            if (!string.IsNullOrEmpty(ex.Detail))
            {
                detailText = ". " + ex.Detail;
            }

            string errorMessage = string.Format("Error{0}: {1}{2}", lineNumberText, ex.MessageText, detailText);
            return errorMessage;
        }

        private void OnNotice(object sender, NpgsqlNoticeEventArgs e)
        {
            if (!NoticeIsNoise(e))
            {
                Console.WriteLine("{0}: {1}", e.Notice.Severity, e.Notice.MessageText);
            }
        }

        private bool NoticeIsNoise(NpgsqlNoticeEventArgs e)
        {
            // Npgsql does not seem to actually return any sort of message number, so go by message text.
            return e.Notice.MessageText.Contains("CREATE TABLE will create implicit sequence")
                || e.Notice.MessageText.Contains("CREATE TABLE / PRIMARY KEY will create implicit index");
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
        public void ImportTable(PgDbscDbConnection sourceConn, PgTable table, string select)
        {
            string copyOutSql = string.Format("COPY ({0}) TO STDOUT WITH (FORMAT 'text', ENCODING 'utf-8')", select);
            string copyInSql = string.Format("COPY {0} FROM STDIN WITH (FORMAT 'text', ENCODING 'utf-8')", table);

            using (TextReader sourceReader = sourceConn.Connection.BeginTextExport(copyOutSql))
            using (TextWriter destinationWriter = this.Connection.BeginTextImport(copyInSql))
            {
                string line;
                while ((line = sourceReader.ReadLine()) != null)
                {
                    destinationWriter.Write(line);
                    destinationWriter.Write("\n");
                }
            }
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
