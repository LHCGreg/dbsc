using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace TestUtils.Sql
{
    public abstract class SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        protected THelper Helper { get; private set; }

        protected virtual bool PortsSupported { get { return true; } }
        protected virtual bool ImportSupported { get { return true; } }
        protected virtual bool TemplateSupported { get { return true; } }
        protected abstract bool CustomSelectImportSupported { get; }

        /// <summary>
        /// If table specification file supports wildcards and negations
        /// </summary>
        protected abstract bool ExtendedTableSpecsSupported { get; }

        protected SqlBaseTestFixture()
        {
            Helper = new THelper();
        }

        protected void IgnoreIfImportNotSupported()
        {
            Skip.IfNot(ImportSupported, "Import not supported.");
        }

        protected void IgnoreIfCustomSelectImportNotSupported()
        {
            Skip.IfNot(CustomSelectImportSupported, "Custom select import not supported.");
        }

        protected void IgnoreIfExtendedTableSpecsNotSupported()
        {
            Skip.IfNot(ExtendedTableSpecsSupported, "Extended table spec syntax not supported.");
        }

        protected void IgnoreIfTemplateNotSupported()
        {
            Skip.IfNot(TemplateSupported);
        }

        protected void IgnoreIfPortNotSupported()
        {
            Skip.If(!PortsSupported, "Port not relevant for this DB engine.");
        }

        protected string DatabaseNameFromScripts { get { return Helper.DatabaseNameFromScripts; } }
        
        protected string TestDatabaseHost { get { return Helper.TestDatabaseHost; } }
        protected int TestDatabasePort { get { return Helper.TestDatabasePort; } }
        protected string TestDatabaseUsername { get { return Helper.TestDatabaseUsername; } }
        protected string TestDatabasePassword { get { return Helper.TestDatabasePassword; } }
        protected string TestDatabaseName { get { return Helper.TestDatabaseName; } }
        protected string AltTestDatabaseName { get { return Helper.AltTestDatabaseName; } }

        protected string SourceDatabaseHost { get { return Helper.SourceDatabaseHost; } }
        protected int SourceDatabasePort { get { return Helper.SourceDatabasePort; } }
        protected string SourceDatabaseUsername { get { return Helper.SourceDatabaseUsername; } }
        protected string SourceDatabasePassword { get { return Helper.SourceDatabasePassword; } }
        protected string SourceDatabaseName { get { return Helper.SourceDatabaseName; } }
        protected string AltSourceDatabaseName { get { return Helper.AltSourceDatabaseName; } }

        protected string DbscExeDllName { get { return Helper.DbscExeDllName; } }

        protected string DbscExeDllPath { get { return Helper.DbscExeDllPath; } }
        protected string ScriptsDir { get { return Helper.ScriptsDir; } }

        protected List<string> GetDestinationArgs()
        {
            return new List<string>()
            {
                "-targetDbServer", TestDatabaseHost,
                "-targetDbPort", TestDatabasePort.ToString(CultureInfo.InvariantCulture),
                "-u", TestDatabaseUsername,
                "-p", TestDatabasePassword
            };
        }

        protected List<string> GetSourceArgs()
        {
            return new List<string>()
            {
                "-sourceDbServer", SourceDatabaseHost,
                "-sourceDbPort", SourceDatabasePort.ToString(CultureInfo.InvariantCulture),
                "-sourceUsername", SourceDatabaseUsername,
                "-sourcePassword", SourceDatabasePassword
            };
        }

        protected List<string> GetDestinationAsSourceArgs()
        {
            return new List<string>()
            {
                "-sourceDbServer", TestDatabaseHost,
                "-sourceDbPort", TestDatabasePort.ToString(CultureInfo.InvariantCulture),
                "-sourceUsername", TestDatabaseUsername,
                "-sourcePassword", TestDatabasePassword
            };
        }

        protected void DropDatabase(IntegrationTestDbHost host, string dbName)
        {
            Helper.DropDatabase(host, dbName);
        }

        protected void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnectionForDbName)
        {
            Helper.DropDatabase(dbName, getDbConnectionForDbName);
        }

        protected void VerifyCreationTemplateRan(IntegrationTestDbHost host, string dbName)
        {
            Helper.VerifyCreationTemplateRan(dbName, databaseName => GetDbConnection(host, databaseName));
        }

        protected IDbConnection GetDbConnection(IntegrationTestDbHost host, string dbName)
        {
            return Helper.GetDbConnection(host, dbName);
        }

        protected void VerifyPersonNameIndexExists(IDbConnection conn)
        {
            Helper.VerifyPersonNameIndexExists(conn);
        }

        protected List<Person> ExpectedPeople { get { return Helper.ExpectedPeople; } }
        protected List<Person> ExpectedSourcePeople { get { return Helper.ExpectedSourcePeople; } }
        protected List<Person> ExpectedSourcePeopleCustomSelect { get { return Helper.ExpectedSourcePeopleCustomSelect; } }
        protected List<Person> ExpectedAltSourcePeople { get { return Helper.ExpectedAltSourcePeople; } }
        protected List<Person> ExpectedRevision0People { get { return Helper.ExpectedRevision0People; } }
        protected List<Person> ExpectedRevision1People { get { return Helper.ExpectedRevision1People; } }
        protected Func<List<Person>, List<Book>> GetExpectedBooksFunc { get { return Helper.GetExpectedBooksFunc; } }
        protected List<script_isolation_test> ExpectedIsolationTestValues { get { return Helper.ExpectedIsolationTestValues; } }
        protected List<script_isolation_test> ExpectedRevision0IsolationTestValues { get { return Helper.ExpectedRevision0IsolationTestValues; } }
        protected List<script_isolation_test> ExpectedSourceIsolationTestValues { get { return Helper.ExpectedSourceIsolationTestValues; } }

        protected void RunSuccessfulCommand(IReadOnlyCollection<string> arguments)
        {
            Helper.RunSuccessfulCommand(arguments);
        }

        protected void RunUnsuccessfulCommand(IReadOnlyCollection<string> arguments)
        {
            Helper.RunUnsuccessfulCommand(arguments);
        }

        protected void VerifyDatabase(IntegrationTestDbHost host, string dbName, List<Person> expectedPeople,
            Func<List<Person>, List<Book>> getExpectedBooks, List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            Helper.VerifyDatabase(host, dbName, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion);
        }

        protected void VerifyDatabase(Func<IDbConnection> getDbConnection, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            Helper.VerifyDatabase(getDbConnection, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion);
        }
    }
}
