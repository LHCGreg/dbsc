﻿using NUnit.Framework;
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
        public string ScriptsForOtherDBDir { get { return Path.Combine(ScriptsDir, "..", "scripts_for_other_db"); } }

        public abstract void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnection);
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

        public void DropDatabase(string dbName)
        {
            DropDatabase(dbName, databaseName => GetDbConnection(databaseName));
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

        public List<Person> ExpectedSourcePeopleCustomSelect
        {
            get
            {
                return new List<Person>()
                {
                    new Person() { birthday = new DateTime(1948, 9, 20), name = "George R.R. Martin", default_test = 5 },
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
        public List<script_isolation_test> ExpectedSourceIsolationTestValues
        {
            get
            {
                return new List<script_isolation_test>()
                {
                    new script_isolation_test() { step = 0, val = "imported" }
                };
            }
        }

        public void RunSuccessfulCommand(string arguments)
        {
            ProcessUtils.RunSuccessfulCommand(DbscExePath, arguments, ScriptsDir);
        }

        public void RunSuccessfulCommand(string arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunSuccessfulCommand(DbscExePath, arguments, ScriptsDir, out stdout, out stderr);
        }

        public void RunUnsuccessfulCommand(string arguments)
        {
            ProcessUtils.RunUnsuccessfulCommand(DbscExePath, arguments, ScriptsDir);
        }

        public void RunUnsuccessfulCommand(string arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunUnsuccessfulCommand(DbscExePath, arguments, ScriptsDir, out stdout, out stderr);
        }

        public void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            VerifyDatabase(dbName, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion, GetDbConnection);
        }

        public void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion, Func<string, IDbConnection> getDbConnection)
        {
            using (IDbConnection conn = getDbConnection(dbName))
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