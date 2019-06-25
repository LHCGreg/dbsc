using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Xunit;

namespace TestUtils.Sql
{
    public abstract class SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        protected THelper Helper { get; private set; }

        protected abstract int? Port { get; }
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
            Skip.If(Port == null, "Port not relevant for this DB engine.");
        }

        protected string TestDatabaseName { get { return Helper.TestDatabaseName; } }
        protected string AltTestDatabaseName { get { return Helper.AltTestDatabaseName; } }
        protected string SourceDatabaseName { get { return Helper.SourceDatabaseName; } }
        protected string AltSourceDatabaseName { get { return Helper.AltSourceDatabaseName; } }
        protected string Username { get { return Helper.Username; } }
        protected string Password { get { return Helper.Password; } }
        protected string DbscExeName { get { return Helper.DbscExeName; } }

        protected string DbscExePath { get { return Helper.DbscExePath; } }
        protected string ScriptsDir { get { return Helper.ScriptsDir; } }

        protected void DropDatabase(string dbName)
        {
            Helper.DropDatabase(dbName);
        }

        protected void DropDatabase(string dbName, Func<string, IDbConnection> getDbConnection)
        {
            Helper.DropDatabase(dbName, getDbConnection);
        }

        protected void VerifyCreationTemplateRan(string dbName)
        {
            Helper.VerifyCreationTemplateRan(dbName);
        }

        protected IDbConnection GetDbConnection(string dbName)
        {
            return Helper.GetDbConnection(dbName);
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

        protected void RunSuccessfulCommand(string arguments)
        {
            Helper.RunSuccessfulCommand(arguments);
        }

        protected void RunUnsuccessfulCommand(string arguments)
        {
            Helper.RunUnsuccessfulCommand(arguments);
        }

        protected void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            Helper.VerifyDatabase(dbName, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion);
        }

        protected void VerifyDatabase(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion, Func<string, IDbConnection> getDbConnection)
        {
            Helper.VerifyDatabase(dbName, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion, getDbConnection);
        }
    }
}
