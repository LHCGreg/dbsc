using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using dbsc.Core;

namespace TestUtils.Sql
{
    public abstract class AbstractShowRevisionTestFixture<THelper> : SqlBaseTestFixture<THelper>
        where THelper : SqlTestHelper, new()
    {
        [Fact]
        public void TargetDbTakesPrecedenceOverCurrentDir()
        {
            // Test that targetDb takes precedence over scripts in current dir
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1}", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -targetDb {0} -u {1} -p {2}", TestDatabaseName, Username, Password), Helper.ScriptsForOtherDBDir, out stdout, out stderr);
            Assert.Equal("2", stdout.TrimEnd());
        }

        [Fact]
        public void PickupOfTargetDBFromScriptsInCurrentDir()
        {
            // Test the db name from scripts in current dir is used if targetDb not specified
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            Helper.RunSuccessfulCommand(string.Format("revision -u {0} -p {1}", Username, Password), out stdout, out stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }

        [Fact]
        public void PickupOfTargetDBFromScriptsInSpecifiedDir()
        {
            // Test that the db name from scripts in the directory specified with -dir is used if targetDb not specified
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -dir {0} -u {1} -p {2}", ScriptsDir.QuoteCommandLineArg(), Username, Password), Path.GetTempPath(), out stdout, out stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }

        [Fact]
        public void RunningWithoutScriptsWorks()
        {
            // Test that getting the revision works when specifying -targetDb even if there are no scripts in the current directory
            string stdout;
            string stderr;
            DropDatabase(TestDatabaseName);
            RunSuccessfulCommand(string.Format("checkout -u {0} -p {1} -r 1", Username, Password));
            ProcessUtils.RunSuccessfulCommand(DbscExePath, string.Format("revision -targetDb {0} -u {1} -p {2}", TestDatabaseName, Username, Password), Path.GetTempPath(), out stdout, out stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }
    }
}
