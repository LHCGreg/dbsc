using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using TestUtils.Sql;

namespace oradbsc.Integration
{
    class OracleTestHelper : SqlTestHelper
    {
        public override string TestDatabaseName { get { return "XE"; } }
        public override string Username { get { return "dbsc_test_user"; } }
        public override string Password { get { return "testpw"; } }
        public override string DbscExeName { get { return "oradbsc.exe"; } }

        // Oracle is weird. We can't really drop the database, so just drop the test tables.
        public override void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnection)
        {
            using (IDbConnection conn = getDbConnection(dbName))
            {
                DropTableIfExists(conn, "dbsc_metadata");
                DropTableIfExists(conn, "book");
                DropTableIfExists(conn, "person");
                DropTableIfExists(conn, "script_isolation_test");
            }
        }

        private void DropTableIfExists(IDbConnection conn, string tableName)
        {
            string existsSql = string.Format("SELECT count(*) AS c FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName.ToUpperInvariant());
            if (conn.Query<OracleCount>(existsSql).First().c > 0)
            {
                Console.WriteLine("Dropping table {0}", tableName);
                conn.Execute(string.Format("DROP TABLE {0}", tableName));
            }
        }

        private class OracleCount
        {
            public int c { get; set; }
        }

        public override IDbConnection GetDbConnection(string dbName)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
            builder.DataSource = dbsc.Oracle.OraDbscDbConnection.GetDataSourceString("localhost", 1521, dbName);
            builder.Password = Password;
            builder.UserID = Username;
            builder.ValidateConnection = true;
            string connectionString = builder.ToString();

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();
            return conn;
        }

        public override void VerifyPersonNameIndexExists(IDbConnection conn)
        {
            string sql = "SELECT count(*) AS c FROM USER_INDEXES WHERE index_name = 'IX_PERSON__NAME'";
            if (conn.Query<OracleCount>(sql).First().c == 0)
            {
                Assert.Fail("Person name index does not exist.");
            }
        }

        public override List<script_isolation_test> ExpectedIsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>() { new script_isolation_test() { step = 1, val = "x" } };
            }
        }

        public override List<script_isolation_test> ExpectedRevision0IsolationTestValues
        {
            get
            {
                return ExpectedIsolationTestValues;
            }
        }

        public override string AltTestDatabaseName
        {
            get { throw new NotImplementedException(); }
        }

        public override string SourceDatabaseName
        {
            get { throw new NotImplementedException(); }
        }

        public override string AltSourceDatabaseName
        {
            get { throw new NotImplementedException(); }
        }

        public override void VerifyCreationTemplateRan(string dbName)
        {
            throw new NotImplementedException();
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