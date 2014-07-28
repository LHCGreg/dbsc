using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TestUtils.Sql;

namespace TestUtils.Sql
{
    [TestFixture]
    public abstract class AbstractCheckoutTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {        
        [Test]
        public void BasicTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void BasicImportTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyTwoCollectionsTest()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithCustomSelectTest()
        {
            IgnoreIfCustomSelectImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_custom_select.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeopleCustomSelect, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithOnlyNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_only_negations.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportWithWildcardsAndNegationsTest()
        {
            IgnoreIfExtendedTableSpecsNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import_wildcards_and_negations.txt",
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
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDbServer localhost -targetDb {2} {3} -sourceDbServer localhost {4} -sourceUsername {5} -sourcePassword {6}",
                Username, Password, AltTestDatabaseName, portArg, sourcePortArg, Username, Password));

            VerifyDatabase(AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestNonexistantTargetDbServer()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDbServer doesnotexist.local",
                Username, Password));
        }

        [Test]
        public void TestNonexistantTargetPort()
        {
            IgnoreIfPortNotSupported();

            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -port 9999",
                Username, Password));
        }

        [Test]
        public void TestNonExistantSourcePort()
        {
            IgnoreIfImportNotSupported();
            IgnoreIfPortNotSupported();
            
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourcePort 9999",
                Username, Password));
        }

        [Test]
        public void TestCreationTemplate()
        {
            IgnoreIfTemplateNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -dbCreateTemplate creation_template.sql",
                Username, Password));
            VerifyCreationTemplateRan(TestDatabaseName);
            VerifyDatabase(TestDatabaseName, ExpectedPeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -dir ../error_test_scripts",
                Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckout()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckoutWithImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 2 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestPartialCheckoutShortOfImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckoutWithTooHighRevision()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 3", Username, Password));
        }

        [Test]
        public void TestCheckoutContinuesAfterImport()
        {
            IgnoreIfImportNotSupported();

            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
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