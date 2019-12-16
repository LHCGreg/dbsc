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
            DropDatabase(TestDatabaseName);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            checkoutArgs.AddRange(new List<string>() { "-targetDb", TestDatabaseName });
            RunSuccessfulCommand(checkoutArgs);

            List<string> revisionArgs = new List<string>() { "revision" };
            checkoutArgs.AddRange(GetDestinationArgs());
            ProcessUtils.RunSuccessfulCommand(DbscExePath, revisionArgs, Helper.ScriptsForOtherDBDir, out string stdout, out string stderr);
            Assert.Equal("2", stdout.TrimEnd());
        }

        [Fact]
        public void PickupOfTargetDBFromScriptsInCurrentDir()
        {
            // Test the db name from scripts in current dir is used if targetDb not specified
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            RunSuccessfulCommand(checkoutArgs);

            List<string> revisionArgs = new List<string>() { "revision" };
            revisionArgs.AddRange(GetDestinationArgs());
            Helper.RunSuccessfulCommand(revisionArgs, out string stdout, out string stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }

        [Fact]
        public void PickupOfTargetDBFromScriptsInSpecifiedDir()
        {
            // Test that the db name from scripts in the directory specified with -dir is used if targetDb not specified
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            checkoutArgs.AddRange(new List<string>() { "-r", "1" });
            RunSuccessfulCommand(checkoutArgs);

            List<string> revisionArgs = new List<string>() { "revision" };
            revisionArgs.AddRange(GetDestinationArgs());
            revisionArgs.AddRange(new List<string>() { "-dir", ScriptsDir });
            ProcessUtils.RunSuccessfulCommand(DbscExePath, revisionArgs, Path.GetTempPath(), out string stdout, out string stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }

        [Fact]
        public void RunningWithoutScriptsWorks()
        {
            // Test that getting the revision works when specifying -targetDb even if there are no scripts in the current directory
            DropDatabase(DatabaseNameFromScripts);

            List<string> checkoutArgs = new List<string>() { "checkout" };
            checkoutArgs.AddRange(GetDestinationArgs());
            checkoutArgs.AddRange(new List<string>() { "-r", "1" });
            RunSuccessfulCommand(checkoutArgs);

            List<string> revisionArgs = new List<string>() { "revision" };
            revisionArgs.AddRange(GetDestinationArgs());
            revisionArgs.AddRange(new List<string>() { "-targetDb", TestDatabaseName });
            ProcessUtils.RunSuccessfulCommand(DbscExePath, revisionArgs, Path.GetTempPath(), out string stdout, out string stderr);
            Assert.Equal("1", stdout.TrimEnd());
        }
    }
}
