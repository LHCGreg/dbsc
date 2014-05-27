using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils.Sql;
using Dapper;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace dbsc.MySql.Integration
{
    public class MySqlTestHelper : SqlTestHelper
    {
        public override string TestDatabaseName { get { return "mydbsc_test"; } }
        public override string AltTestDatabaseName { get { return "mydbsc_test_2"; } }
        public override string SourceDatabaseName { get { return "mydbsc_test_source"; } }
        public override string AltSourceDatabaseName { get { return "mydbsc_test_source_2"; } }
        public override string Username { get { return "dbsc_test_user"; } }
        public override string Password { get { return "testpw"; } }
        public override string DbscExeName { get { return "mydbsc.exe"; } }

        public override void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnection)
        {
            using (IDbConnection conn = getDbConnection(null))
            {
                string sql = string.Format("DROP DATABASE IF EXISTS {0}", dbName);
                conn.Execute(sql);
            }
        }

        public override void VerifyCreationTemplateRan(string dbName)
        {
            using (IDbConnection conn = GetDbConnection(dbName))
            {
                string sql = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'creation_template_ran' AND TABLE_SCHEMA = @db";
                bool templateRan = conn.Query(sql, new { db = dbName }).Any();
                Assert.That(templateRan, Is.True);
            }
        }

        public override IDbConnection GetDbConnection(string dbName)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.UserID = Username;
            builder.Password = Password;
            builder.Database = dbName;

            // Turn off connection pooling so that connections don't hang around preventing the database from being able to be dropped
            builder.Pooling = false;
            builder.Server = "localhost";

            string connectionString = builder.ToString();

            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public override void VerifyPersonNameIndexExists(IDbConnection conn)
        {
            string sql = "SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS WHERE INDEX_NAME = 'ix_person__name'";
            bool indexExists = conn.Query(sql).Any();
            Assert.That(indexExists, Is.True);
        }

        public override List<script_isolation_test> ExpectedIsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>()
                {
                    new script_isolation_test() { step = 0, val = "1" },
                    new script_isolation_test() { step = 1, val = "5" },
                    new script_isolation_test() { step = 2, val = "1" }
                };
            }
        }

        public override List<script_isolation_test> ExpectedRevision0IsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>()
                {
                    new script_isolation_test() { step = 0, val = "1" },
                    new script_isolation_test() { step = 1, val = "5" },
                };
            }
        }
    }
}

// Copyright (C) 2013 Greg Najda
//
// This file is part of mydbsc.Integration.
//
// mydbsc.Integration is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc.Integration is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.Integration.  If not, see <http://www.gnu.org/licenses/>.