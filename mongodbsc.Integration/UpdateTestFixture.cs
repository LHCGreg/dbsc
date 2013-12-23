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
            RunSuccessfulCommand("update");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccessfulCommand("update -port 30017 -u useradmin -p testpw");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void PartialUpdateTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand("update -r 1");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, new List<Number>());
        }

        [Test]
        public void UpdateNothingTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
            RunSuccessfulCommand("update");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void UpdateNothingTestWithExplicitRevision()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
            RunSuccessfulCommand("update -r 2");
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
            RunSuccessfulCommand(string.Format("update -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicImportTestWithAuth()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccessfulCommand(string.Format("update -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void ImportOnlyOneCollectionTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void ImportOnlyOneCollectionWithAuthTest()
        {
            DropDatabaseOnAuthMongo(TestDatabaseName);
            CheckoutZeroOnAuthMongo();
            RunSuccessfulCommand(string.Format("update -port 30017 -u useradmin -p testpw -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void TestTargetDb()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccessfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            // Then import from the main test database into the alt test database
            CheckoutZeroOnAltDatabase();
            RunSuccessfulCommand(string.Format("update -targetDbServer localhost -targetDb {0} -port 27017 -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand("checkout -r 0 -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
            RunUnsuccessfulCommand("update -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialUpdateWithImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -r 2 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestPartialUpdateShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -r 1 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestImportWhenAtSourceRevision()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout"));
            RunSuccessfulCommand(string.Format("update -sourceDbServer localhost -sourceDb {0}",
                SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        private void CheckoutZero()
        {
            RunSuccessfulCommand("checkout -r 0");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
        }

        private void CheckoutZeroOnAuthMongo()
        {
            RunSuccessfulCommand("checkout -port 30017 -u useradmin -p testpw -r 0");
            VerifyDatabaseOnAuthMongo(TestDatabaseName, ExpectedBooks, new List<Person>(), new List<Number>());
        }

        private void CheckoutZeroOnAltDatabase()
        {
            RunSuccessfulCommand(string.Format("checkout -r 0 -targetDb {0}", AltTestDatabaseName));
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