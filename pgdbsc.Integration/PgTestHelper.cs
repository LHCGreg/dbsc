using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TestUtils.Sql;
using Dapper;
using NUnit.Framework;
using Npgsql;

namespace dbsc.Postgres.Integration
{
    class PgTestHelper : SqlTestHelper
    {
        public override string TestDatabaseName { get { return "pgdbsc_test"; } }
        public override string AltTestDatabaseName { get { return "pgdbsc_test_2"; } }
        public override string SourceDatabaseName { get { return "pgdbsc_test_source"; } }
        public override string AltSourceDatabaseName { get { return "pgdbsc_test_source_2"; } }
        public override string Username { get { return "dbsc_test_user"; } }
        public override string Password { get { return "testpw"; } }
        public override string DbscExeName { get { return "pgdbsc.exe"; } }

        public override void DropDatabase(string dbName)
        {
            using (IDbConnection conn = GetDbConnection("postgres"))
            {
                conn.Execute(string.Format("DROP DATABASE IF EXISTS {0}", dbName));
            }
        }

        private const int ExpectedCreateTemplateConnLimit = 19;

        public override void VerifyCreationTemplateRan(string dbName)
        {
            using (IDbConnection conn = GetDbConnection(dbName))
            {
                string connLimitSql = string.Format(@"SELECT datconnlimit FROM pg_database WHERE datname = '{0}'",
                    dbName);
                int connLimit = conn.Query<int>(connLimitSql).First();
                Assert.That(connLimit, Is.EqualTo(ExpectedCreateTemplateConnLimit));
            }
        }

        public override IDbConnection GetDbConnection(string dbName)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.Database = dbName;
            builder.Host = "localhost";
            builder.Password = Password;
            builder.UserName = Username;

            // Turn off connection pooling so that connections don't hang around preventing the database from being able to be dropped
            builder.Pooling = false;

            string connectionString = builder.ToString();

            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public override void VerifyPersonNameIndexExists(IDbConnection conn)
        {
            string indexQuerySql = @"SELECT ind_more.relname AS index_name
FROM pg_index AS ind
JOIN pg_class AS tab ON ind.indrelid = tab.oid
JOIN pg_class AS ind_more ON ind.indexrelid = ind_more.oid
JOIN pg_namespace AS table_schema ON tab.relnamespace = table_schema.oid
JOIN pg_namespace AS ind_schema ON ind_more.relnamespace = ind_schema.oid
WHERE table_schema.nspname NOT LIKE 'pg_%' AND table_schema.nspname <> 'information_schema'
AND ind_schema.nspname NOT LIKE 'pg_%' AND ind_schema.nspname <> 'information_schema'
AND tab.relname <> 'dbsc_metadata'";

            List<PgIndex> indexes = conn.Query<PgIndex>(indexQuerySql).ToList();
            Assert.That(indexes.Any(ix => ix.index_name == "ix_person__name"), Is.True);
        }

        class PgIndex
        {
            public string index_name { get; set; }
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