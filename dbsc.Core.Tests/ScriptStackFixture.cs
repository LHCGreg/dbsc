using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using dbsc.Core;

namespace dbsc.Core.Tests
{
    public class ScriptStackFixture
    {
        // On Linux, C:\db.0000.sql is treated as a relative path.
        private string GetPrefix()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return "/foo/";
            }
            else
            {
                return @"C:\";
            }
        }

        [Fact]
        public void TestHappyPath()
        {
            string prefix = GetPrefix();

            List<string> filePaths = new List<string>()
            {
                prefix + "db.0000.sql",
                prefix + "db.0001.comment.sql",
                prefix + "db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.Equal("db", stack.MasterDatabaseName);
            Assert.Equal(prefix + "db.0000.sql", stack.ScriptsByRevision[0]);
            Assert.Equal(prefix + "db.0001.comment.sql", stack.ScriptsByRevision[1]);
            Assert.Equal(prefix + "db.02.SQL", stack.ScriptsByRevision[2]);
            Assert.Equal(3, stack.ScriptsByRevision.Count);
        }

        [Fact]
        public void NonSqlExtension()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix + "db.0000.js",
                prefix + "db.0001.comment.js",
                prefix + "db.02.JS"
            };

            ScriptStack stack = new ScriptStack(filePaths, "js");

            Assert.Equal("db", stack.MasterDatabaseName);
            Assert.Equal(prefix + "db.0000.js", stack.ScriptsByRevision[0]);
            Assert.Equal(prefix + "db.0001.comment.js", stack.ScriptsByRevision[1]);
            Assert.Equal(prefix + "db.02.JS", stack.ScriptsByRevision[2]);
            Assert.Equal(3, stack.ScriptsByRevision.Count);
        }

        [Fact]
        public void DifferentMasterDatabaseNameThrows()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix + "db.0000.sql",
                prefix + "DB.0001.comment.sql",
                prefix + "db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new ScriptStack(filePaths, "sql"));
        }

        [Fact]
        public void DifferentExtensionNotPickedUp()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix + "db.0000.sql",
                prefix + "db.0001.comment.sqlx",
                prefix + "db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.Equal(prefix + "db.0000.sql", stack.ScriptsByRevision[0]);
            Assert.Equal(prefix + "db.02.SQL", stack.ScriptsByRevision[2]);
            Assert.Equal(2, stack.ScriptsByRevision.Count);
        }

        [Fact]
        public void MultipleScriptsForSameRevisionThrows()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"db.0000.comment.sql",
                prefix+"db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new ScriptStack(filePaths, "sql"));
        }

        [Fact]
        public void NoZeroRevisionScriptThrows()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new ScriptStack(filePaths, "sql"));
        }

        [Fact]
        public void GapInRevisionsDoesNotThrow()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.False(stack.ScriptsByRevision.ContainsKey(1));
            Assert.Equal(prefix + "db.02.SQL", stack.ScriptsByRevision[2]);
        }
    }
}
