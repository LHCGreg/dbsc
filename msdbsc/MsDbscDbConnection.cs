using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using dbsc.Core;
using Dapper;

namespace dbsc.SqlServer
{
    class MsDbscDbConnection : BaseDbscDbConnection
    {
        public SqlConnection Connection { get; private set; }

        public MsDbscDbConnection(string connectionString)
            : base(OpenConnection(connectionString))
        {
            Connection = (SqlConnection)BaseConnection;
        }

        private static SqlConnection OpenConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public override void ExecuteSqlScript(string sql)
        {
            ServerConnection serverConnection = new ServerConnection(Connection);
            Server server = new Server(serverConnection);
            server.ConnectionContext.InfoMessage += OnInfoMessage;
            server.ConnectionContext.ServerMessage += OnServerMessage;
            try
            {
                server.ConnectionContext.ExecuteNonQuery(sql);
            }
            finally
            {
                // Need to unsubscribe because subscribing does something to the actual underlying connection
                server.ConnectionContext.ServerMessage -= OnServerMessage;
                server.ConnectionContext.InfoMessage -= OnInfoMessage;
            }
        }

        private void OnServerMessage(object sender, ServerMessageEventArgs e)
        {
            Console.WriteLine("Server message");
            Console.WriteLine("{0} {1}: {2}", e.Error.Procedure, e.Error.LineNumber, e.Error.Message);
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Console.WriteLine("Info message");
            foreach (SqlError message in e.Errors)
            {
                Console.WriteLine("{0} {1}: {2}", message.Procedure, message.LineNumber, message.Message);
            }
            Console.WriteLine(e.Message);
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