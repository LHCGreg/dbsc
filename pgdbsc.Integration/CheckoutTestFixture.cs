using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Postgres.Integration
{
    [TestFixture]
    class CheckoutTestFixture : BaseTestFixture
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
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyTwoCollectionsTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, people => new List<Book>(), ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestTargetDb()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));

            // Then import from the main test database into the alt test database
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -targetDbServer localhost -targetDb {2} -port 5432 -sourceDbServer localhost -sourcePort 5432 -sourceUsername {3} -sourcePassword {4}",
                Username, Password, AltTestDatabaseName, Username, Password));

            VerifyDatabase(AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
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
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -port 9999",
                Username, Password));
        }

        [Test]
        public void TestNonExistantSourcePort()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourcePort 9999",
                Username, Password));
        }

        [Test]
        public void TestCreationTemplate()
        {
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
            RunUnsuccessfulCommand(string.Format("checkout -u {0} -p{1} -dir ../error_test_scripts",
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
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 2 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestPartialCheckoutShortOfImport()
        {
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
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, AltSourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedAltSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
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