using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo.Integration
{
    [TestFixture]
    class UpdateTestFixture : BaseTestFixture
    {
        [Test]
        public void BasicTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand("update");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccesfulCommand("update -port 30017 -u useradmin -p testpw");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void PartialUpdateTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand("update -r 1");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, new List<Number>());
        }

        [Test]
        public void UpdateNothingTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
            RunSuccesfulCommand("update");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void UpdateNothingTestWithExplicitRevision()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
            RunSuccesfulCommand("update -r 2");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestPartialUpdateWithTooHighRevision()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunUnsuccessfulCommand("update -r 3");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void BasicImportTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand(string.Format("update -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicImportTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccesfulCommand(string.Format("update -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void ImportOnlyOneCollectionTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand(string.Format("update -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void ImportOnlyOneCollectionWithAuthTest()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccesfulCommand(string.Format("update -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
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
            CheckoutZeroOnAltDatabase();
            RunSuccesfulCommand(string.Format("update -targetDbServer localhost -targetDb {0} -port 27017 -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout -r 0 -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
            RunUnsuccessfulCommand("update -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialUpdateWithImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand(string.Format("update -r 2 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestPartialUpdateShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccesfulCommand(string.Format("update -r 1 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        private void CheckoutZero()
        {
            RunSuccesfulCommand("checkout -r 0");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
        }

        private void CheckoutZeroOnAuthMongo()
        {
            RunSuccesfulCommand("checkout -port 30017 -u useradmin -p testpw -r 0");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
        }

        private void CheckoutZeroOnAltDatabase()
        {
            RunSuccesfulCommand(string.Format("checkout -r 0 -targetDb {0}", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
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