using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TestUtils.Sql
{
    public abstract class AbstractUpdateTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        private void CheckoutZeroOnScriptsDatabase()
        {
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "0" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        private void CheckoutZeroOnNamedDatabase()
        {
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "0" });
            args.AddRange(new List<string>() { "-targetDb", TestDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(TestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        private void CheckoutZeroOnAltDatabase()
        {
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "0" });
            args.AddRange(new List<string>() { "-targetDb", AltTestDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(AltTestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        [Fact]
        public void BasicTest()
        {
            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void PartialUpdateTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "1" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Fact]
        public void UpdateNothingTest()
        {
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(checkoutArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);

            List<string> updateArgs = new List<string>() { "update" };
            updateArgs.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(updateArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void UpdateNothingTestWithExplicitRevision()
        {
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(checkoutArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);

            List<string> updateArgs = new List<string>() { "update" };
            updateArgs.AddRange(GetDestinationArgs());
            updateArgs.AddRange(new List<string>() { "-r", "2" });
            RunSuccessfulCommand(updateArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void TestPartialUpdateWithTooHighRevision()
        {
            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "3" });
            RunUnsuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        [SkippableFact]
        public void BasicImportTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZeroOnNamedDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportOnlyTwoCollectionsTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithCustomSelectTest()
        {
            IgnoreIfCustomSelectImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_custom_select.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedSourcePeopleCustomSelect, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithOnlyNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_only_negations.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void ImportWithWildcardsAndNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            args.AddRange(new List<string>() { "-importTableList", "tables_to_import_wildscards_and_negations.txt" });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void TestTargetDb()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            checkoutArgs.AddRange(GetSourceArgs());
            checkoutArgs.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(checkoutArgs);

            // Then import from the main test database into the alt test database
            CheckoutZeroOnAltDatabase();

            List<string> updateArgs = new List<string>() { "update" };
            updateArgs.AddRange(GetDestinationArgs());
            updateArgs.AddRange(new List<string>() { "-targetDb", AltTestDatabaseName });
            updateArgs.AddRange(GetDestinationAsSourceArgs());
            RunSuccessfulCommand(updateArgs);
            VerifyDatabase(AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Fact]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            checkoutArgs.AddRange(new List<string>() { "-r", "0" });
            checkoutArgs.AddRange(new List<string>() { "-dir", "../error_test_scripts" });
            RunSuccessfulCommand(checkoutArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);

            List<string> updateArgs = new List<string>() { "update" };
            updateArgs.AddRange(GetDestinationArgs());
            updateArgs.AddRange(new List<string>() { "-dir", "../error_test_scripts" });
            RunUnsuccessfulCommand(updateArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [SkippableFact]
        public void TestPartialUpdateWithImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "2" });
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void TestPartialUpdateShortOfImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(new List<string>() { "-r", "1" });
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [SkippableFact]
        public void TestImportWhenAtSourceRevision()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(checkoutArgs);

            List<string> updateArgs = new List<string>() { "update" };
            updateArgs.AddRange(GetDestinationArgs());
            updateArgs.AddRange(GetSourceArgs());
            updateArgs.AddRange(new List<string>() { "-sourceDb", SourceDatabaseName });
            RunSuccessfulCommand(updateArgs);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [SkippableFact]
        public void TestUpdateContinuesAfterImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(DatabaseNameFromScripts);
            CheckoutZeroOnScriptsDatabase();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgs());
            args.AddRange(GetSourceArgs());
            args.AddRange(new List<string>() { "-sourceDb", AltSourceDatabaseName });
            RunSuccessfulCommand(args);
            VerifyDatabase(DatabaseNameFromScripts, ExpectedAltSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }
    }
}
