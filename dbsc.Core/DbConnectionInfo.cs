﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class DbConnectionInfo
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// If null, use the normal port for the database.
        /// </summary>
        public int? Port { get; set; }
        public int TimeoutInSeconds { get; set; }

        public DbConnectionInfo(string server, string database, int? port, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
            Port = port;
            TimeoutInSeconds = 60 * 60 * 24 * 7; // 1 week - plenty of time for imports
        }

        public virtual DbConnectionInfo Clone()
        {
            return new DbConnectionInfo(server: Server, database: Database, port: Port, username: Username, password: Password);
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