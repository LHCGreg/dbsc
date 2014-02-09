using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils.Sql;
using Dapper;
using NUnit.Framework;

namespace dbsc.SqlServer.Integration
{
    public class MSTestHelper : SqlTestHelper
    {
        public override string TestDatabaseName { get { return "msdbsc_test"; } }
        public override string AltTestDatabaseName { get { return "msdbsc_test_2"; } }
        public override string SourceDatabaseName { get { return "msdbsc_test_source"; } }
        public override string AltSourceDatabaseName { get { return "msdbsc_test_source_2"; } }
        public override string Username { get { return "dbsc_test_user"; } }
        public override string Password { get { return "testpw"; } }
        public override string DbscExeName { get { return "msdbsc.exe"; } }

        public override void DropDatabase(string dbName)
        {
            using (IDbConnection conn = GetDbConnection("master"))
            {
                string existenceCheckSql = @"SELECT 1 FROM sys.databases WHERE name = @dbName";
                if (conn.Query(existenceCheckSql, new { dbName = dbName }).Any())
                {
                    string dropSql = string.Format("DROP DATABASE {0}", dbName);
                    conn.Execute(dropSql);
                }
            }
        }

        public override void VerifyCreationTemplateRan(string dbName)
        {
            using (IDbConnection conn = GetDbConnection(dbName))
            {
                string sql = @"SELECT 1 FROM sys.tables WHERE name = 'creation_template_ran'";
                bool templateRan = conn.Query(sql).Any();
                Assert.That(templateRan, Is.True);
            }
        }

        public override IDbConnection GetDbConnection(string dbName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.InitialCatalog = dbName;
            builder.IntegratedSecurity = false;
            builder.UserID = Username;
            builder.Password = Password;

            // Turn off connection pooling so that connections don't hang around preventing the database from being able to be dropped
            builder.Pooling = false;

            string connectionString = builder.ToString();

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public override void VerifyPersonNameIndexExists(IDbConnection conn)
        {
            string sql = "SELECT 1 FROM sys.indexes WHERE name = 'ix_person__name'";
            bool indexExists = conn.Query(sql).Any();
            Assert.That(indexExists, Is.True);
        }

        public override List<script_isolation_test> ExpectedIsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>()
                {
                    new script_isolation_test() { step = 0, val = "on" },
                    new script_isolation_test() { step = 1, val = "off" },
                    new script_isolation_test() { step = 2, val = "on" }
                };
            }
        }

        public override List<script_isolation_test> ExpectedRevision0IsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>()
                {
                    new script_isolation_test() { step = 0, val = "on" },
                    new script_isolation_test() { step = 1, val = "off" },
                };
            }
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