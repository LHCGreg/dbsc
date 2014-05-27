using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;
using System.Diagnostics;

namespace dbsc.Oracle
{
    class OraDbscDbConnection : BaseDbscDbConnection<OracleConnection, OracleTransaction>
    {
        public int ScriptTimeoutInSeconds { get; private set; }
        public int ImportTableTimeoutInSeconds { get; private set; }

        private DbConnectionInfo ConnectionSettings { get; set; }
        
        public OraDbscDbConnection(DbConnectionInfo connectionSettings)
            : base(OpenConnection(connectionSettings))
        {
            ScriptTimeoutInSeconds = connectionSettings.ScriptTimeoutInSeconds;
            ImportTableTimeoutInSeconds = connectionSettings.ImportTableTimeoutInSeconds;
            ConnectionSettings = connectionSettings;
        }

        private static OracleConnection OpenConnection(DbConnectionInfo connectionSettings)
        {
            string connectionString = GetConnectionString(connectionSettings);
            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static int GetPort(DbConnectionInfo connectionSettings)
        {
            const int defaultPort = 1521;
            return connectionSettings.Port ?? defaultPort;
        }

        private static string GetConnectionString(DbConnectionInfo connectionSettings)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
            builder.ConnectionTimeout = connectionSettings.ConnectTimeoutInSeconds;
            // Normally dbsc shouldn't get pooled connections to avoid keeping settings set in scripts
            // from one connection to the next, but sqlplus will be running the scripts so it's ok.
            builder.Pooling = true;
            
            builder.UserID = connectionSettings.Username;
            builder.Password = connectionSettings.Password;

            int port = GetPort(connectionSettings);

            builder.DataSource = GetDataSourceString(connectionSettings.Server, port, connectionSettings.Database);
            builder.ValidateConnection = true;

            string connectionString = builder.ToString();
            return connectionString;
        }

        internal static string GetDataSourceString(string host, int port, string serviceName)
        {
            return string.Format("(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {2})))",
                host, port, serviceName);
        }

        private string GetSqlplusArgs(string sqlScriptPath)
        {
            string loginArg = string.Format("{0}/{1}@{2}:{3}/{4}", ConnectionSettings.Username, ConnectionSettings.Password, ConnectionSettings.Server, GetPort(ConnectionSettings), ConnectionSettings.Database);
            string args = loginArg.QuoteCommandLineArg();
            return args;
        }

        // Need to use the sqlplus command line program to run scripts because you can't run multiple DDL statements
        // in one command normally.
        public void ExecuteSqlScriptFromFile(string sqlScriptPath)
        {
            string sqlplusArgs = GetSqlplusArgs(sqlScriptPath);
            Process sqlplus = new Process()
            {
                StartInfo = new ProcessStartInfo("sqlplus", sqlplusArgs)
                {
                    ErrorDialog = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };

            object consoleLock = new object();
            sqlplus.OutputDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
            sqlplus.ErrorDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
            using (sqlplus)
            {
                sqlplus.Start();
                sqlplus.BeginOutputReadLine();
                sqlplus.BeginErrorReadLine();
                
                // sqlplus is not designed for programmatically running scripts, so need to make it stop and return an error code
                // if there's an error, and tell it to exist after running the script instead of waiting for more input.
                sqlplus.StandardInput.WriteLine("WHENEVER SQLERROR EXIT SQL.SQLCODE");
                sqlplus.StandardInput.WriteLine("WHENEVER OSERROR EXIT");
                sqlplus.StandardInput.WriteLine("START " + sqlScriptPath);
                sqlplus.StandardInput.WriteLine("exit");
                sqlplus.WaitForExit();
                if (sqlplus.ExitCode != 0)
                {
                    throw new DbscException(string.Format("Error running SQL script {0}. Check sqlplus output above for details.", sqlScriptPath));
                }
            }
        }

        /// <summary>
        /// Not implemented, use ExecuteSqlScriptFromFile instead.
        /// </summary>
        /// <param name="sql"></param>
        public override void ExecuteSqlScript(string sql)
        {
            throw new NotImplementedException("Internal error: ExecuteSqlScriptFromFile should be used instead.");
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