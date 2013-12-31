using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace dbsc.Mongo.Integration
{
    [TestFixture]
    class CheckoutTestFixture : BaseTestFixture
    {
        [Test]
        public void BasicTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void BasicTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccessfulCommand("checkout -port 30017 -u useradmin -p testpw");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void BasicImportTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void BasicImportTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyOneCollectionTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>(), expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyOneCollectionWithAuthTest()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>(), expectedVersion: 2);
        }

        [Test]
        public void TestTargetDb()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccessfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            // Then import from the main test database into the alt test database
            RunSuccessfulCommand(string.Format("checkout -targetDbServer localhost -targetDb {0} -port 27017 -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void TestTargetDbWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            DropDatabaseOnAuthMongo(AltTestDatabaseName);

            RunSuccessfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            RunSuccessfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -targetDbServer localhost -targetDb {0} -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabaseOnAuthMongo(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void TestNonexistantTargetDbServer()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -targetDbServer doesnotexist.local");
        }

        [Test]
        public void TestNonexistantTargetPort()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -port 9999");
        }

        [Test]
        public void TestNonExistantSourcePort()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -sourceDbServer localhost -sourcePort 9999");
        }

        [Test]
        public void TestCreationTemplate()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout -dbCreateTemplate creation_template.js");
            List<Book> expectedBooksWithCreationTemplate = ExpectedBooks.Concat(BooksFromCreationTemplate).ToList();
            VerifyDatabase(TestDatabaseName, expectedBooksWithCreationTemplate, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void TestCreationTemplateWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccessfulCommand("checkout -port 30017 -u useradmin -p testpw -dbCreateTemplate creation_template.js");
            List<Book> expectedBooksWithCreationTemplate = ExpectedBooks.Concat(BooksFromCreationTemplate).ToList();
            VerifyDatabaseOnAuthMongo(TestDatabaseName, expectedBooksWithCreationTemplate, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>(), expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckout()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout -r 1");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>(), expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckoutWithImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -r 2 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
        }

        [Test]
        public void TestPartialCheckoutShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -r 1 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>(), expectedVersion: 1);
        }

        [Test]
        public void TestPartialCheckoutWithTooHighRevision()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -r 3");
        }

        [Test]
        public void TestCheckoutWithoutAuthWhenAuthRequiredFails()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -port 30017");
        }

        [Test]
        public void TestCheckoutContinuesAfterImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -sourceDbServer localhost -sourceDb {0}", AltSourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedAltSourceBooks, ExpectedPeople, ExpectedNumbers, expectedVersion: 2);
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