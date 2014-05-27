using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestUtils.Sql;

namespace dbsc.SqlServer.Integration
{
    [TestFixture]
    public class CheckoutTestFixture : AbstractCheckoutTestFixture<MSTestHelper>
    {
        protected override int? Port { get { return null; } }

        [Test]
        public void TestIntegratedSecurity()
        {
            DropDatabaseWithIntegratedSecurity(Helper.IntegratedSecurityTargetDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}",
                Helper.IntegratedSecurityTargetDatabaseName, SourceDatabaseName));
            VerifyDatabaseWithIntegratedSecurity(Helper.IntegratedSecurityTargetDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedIsolationTestValues, expectedVersion: 2);
        }

        private void DropDatabaseWithIntegratedSecurity(string dbName)
        {
            DropDatabase(dbName, databaseName => Helper.GetDbConnectionWithIntegratedSecurity(databaseName));
        }

        private void VerifyDatabaseWithIntegratedSecurity(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            VerifyDatabase(dbName, expectedPeople, getExpectedBooks,
                expectedIsolationTestValues, expectedVersion, databaseName => Helper.GetDbConnectionWithIntegratedSecurity(databaseName));
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