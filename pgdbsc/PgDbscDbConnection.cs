using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Dapper;
using dbsc.Core;

namespace dbsc.Postgres
{
    class PgDbscDbConnection : BaseDbscDbConnection
    {
        public NpgsqlConnection Connection { get; private set; }
        
        public PgDbscDbConnection(string connectionString)
            : base(OpenConnection(connectionString))
        {
            Connection = (NpgsqlConnection)BaseConnection;
        }

        private static NpgsqlConnection OpenConnection(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return conn;
        }
        
        public override void ExecuteSqlScript(string sql)
        {
            Connection.Notice += OnNotice;
            Connection.Notification += OnNotification;
            try
            {
                Connection.Execute(sql);
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