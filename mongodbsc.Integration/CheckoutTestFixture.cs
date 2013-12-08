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
            RunSuccesfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccesfulCommand("checkout -port 30017 -u useradmin -p testpw");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicImportTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicImportTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void ImportOnlyOneCollectionTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void ImportOnlyOneCollectionWithAuthTest()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void TestTargetDb()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccesfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            // Then import from the main test database into the alt test database
            RunSuccesfulCommand(string.Format("checkout -targetDbServer localhost -targetDb {0} -port 27017 -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestTargetDbWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            DropDatabaseOnAuthMongo(AltTestDatabaseName);

            RunSuccesfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            RunSuccesfulCommand(string.Format("checkout -port 30017 -u useradmin -p testpw -targetDbServer localhost -targetDb {0} -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabaseOnAuthMongo(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
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
            DropDatabase(CreationTemplateDatabaseName);
            RunSuccesfulCommand("checkout -dbCreateTemplate creation_template.js");
            VerifyCreationTemplateDatabase();
        }

        [Test]
        public void TestCreationTemplateWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            DropDatabaseOnAuthMongo(CreationTemplateDatabaseName);
            RunSuccesfulCommand("checkout -port 30017 -u useradmin -p testpw -dbCreateTemplate creation_template.js");
            VerifyCreationTemplateDatabaseOnAuthMongo();
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialCheckout()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout -r 1");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialCheckoutWithImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -r 2 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestPartialCheckoutShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -r 1 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
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