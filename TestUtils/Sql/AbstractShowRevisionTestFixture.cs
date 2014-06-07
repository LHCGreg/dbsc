using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.Core;

namespace TestUtils.Sql
{
    [TestFixture]
    public abstract class AbstractShowRevisionTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        [Test]
        public void TargetDbTakesPrecedenceOverCurrentDir()
        {
            // Test that targetDb takes precedence over scripts in current dir
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -targetDb {0} -u {1} -p {2}", TestDatabaseName, Username, Password), Helper.ScriptsForOtherDBDir, out stdout, out stderr);
            Assert.That(stdout.TrimEnd(), Is.EqualTo("2"));
        }

        [Test]
        public void PickupOfTargetDBFromScriptsInCurrentDir()
        {
            // Test the db name from scripts in current dir is used if targetDb not specified
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            Helper.RunSuccessfulCommand(string.Format("revision -u {0} -p {1}", Username, Password), out stdout, out stderr);
            Assert.That(stdout.TrimEnd(), Is.EqualTo("1"));
        }

        [Test]
        public void PickupOfTargetDBFromScriptsInSpecifiedDir()
        {
            // Test that the db name from scripts in the directory specified with -dir is used if targetDb not specified
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -dir {0} -u {1} -p {2}", ScriptsDir.QuoteCommandLineArg(), Username, Password), Path.GetTempPath(), out stdout, out stderr);
            Assert.That(stdout.TrimEnd(), Is.EqualTo("1"));
        }

        [Test]
        public void RunningWithoutScriptsWorks()
        {
            // Test that getting the revision works when specifying -targetDb even if there are no scripts in the current directory
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -targetDb {0} -u {1} -p {2}", TestDatabaseName, Username, Password), Path.GetTempPath(), out stdout, out stderr);
            Assert.That(stdout.TrimEnd(), Is.EqualTo("1"));
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