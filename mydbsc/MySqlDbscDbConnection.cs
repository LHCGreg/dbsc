using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Dapper;
using dbsc.Core;

namespace dbsc.MySql
{
    class MySqlDbscDbConnection : BaseDbscDbConnection<MySqlConnection, MySqlTransaction>
    {
        public MySqlDbscDbConnection(DbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo), connectionInfo)
        {
            ;
        }

        private static MySqlConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            string connectionString = GetConnectionString(connectionInfo);
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        private static string GetConnectionString(DbConnectionInfo connectionInfo)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.AllowBatch = true;
            builder.AllowZeroDateTime = true;
            builder.ConnectionTimeout = (uint)connectionInfo.ConnectTimeoutInSeconds;
            builder.Database = connectionInfo.Database;
            builder.Pooling = false;
            builder.AllowUserVariables = true;

            if (connectionInfo.Username == null)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = connectionInfo.Username;
                builder.Password = connectionInfo.Password;
            }

            builder.Server = connectionInfo.Server;
            if (connectionInfo.Port != null)
            {
                builder.Port = (uint)connectionInfo.Port.Value;
            }

            string connString = builder.ToString();
            return connString;
        }

        public override void ExecuteSqlScript(string sql)
        {
            Connection.InfoMessage += OnInfoMessage;
            try
            {
                Connection.Execute(sql);
            }
            finally
            {
                Connection.InfoMessage -= OnInfoMessage;
            }
        }

        private void OnInfoMessage(object sender, MySqlInfoMessageEventArgs args)
        {
            foreach (MySqlError message in args.errors)
            {
                Console.WriteLine("{0}: {1}", message.Level, message.Message);
            }
        }

        public override void Dispose()
        {
            Connection.Dispose();
        }
    }
}

// Copyright (C) 2013 Greg Najda
//
// This file is part of mydbsc.
//
// mydbsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.  If not, see <http://www.gnu.org/licenses/>.