﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.MySql
{
    class MySqlDbscDbConnection : BaseDbscDbConnection<MySqlConnection, MySqlTransaction>
    {
        public int ScriptTimeoutInSeconds { get; private set; }
        public int ImportTableTimeoutInSeconds { get; private set; }
        public DbConnectionInfo ConnectionInfo { get; private set; }
        
        public MySqlDbscDbConnection(DbConnectionInfo connectionInfo)
            : base(OpenConnection(connectionInfo))
        {
            ScriptTimeoutInSeconds = connectionInfo.ScriptTimeoutInSeconds;
            CommandTimeoutInSeconds = connectionInfo.CommandTimeoutInSeconds;
            ImportTableTimeoutInSeconds = connectionInfo.ImportTableTimeoutInSeconds;
            ConnectionInfo = connectionInfo;
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

            builder.UserID = connectionInfo.Username;
            builder.Password = connectionInfo.Password;

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
                Connection.Execute(sql, commandTimeout: ScriptTimeoutInSeconds);
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

        private class Table
        {
            public string TABLE_NAME { get; set; }
        }

        public ICollection<MySqlTable> GetTablesExceptMetadata()
        {
            string sql =
@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME <> 'dbsc_metadata'
AND TABLE_SCHEMA = @db
AND TABLE_TYPE = 'BASE TABLE'";

            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "db", ConnectionInfo.Database } };

            List<MySqlTable> tables = Query<Table>(sql, sqlParams).Select(table => new MySqlTable(table.TABLE_NAME)).ToList();
            return tables;
        }
    }
}

// Copyright (C) 2014 Greg Najda
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