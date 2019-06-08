﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    /// <summary>
    /// Typical settings needed for opening a database connection.
    /// </summary>
    public class DbConnectionInfo : IConnectionSettings, ICloneable
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool UseIntegratedSecurity { get { return Password == null; } }

        /// <summary>
        /// If null, use the normal port for the database.
        /// </summary>
        public int? Port { get; set; }

        public int ConnectTimeoutInSeconds { get; set; }
        public int ScriptTimeoutInSeconds { get; set; }
        public int CommandTimeoutInSeconds { get; set; }
        public int ImportTableTimeoutInSeconds { get; set; }

        public DbConnectionInfo(string server, string database, int? port, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
            Port = port;

            ConnectTimeoutInSeconds = 10;
            ScriptTimeoutInSeconds = 60 * 60 * 24 * 7; // 1 week
            CommandTimeoutInSeconds = 15;
            ImportTableTimeoutInSeconds = 60 * 60 * 24 * 7;
        }

        public string ToDescriptionString()
        {
            return Database + " on " + Server;
        }

        public virtual DbConnectionInfo Clone()
        {
            DbConnectionInfo clone = new DbConnectionInfo(server: Server, database: Database, port: Port, username: Username, password: Password);
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
