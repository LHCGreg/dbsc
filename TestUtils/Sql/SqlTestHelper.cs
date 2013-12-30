using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using System.Globalization;

namespace TestUtils.Sql
{
    public abstract class SqlTestHelper
    {
        public abstract string TestDatabaseName { get; }
        public abstract string AltTestDatabaseName { get; }
        public abstract string SourceDatabaseName { get; }
        public abstract string AltSourceDatabaseName { get; }
        public abstract string Username { get; }
        public abstract string Password { get; }
        public abstract string DbscExeName { get; }

        public string DbscExePath { get; private set; }
        public string ScriptsDir { get; private set; }

        public abstract void DropDatabase(string dbName);
        public abstract void VerifyCreationTemplateRan(string dbName);
        public abstract IDbConnection GetDbConnection(string dbName);
        public abstract void VerifyPersonNameIndexExists(IDbConnection conn);

        public SqlTestHelper()
        {
            Uri thisAssemblyUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string thisAssemblyPath = thisAssemblyUri.LocalPath;
            string thisAssemblyDir = Path.GetDirectoryName(thisAssemblyPath);
            DbscExePath = Path.Combine(thisAssemblyDir, DbscExeName);
            ScriptsDir = Path.Combine(thisAssemblyDir, "scripts");
        }

        public List<Person> ExpectedPeople
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
                    new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
                };
            }
        }

        public List<Person> ExpectedSourcePeople
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
                    new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
                };
            }
        }

        public List<Person> ExpectedAltSourcePeople
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
                    new Person() { birthday = new DateTime(2012, 2, 3), name = "Christina", default_test = null },
                    new Person() { birthday = new DateTime(2013, 5, 11), name = "Mike", default_test = null }
                };
            }
        }

        public List<Person> ExpectedRevision0People
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 }
                };
            }
        }

        public List<Person> ExpectedRevision1People
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(2012, 6, 7), name = "Greg", default_test = 42 },
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 }
                };
            }
        }

        public static List<Book> GetExpectedBooks(List<Person> people)
        {
            int authorId = people.Where(p => p.name == "George R.R. Martin").Select(p => p.person_id).First();
            return new List<Book>()
            {
                new Book() { title = "A Game of Thrones", author_person_id = authorId, subtitle = null }
            };
        }

        public Func<List<Person>, List<Book>> GetExpectedBooksFunc { get { return people => GetExpectedBooks(people); } }

        public abstract List<script_isolation_test> ExpectedIsolationTestValues { get; }
        public abstract List<script_isolation_test> ExpectedRevision0IsolationTestValues { get; }

        public void RunSuccessfulCommand(string arguments)
        {
            ProcessUtils.RunSuccesfulCommand(DbscExePath, arguments, ScriptsDir);
        }

        public void RunUnsuccessfulCommand(string arguments)
        {
            ProcessUtils.RunUnsuccesfulCommand(DbscExePath, arguments, ScriptsDir);
        }

        public void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            using (IDbConnection conn = GetDbConnection(dbName))
            {
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

                VerifyPersonNameIndexExists(conn);

                string metadataQuerySql = @"SELECT * FROM dbsc_metadata";
                Dictionary<string, string> metadata = conn.Query<dbsc_metadata>(metadataQuerySql)
                    .ToDictionary(md => md.property_name, md => md.property_value);
                Assert.That(int.Parse(metadata["Version"]), Is.EqualTo(expectedVersion));
                Assert.That(metadata["MasterDatabaseName"], Is.EqualTo(TestDatabaseName));

                DateTime lastChangeUtc = DateTime.ParseExact(metadata["LastChangeUTC"], "s", CultureInfo.InvariantCulture);
                Assert.That(lastChangeUtc, Is.LessThan(DateTime.UtcNow + TimeSpan.FromMinutes(5)));
                Assert.That(lastChangeUtc, Is.GreaterThan(DateTime.UtcNow - TimeSpan.FromMinutes(5)));
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