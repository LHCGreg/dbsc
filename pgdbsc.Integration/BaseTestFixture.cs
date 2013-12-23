using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TestUtils;
using Dapper;
using System.Globalization;

namespace dbsc.Postgres.Integration
{
    [TestFixture]
    abstract class BaseTestFixture
    {
        protected static readonly string TestDatabaseName = "pgdbsc_test";
        protected static readonly string AltTestDatabaseName = "pgdbsc_test_2";
        protected static readonly string SourceDatabaseName = "pgdbsc_test_source";
        protected static readonly string AltSourceDatabaseName = "pgdbsc_test_source_2";
        protected static readonly string Username = "dbsc_test_user";
        protected static readonly string Password = "testpw";

        protected string PgdbscPath { get; private set; }
        protected string ScriptsDir { get; private set; }

        [TestFixtureSetUp]
        public void SetDirectories()
        {
            Uri thisAssemblyUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string thisAssemblyPath = thisAssemblyUri.LocalPath;
            string thisAssemblyDir = Path.GetDirectoryName(thisAssemblyPath);
            PgdbscPath = Path.Combine(thisAssemblyDir, "pgdbsc.exe");
            ScriptsDir = Path.Combine(thisAssemblyDir, "scripts");
        }

        protected List<Person> ExpectedPeople = new List<Person>()
        {
            new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
            new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
            new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
        };

        protected List<Person> ExpectedSourcePeople = new List<Person>()
        {
            new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
            new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
        };

        protected List<Person> ExpectedAltSourcePeople = new List<Person>()
        {
            new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
            new Person() { birthday = new DateTime(2012, 2, 3), name = "Christina", default_test = null },
            new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
        };

        protected List<Person> ExpectedRevision0People = new List<Person>()
        {
            new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
            new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 }
        };

        protected List<Person> ExpectedRevision1People = new List<Person>()
        {
            new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
            new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 }
        };

        protected static List<Book> GetExpectedBooks(List<Person> people)
        {
            int authorId = people.Where(p => p.name == "George R.R. Martin").Select(p => p.person_id).First();
            return new List<Book>()
            {
                new Book() { title = "A Game of Thrones", author_person_id = authorId, subtitle = null }
            };
        }

        protected Func<List<Person>, List<Book>> GetExpectedBooksFunc = people => GetExpectedBooks(people);

        protected List<script_isolation_test> ExpectedIsolationTestValues = new List<script_isolation_test>()
        {
            new script_isolation_test() { step = 0, val = "on" },
            new script_isolation_test() { step = 1, val = "off" },
            new script_isolation_test() { step = 2, val = "on" }
        };

        protected List<script_isolation_test> ExpectedRevision0IsolationTestValues = new List<script_isolation_test>()
        {
            new script_isolation_test() { step = 0, val = "on" },
            new script_isolation_test() { step = 1, val = "off" },
        };

        protected int ExpectedCreateTemplateConnLimit = 19;

        public string GetConnectionString(string dbName)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.Database = dbName;
            builder.Host = "localhost";
            builder.Password = Password;
            builder.UserName = Username;

            // Turn off connection pooling so that connections don't hang around preventing the database from being able to be dropped
            builder.Pooling = false;

            return builder.ToString();
        }

        protected void DropDatabase(string dbName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString("postgres")))
            {
                conn.Open();
                conn.Execute(string.Format("DROP DATABASE IF EXISTS {0}", dbName));
            }
        }

        protected void RunSuccessfulCommand(string arguments)
        {
            ProcessUtils.RunSuccesfulCommand(PgdbscPath, arguments, ScriptsDir);
        }

        protected void RunUnsuccessfulCommand(string arguments)
        {
            ProcessUtils.RunUnsuccesfulCommand(PgdbscPath, arguments, ScriptsDir);
        }

        public void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks, List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString(dbName)))
            {
                conn.Open();
                List<Person> people = conn.Query<Person>("SELECT * FROM person").ToList();
                List<script_isolation_test> isolationTest = conn.Query<script_isolation_test>("SELECT * FROM script_isolation_test").ToList();

                Assert.That(people, Is.EquivalentTo(expectedPeople));

                if (getExpectedBooks != null)
                {
                    List<Book> books = conn.Query<Book>("SELECT * FROM book").ToList();
                    List<Book> expectedBooks = getExpectedBooks(people);
                    Assert.That(books, Is.EquivalentTo(expectedBooks));
                }

                Assert.That(isolationTest, Is.EquivalentTo(expectedIsolationTestValues));

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

                string metadataQuerySql = @"SELECT * FROM dbsc_metadata";
                Dictionary<string, string> metadata = conn.Query<dbsc_metadata>(metadataQuerySql)
                    .ToDictionary(md => md.property_name, md => md.property_value);
                Assert.That(int.Parse(metadata["Version"]), Is.EqualTo(expectedVersion));
                Assert.That(metadata["MasterDatabaseName"], Is.EqualTo("pgdbsc_test"));

                DateTime lastChangeUtc = DateTime.ParseExact(metadata["LastChangeUTC"], "s", CultureInfo.InvariantCulture);
                Assert.That(lastChangeUtc, Is.LessThan(DateTime.UtcNow + TimeSpan.FromMinutes(5)));
                Assert.That(lastChangeUtc, Is.GreaterThan(DateTime.UtcNow - TimeSpan.FromMinutes(5)));
            }
        }

        class PgIndex
        {
            public string index_name { get; set; }
        }

        class dbsc_metadata
        {
            public string property_name { get; set; }
            public string property_value { get; set; }
        }

        public void VerifyCreationTemplateRan(string dbName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString(dbName)))
            {
                conn.Open();
                string connLimitSql = string.Format(@"SELECT datconnlimit FROM pg_database
WHERE datname = '{0}'", dbName);
                int connLimit = conn.Query<int>(connLimitSql).First();
                Assert.That(connLimit, Is.EqualTo(ExpectedCreateTemplateConnLimit));
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