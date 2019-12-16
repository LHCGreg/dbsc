using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Globalization;

namespace TestUtils.Sql
{
    public abstract class AbstractCheckoutTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {        
        [Fact]
        public void BasicTest()
        {
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void BasicImportTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, TestDatabaseName);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-targetDb", TestDatabaseName });
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportOnlyTwoCollectionsTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithCustomSelectTest()
        {
            IgnoreIfCustomSelectImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_custom_select.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedSourcePeopleCustomSelect, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithOnlyNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_only_negations.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithWildcardsAndNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_wildcards_and_negations.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void TestTargetDb()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, TestDatabaseName);
            DropDatabase(IntegrationTestDbHost.Destination, AltTestDatabaseName);

            // First get the source database into the the main test database
            List<string> checkout1Args = new List<string>() { "checkout" };
            checkout1Args.AddRange(GetDestinationArgs());
            checkout1Args.AddRange(new List<string>() { "-targetDb", TestDatabaseName });
            checkout1Args.AddRange(GetSourceArgs());
            checkout1Args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(checkout1Args);

            List<string> checkout2Args = new List<string>() { "checkout" };
            checkout2Args.AddRange(GetDestinationArgs());
            checkout2Args.AddRange(new List<string>() { "-targetDb", AltTestDatabaseName });
            checkout2Args.AddRange(GetDestinationAsSourceArgs());

            // Then import from the main test database into the alt test database
            RunSuccessfulCommand(checkout2Args);

            VerifyDatabase(IntegrationTestDbHost.Destination, AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void TestNonexistantTargetDbServer()
        {
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout", "-u", TestDatabaseUsername, "-p", TestDatabasePassword,
                "-targetDbServer", "doesnotexist.local" };

            RunUnsuccessfulCommand(args);
        }

        [SkippableFact]
        public void TestNonexistantTargetPort()
        {
            IgnoreIfPortNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>()
            {
                "checkout",
                "-targetDbServer", TestDatabaseHost,
                "-targetDbPort", "9999",
                "-u", TestDatabaseUsername,
                "-p", TestDatabasePassword
            };
            RunUnsuccessfulCommand(args);
        }

        [SkippableFact]
        public void TestNonExistantSourcePort()
        {
            IgnoreIfImportNotSupported();
            IgnoreIfPortNotSupported();
            
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-sourceDbServer", SourceDatabaseHost, "-sourceDbUsername",
                SourceDatabaseUsername, "-sourceDbPassword", SourceDatabasePassword, "-sourcePort", "9999" });
            RunUnsuccessfulCommand(args);
        }

        [SkippableFact]
        public void TestCreationTemplate()
        {
            IgnoreIfTemplateNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-dbCreateTemplate", "creation_template.sql" });

            RunSuccessfulCommand(args);
            VerifyCreationTemplateRan(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-dir", "../error_test_scripts" });
            RunUnsuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Fact]
        public void TestPartialCheckout()
        {
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "1" });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [SkippableFact]
        public void TestPartialCheckoutWithImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "2" });
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void TestPartialCheckoutShortOfImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "1" });
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });

            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Fact]
        public void TestPartialCheckoutWithTooHighRevision()
        {
            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "3" });
            RunUnsuccessfulCommand(args);
        }

        [SkippableFact]
        public void TestCheckoutContinuesAfterImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(IntegrationTestDbHost.Destination, DatabaseNameFromScripts);

            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", AltSourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(IntegrationTestDbHost.Destination, TestDatabaseName, ExpectedAltSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }
    }
}
