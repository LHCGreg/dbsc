using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TestUtils.Sql
{
    [TestFixture]
    public abstract class AbstractUpdateTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        private void CheckoutZero()
        {
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 0", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        private void CheckoutZeroOnAltDatabase()
        {
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 0 -targetDb {2}",
                Username, Password, AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        [Test]
        public void BasicTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1}", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void PartialUpdateTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 1", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void UpdateNothingTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
            RunSuccessfulCommand(string.Format("update -u {0} -p {1}", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void UpdateNothingTestWithExplicitRevision()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 2", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestPartialUpdateWithTooHighRevision()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunUnsuccessfulCommand(string.Format("update -u {0} -p {1} -r 3", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        [Test]
        public void BasicImportTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyTwoCollectionsTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithCustomSelectTest()
        {
            IgnoreIfCustomSelectImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_custom_select.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeopleCustomSelect, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithOnlyNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_only_negations.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithWildcardsAndNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_wildcards_and_negations.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestTargetDb()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));

            string portArg = "";
            string sourcePortArg = "";
            if (Port != null)
            {
                portArg = string.Format("-port {0}", Port);
                sourcePortArg = string.Format("-sourcePort {0}", Port);
            }

            // Then import from the main test database into the alt test database
            CheckoutZeroOnAltDatabase();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -targetDbServer localhost -targetDb {2} {3} -sourceDbServer localhost {4} -sourceUsername {5} -sourcePassword {6}",
                Username, Password, AltTestDatabaseName, portArg, sourcePortArg, Username, Password));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 0 -dir ../error_test_scripts",
                Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
            RunUnsuccessfulCommand(string.Format("update -u {0} -p {1} -dir ../error_test_scripts",
                Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestPartialUpdateWithImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 2 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestPartialUpdateShortOfImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 1 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestImportWhenAtSourceRevision()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestUpdateContinuesAfterImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, AltSourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedAltSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
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