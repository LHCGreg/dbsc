using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using System.Globalization;
using FluentAssertions;

namespace TestUtils.Sql
{
    public abstract class SqlTestHelper
    {
        public abstract string DatabaseNameFromScripts { get; }

        public abstract string TestDatabaseHost { get; }
        public abstract int TestDatabasePort { get; }
        public abstract string TestDatabaseUsername { get; }
        public abstract string TestDatabasePassword { get; }
        public abstract string TestDatabaseName { get; }
        public abstract string AltTestDatabaseName { get; }

        public abstract string SourceDatabaseHost { get; }
        public abstract int SourceDatabasePort { get; }
        public abstract string SourceDatabaseUsername { get; }
        public abstract string SourceDatabasePassword { get; }
        public abstract string SourceDatabaseName { get; }
        public abstract string AltSourceDatabaseName { get; }

        public abstract string DbscExeDllName { get; }
        public abstract string DbscProjectName { get; }

        public string DbscExeDllPath { get; private set; }
        public string ScriptsDir { get; private set; }
        public string ScriptsForOtherDBDir { get { return Path.Combine(ScriptsDir, "..", "scripts_for_other_db"); } }

        public abstract void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnectionForDbName);
        public abstract void VerifyCreationTemplateRan(string dbName, Func<string, IDbConnection> getDbConnectionForDbName);
        public abstract IDbConnection GetDbConnection(string host, int port, string username, string password, string dbName);
        public abstract void VerifyPersonNameIndexExists(IDbConnection conn);

        public SqlTestHelper()
        {
            Uri thisAssemblyUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string thisAssemblyPath = thisAssemblyUri.LocalPath;
            string thisAssemblyDir = Path.GetDirectoryName(thisAssemblyPath);

            // Pretty hacky...but can't just use the program dll that gets copied into the integration test project's bin folder.
            // That might be built against a different version of the framework. Currently the integration test projects require
            // .net core 3.0 for .net standard 2.1 support for ProcessStartInfo.ArgumentList, while the programs themselves only
            // require .net core 2.0.
            DbscExeDllPath = Path.Combine(thisAssemblyDir, "..", "..", "..", "..", DbscProjectName, "bin", "Debug", "netcoreapp2.0", DbscExeDllName);
            ScriptsDir = Path.Combine(thisAssemblyDir, "scripts");
        }

        public void DropDatabase(IntegrationTestDbHost host, string dbName)
        {
            DropDatabase(dbName, (dbToConnectToForDrop) => GetDbConnection(host, dbToConnectToForDrop));
        }

        public IDbConnection GetDbConnection(IntegrationTestDbHost host, string dbName)
        {
            if (host == IntegrationTestDbHost.Destination)
            {
                return GetDbConnection(TestDatabaseHost, TestDatabasePort, TestDatabaseUsername, TestDatabasePassword, dbName);
            }
            else if (host == IntegrationTestDbHost.Source)
            {
                return GetDbConnection(SourceDatabaseHost, SourceDatabasePort, SourceDatabaseUsername, SourceDatabasePassword, dbName);
            }
            else
            {
                throw new Exception("oops, forgot to handle a db host.");
            }
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

        public void RunSuccessfulCommand(IReadOnlyCollection<string> arguments)
        {
            ProcessUtils.RunSuccessfulDbscCommand(DbscExeDllPath, arguments, ScriptsDir);
        }

        public void RunSuccessfulCommand(IReadOnlyCollection<string> arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunSuccessfulDbscCommand(DbscExeDllPath, arguments, ScriptsDir, out stdout, out stderr);
        }

        public void RunUnsuccessfulCommand(IReadOnlyCollection<string> arguments)
        {
            ProcessUtils.RunUnsuccessfulDbscCommand(DbscExeDllPath, arguments, ScriptsDir);
        }

        public void RunUnsuccessfulCommand(IReadOnlyCollection<string> arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunUnsuccessfulDbscCommand(DbscExeDllPath, arguments, ScriptsDir, out stdout, out stderr);
        }

        public void VerifyDatabase(IntegrationTestDbHost host, string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            VerifyDatabase(() => GetDbConnection(host, dbName), expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion);
        }

        public void VerifyDatabase(Func<IDbConnection> getDbConnection, List<Person> expectedPeople, Func<List<Person>,
            List<Book>> getExpectedBooks, List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            using (IDbConnection conn = getDbConnection())
            {
                List<Person> people = conn.Query<Person>("SELECT * FROM person").ToList();
                List<script_isolation_test> isolationTest = conn.Query<script_isolation_test>("SELECT * FROM script_isolation_test").ToList();

                people.Should().BeEquivalentTo(expectedPeople);

                if (getExpectedBooks != null)
                {
                    List<Book> books = conn.Query<Book>("SELECT * FROM book").ToList();
                    List<Book> expectedBooks = getExpectedBooks(people);
                    books.Should().BeEquivalentTo(expectedBooks);
                }

                isolationTest.Should().BeEquivalentTo(expectedIsolationTestValues);

                VerifyPersonNameIndexExists(conn);

                string metadataQuerySql = @"SELECT * FROM dbsc_metadata";
                Dictionary<string, string> metadata = conn.Query<dbsc_metadata>(metadataQuerySql)
                    .ToDictionary(md => md.property_name, md => md.property_value);
                Assert.Equal(expectedVersion, int.Parse(metadata["Version"]));
                Assert.Equal(DatabaseNameFromScripts, metadata["MasterDatabaseName"]);

                DateTime lastChangeUtc = DateTime.ParseExact(metadata["LastChangeUTC"], "s", CultureInfo.InvariantCulture);
                Assert.True(DateTime.UtcNow + TimeSpan.FromMinutes(5) > lastChangeUtc);
                Assert.True(DateTime.UtcNow - TimeSpan.FromMinutes(5) < lastChangeUtc);
            }
        }
    }
}
