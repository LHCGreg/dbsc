using dbsc.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Oracle
{
    class OracleDbscDbConnection : BaseDbscDbConnection<OracleConnection, OracleTransaction>
    {
        public OracleDbscDbConnection(OracleDbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo), connectionInfo)
        {
            ;
        }

        private static OracleConnection OpenConnection(OracleDbConnectionInfo connectionInfo)
        {
            string connectionString = GetConnectionString(connectionInfo);
            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static string GetConnectionString(OracleDbConnectionInfo connectionInfo)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
            builder.ConnectionTimeout = connectionInfo.ConnectTimeoutInSeconds;
            builder.Pooling = false; // don't enable unless sure that connections can't get settings from the previous connection
            
            // Only allow integrated security on windows
            if (connectionInfo.Username == null)
            {
                builder.UserID = "/";
            }
            else
            {
                builder.UserID = connectionInfo.Username;
                builder.Password = connectionInfo.Password;
            }

            const int defaultPort = 1521;
            int port = connectionInfo.Port ?? defaultPort;

            string dataSource = string.Format("(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {2})))",
                connectionInfo.Server, port, connectionInfo.ServiceName);

            string connectionString = builder.ToString();
            return connectionString;
        }

        public override void ExecuteSqlScript(string sql)
        {
            throw new NotImplementedException();
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