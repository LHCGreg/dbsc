using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.Core;

namespace dbsc.Core.Tests
{
    [TestFixture]
    public class SqlStackFixture
    {
        [Test]
        public void TestHappyPath()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\db.comment.0001.sql",
                @"C:\db.02.SQL"
            };

            SqlStack stack = new SqlStack(filePaths);
            Assert.That(stack.MasterDatabaseName, Is.EqualTo("db"));
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(@"C:\db.0000.sql"));
            Assert.That(stack.ScriptsByRevision[1], Is.EqualTo(@"C:\db.comment.0001.sql"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(@"C:\db.02.SQL"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(3));
        }

        [Test]
        public void DifferentMasterDatabaseNameThrows()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\DB.comment.0001.sql",
                @"C:\db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new SqlStack(filePaths));
        }

        [Test]
        public void MultipleScriptsForSameRevisionThrows()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\db.comment.0000.sql",
                @"C:\db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new SqlStack(filePaths));
        }

        [Test]
        public void NoZeroRevisionScriptThrows()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new SqlStack(filePaths));
        }

        [Test]
        public void GapInRevisionsDoesNotThrow()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\db.02.SQL"
            };

            SqlStack stack = new SqlStack(filePaths);
            Assert.That(stack.ScriptsByRevision.ContainsKey(1), Is.False);
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(@"C:\db.02.SQL"));
        }
    }
}
