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
                @"C:\db.0001.comment.sql",
                @"C:\db.02.SQL"
            };

            SqlStack stack = new SqlStack(filePaths);
            Assert.That(stack.MasterDatabaseName, Is.EqualTo("db"));
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(@"C:\db.0000.sql"));
            Assert.That(stack.ScriptsByRevision[1], Is.EqualTo(@"C:\db.0001.comment.sql"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(@"C:\db.02.SQL"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(3));
        }

        [Test]
        public void DifferentMasterDatabaseNameThrows()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\DB.0001.comment.sql",
                @"C:\db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new SqlStack(filePaths));
        }

        [Test]
        public void DifferentExtensionNotPickedUp()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\db.0001.comment.sqlx",
                @"C:\db.02.SQL"
            };

            SqlStack stack = new SqlStack(filePaths);
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(@"C:\db.0000.sql"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(@"C:\db.02.SQL"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(2));
        }

        [Test]
        public void MultipleScriptsForSameRevisionThrows()
        {
            List<string> filePaths = new List<string>()
            {
                @"C:\db.0000.sql",
                @"C:\db.0000.comment.sql",
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