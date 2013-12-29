using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.Core;

namespace dbsc.Core.Tests
{
    [TestFixture]
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
                return  @"C:\";
            }
        }
        
        [Test]
        public void TestHappyPath()
        {
            string prefix = GetPrefix();

            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"db.0001.comment.sql",
                prefix+"db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.That(stack.MasterDatabaseName, Is.EqualTo("db"));
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(prefix+"db.0000.sql"));
            Assert.That(stack.ScriptsByRevision[1], Is.EqualTo(prefix+"db.0001.comment.sql"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(prefix+"db.02.SQL"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(3));
        }

        [Test]
        public void NonSqlExtension()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.js",
                prefix+"db.0001.comment.js",
                prefix+"db.02.JS"
            };

            ScriptStack stack = new ScriptStack(filePaths, "js");
            Assert.That(stack.MasterDatabaseName, Is.EqualTo("db"));
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(prefix+"db.0000.js"));
            Assert.That(stack.ScriptsByRevision[1], Is.EqualTo(prefix+"db.0001.comment.js"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(prefix+"db.02.JS"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(3));
        }

        [Test]
        public void DifferentMasterDatabaseNameThrows()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"DB.0001.comment.sql",
                prefix+"db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new ScriptStack(filePaths, "sql"));
        }

        [Test]
        public void DifferentExtensionNotPickedUp()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"db.0001.comment.sqlx",
                prefix+"db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.That(stack.ScriptsByRevision[0], Is.EqualTo(prefix+"db.0000.sql"));
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(prefix+"db.02.SQL"));
            Assert.That(stack.ScriptsByRevision.Count, Is.EqualTo(2));
        }

        [Test]
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

        [Test]
        public void NoZeroRevisionScriptThrows()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.02.SQL"
            };

            Assert.Throws<DbscException>(() => new ScriptStack(filePaths, "sql"));
        }

        [Test]
        public void GapInRevisionsDoesNotThrow()
        {
            string prefix = GetPrefix();
            List<string> filePaths = new List<string>()
            {
                prefix+"db.0000.sql",
                prefix+"db.02.SQL"
            };

            ScriptStack stack = new ScriptStack(filePaths, "sql");
            Assert.That(stack.ScriptsByRevision.ContainsKey(1), Is.False);
            Assert.That(stack.ScriptsByRevision[2], Is.EqualTo(prefix+"db.02.SQL"));
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