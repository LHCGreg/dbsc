using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TestUtils.Sql
{
    [TestFixture]
    public abstract class AbstractUpdateTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        protected THelper Helper { get; private set; }

        protected abstract int? Port { get; }

        public AbstractUpdateTestFixture()
        {
            Helper = new THelper();
        }

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
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -targetDb {2} -sourceDbServer localhost -sourceDb {3} -sourceUsername {4} -sourcePassword {5}",
                Username, Password, TestDatabaseName, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void ImportOnlyTwoCollectionsTest()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4} -importTableList tables_to_import.txt",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestTargetDb()
        {
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
            VerifyDatabase(AltTestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
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
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 2 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestPartialUpdateShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -r 1 -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedRevision1People, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 1);
        }

        [Test]
        public void TestImportWhenAtSourceRevision()
        {
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, SourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        [Test]
        public void TestUpdateContinuesAfterImport()
        {
            DropDatabase(TestDatabaseName);
            CheckoutZero();
            RunSuccessfulCommand(string.Format("update -u {0} -p {1} -sourceDbServer localhost -sourceDb {2} -sourceUsername {3} -sourcePassword {4}",
                Username, Password, AltSourceDatabaseName, Username, Password));
            VerifyDatabase(TestDatabaseName, ExpectedAltSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
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
        protected List<Person> ExpectedAltSourcePeople { get { return Helper.ExpectedAltSourcePeople; } }
        protected List<Person> ExpectedRevision0People { get { return Helper.ExpectedRevision0People; } }
        protected List<Person> ExpectedRevision1People { get { return Helper.ExpectedRevision1People; } }
        protected Func<List<Person>, List<Book>> GetExpectedBooksFunc { get { return Helper.GetExpectedBooksFunc; } }
        protected List<script_isolation_test> ExpectedIsolationTestValues { get { return Helper.ExpectedIsolationTestValues; } }
        protected List<script_isolation_test> ExpectedRevision0IsolationTestValues { get { return Helper.ExpectedRevision0IsolationTestValues; } }

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