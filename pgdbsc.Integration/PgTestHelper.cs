using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TestUtils.Sql;
using Dapper;
using Npgsql;
using Xunit;

namespace dbsc.Postgres.Integration
{
    public class PgTestHelper : SqlTestHelper
    {
        public override string DatabaseNameFromScripts { get { return "pgdbsc_test"; } }

        private string _testDatabaseHost;
        public override string TestDatabaseHost { get { return _testDatabaseHost; } }

        private int _testDatabasePort;
        public override int TestDatabasePort { get { return _testDatabasePort; } }

        private string _testDatabaseUsername;
        public override string TestDatabaseUsername { get { return _testDatabaseUsername; } }

        private string _testDatabasePassword;
        public override string TestDatabasePassword { get { return _testDatabasePassword; } }

        private string _testDatabaseName;
        public override string TestDatabaseName { get { return _testDatabaseName; } }

        private string _altTestDatabaseName;
        public override string AltTestDatabaseName { get { return _altTestDatabaseName; } }

        private string _sourceDatabaseHost;
        public override string SourceDatabaseHost { get { return _sourceDatabaseHost; } }

        private int _sourceDatabasePort;
        public override int SourceDatabasePort { get { return _sourceDatabasePort; } }

        private string _sourceDatabaseUsername;
        public override string SourceDatabaseUsername { get { return _sourceDatabaseUsername; } }

        private string _sourceDatabasePassword;
        public override string SourceDatabasePassword { get { return _sourceDatabasePassword; } }

        private string _sourceDatabaseName;
        public override string SourceDatabaseName { get { return _sourceDatabaseName; } }

        private string _altSourceDatabaseName;
        public override string AltSourceDatabaseName { get { return _altSourceDatabaseName; } }

        public override string DbscProjectName { get { return "pgdbsc"; } }
        public override string DbscExeDllName { get { return "pgdbsc.dll"; } }

        public string IntegratedSecurityTargetDatabaseName { get { return "pgdbsc_integrated_security_test"; } }
        public string IntegratedSecurityPostgresUsername { get { return "dbsc_test_user_sspi"; } }

        public PgTestHelper()
        {
            _testDatabaseHost = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_HOST") ?? "localhost";
            _testDatabasePort = int.Parse(Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_PORT") ?? "8610");
            _testDatabaseUsername = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_USERNAME") ?? "dbsc_test_user";
            _testDatabasePassword = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_PASSWORD") ?? "testpw_dest";
            _testDatabaseName = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_DB") ?? "pgdbsc_test_explicit_db_name";
            _altTestDatabaseName = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_DESTINATION_DB2") ?? "pgdbsc_test2";

            _sourceDatabaseHost = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_HOST") ?? "localhost";
            _sourceDatabasePort = int.Parse(Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_PORT") ?? "8611");
            _sourceDatabaseUsername = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_USERNAME") ?? "dbsc_test_user";
            _sourceDatabasePassword = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_PASSWORD") ?? "testpw_source";
            _sourceDatabaseName = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_DB") ?? "pgdbsc_test_source";
            _altSourceDatabaseName = Environment.GetEnvironmentVariable("PGDBSC_INTEGRATION_TEST_SOURCE_DB2") ?? "pgdbsc_test_source_2";
        }

        public override void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnectionForDbName)
        {
            using (IDbConnection conn = getDbConnectionForDbName("postgres"))
            {
                conn.Execute(string.Format("DROP DATABASE IF EXISTS {0}", dbName));
            }
        }

        private const int ExpectedCreateTemplateConnLimit = 19;

        public override void VerifyCreationTemplateRan(string dbName, Func<string, IDbConnection> getDbConnectionForDbName)
        {
            using (IDbConnection conn = getDbConnectionForDbName(dbName))
            {
                string connLimitSql = string.Format(@"SELECT datconnlimit FROM pg_database WHERE datname = '{0}'",
                    dbName);
                int connLimit = conn.Query<int>(connLimitSql).First();
                Assert.Equal(ExpectedCreateTemplateConnLimit, connLimit);
            }
        }

        public override IDbConnection GetDbConnection(string host, int port, string username, string password, string dbName)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.Database = dbName;
            builder.Host = host;
            builder.Port = port;

            if (password == null)
            {
                builder.Username = username;
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.Username = username;
                builder.Password = password;
            }

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
            Assert.Contains(indexes, ix => ix.index_name == "ix_person__name");
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