﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.SqlServer
{
    /// <summary>
    /// Typical settings needed for opening a database connection.
    /// </summary>
    public class SqlServerConnectionSettings : IConnectionSettings, ICloneable
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool UseIntegratedSecurity { get { return Username == null; } }

        public int ConnectTimeoutInSeconds { get; set; }
        public int ScriptTimeoutInSeconds { get; set; }
        public int CommandTimeoutInSeconds { get; set; }
        public int ImportTableTimeoutInSeconds { get; set; }

        public SqlServerConnectionSettings(string server, string database, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;

            ConnectTimeoutInSeconds = 10;
            ScriptTimeoutInSeconds = 60 * 60 * 24 * 7; // 1 week
            CommandTimeoutInSeconds = 15;
            ImportTableTimeoutInSeconds = 60 * 60 * 24 * 7;
        }

        public string ToDescriptionString()
        {
            return Database + " on " + Server;
        }

        public virtual SqlServerConnectionSettings Clone()
        {
            SqlServerConnectionSettings clone = new SqlServerConnectionSettings(server: Server, database: Database, username: Username, password: Password);
            clone.ConnectTimeoutInSeconds = this.ConnectTimeoutInSeconds;
            clone.ScriptTimeoutInSeconds = this.ScriptTimeoutInSeconds;
            clone.CommandTimeoutInSeconds = this.CommandTimeoutInSeconds;
            ImportTableTimeoutInSeconds = this.ImportTableTimeoutInSeconds;
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
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