using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using dbsc.Core;
using TestUtils.Sql;
using Xunit;

namespace dbsc.Postgres.Integration
{
    public class UpdateTestFixture : AbstractUpdateTestFixture<PgTestHelper>
    {
        protected override bool ExtendedTableSpecsSupported { get { return true; } }
        protected override bool CustomSelectImportSupported { get { return true; } }

        [Fact]
        public void TestIntegratedSecurity()
        {
            Skip.IfNot(Utils.RunningOnWindows(), "Not running on Windows, skipping integrated security test.");
            
            DropDatabaseWithIntegratedSecurity(Helper.IntegratedSecurityTargetDatabaseName);
            CheckoutZeroWithIntegratedSecurity();

            List<string> args = new List<string>() { "update" };
            args.AddRange(GetDestinationArgsWithIntegratedSecurity());
            args.AddRange(GetSourceArgsWithIntegratedSecurity());

            RunSuccessfulCommand(args);
            VerifyDatabaseWithIntegratedSecurity(Helper.IntegratedSecurityTargetDatabaseName, ExpectedSourcePeople, GetExpectedBooksFunc, ExpectedSourceIsolationTestValues, expectedVersion: 2);
        }

        private List<string> GetDestinationArgsWithIntegratedSecurity()
        {
            List<string> args = GetDestinationArgs();
            int indexOfPasswordArg = args.IndexOf("-p");
            args.RemoveAt(indexOfPasswordArg + 1);
            args.RemoveAt(indexOfPasswordArg);
            args.Add("-SSPI");
            return args;
        }

        private List<string> GetSourceArgsWithIntegratedSecurity()
        {
            List<string> args = GetSourceArgs();
            int indexOfSourcePasswordArg = args.IndexOf("-sourcePassword");
            args.RemoveAt(indexOfSourcePasswordArg + 1);
            args.RemoveAt(indexOfSourcePasswordArg);
            args.Add("-sourceSSPI");
            return args;
        }

        private void CheckoutZeroWithIntegratedSecurity()
        {
            List<string> args = new List<string>() { "checkout" };
            args.AddRange(GetDestinationArgsWithIntegratedSecurity());
            args.AddRange(new List<string>() { "-r", "0" });
            RunSuccessfulCommand(args);
            VerifyDatabaseWithIntegratedSecurity(Helper.IntegratedSecurityTargetDatabaseName, ExpectedRevision0People, null, ExpectedRevision0IsolationTestValues, expectedVersion: 0);
        }

        private void VerifyDatabaseWithIntegratedSecurity(string dbName, List<Person> expectedPeople, Func<List<Person>, List<Book>> getExpectedBooks,
            List<script_isolation_test> expectedIsolationTestValues, int expectedVersion)
        {
            Func<IDbConnection> getDbConnectionWithIntegratedSecurity = () => Helper.GetDbConnection(TestDatabaseHost, TestDatabasePort, TestDatabaseUsername, password: null, dbName: dbName);
            VerifyDatabase(getDbConnectionWithIntegratedSecurity, expectedPeople, getExpectedBooks, expectedIsolationTestValues, expectedVersion);
        }

        private void DropDatabaseWithIntegratedSecurity(string dbName)
        {
            Func<string, IDbConnection> getDbConnectionWithIntegratedSecurity = databaseName => Helper.GetDbConnection(TestDatabaseHost, TestDatabasePort, TestDatabaseUsername, password: null, dbName: databaseName);
            DropDatabase(dbName, getDbConnectionWithIntegratedSecurity);
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