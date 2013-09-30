using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Tests
{
    [TestFixture]
    public class StringExtensionsFixture
    {
        [Test]
        public void QuoteCommandLineArgWindowsTest()
        {
            Assert.That(@"".QuoteCommandLineArgWindows(), Is.EqualTo("\"\""));
            Assert.That(@"\".QuoteCommandLineArgWindows(), Is.EqualTo(@"""\\"""));
            Assert.That(@"C:\foo\bar\".QuoteCommandLineArgWindows(), Is.EqualTo(@"""C:\foo\bar\\"""));
            Assert.That(@"C:\""quote""\bar\\\\".QuoteCommandLineArgWindows(), Is.EqualTo(@"""C:\\\""quote\""\bar\\\\\\\\"""));
            Assert.That(@"\\""quote\bar".QuoteCommandLineArgWindows(), Is.EqualTo(@"""\\\\\""quote\bar"""));
            Assert.That(@"\\foo\bar\baz".QuoteCommandLineArgWindows(), Is.EqualTo(@"""\\foo\bar\baz"""));
        }

        [Test]
        public void QuoteCommandLineArgUnixTest()
        {
            Assert.That(@"".QuoteCommandLineArgUnix(), Is.EqualTo("\"\""));
            Assert.That(@"\".QuoteCommandLineArgUnix(), Is.EqualTo(@"""\\"""));
            Assert.That(@"C:\foo\bar\".QuoteCommandLineArgUnix(), Is.EqualTo(@"""C:\\foo\\bar\\"""));
            Assert.That(@"C:\""quote""\bar\\\\".QuoteCommandLineArgUnix(), Is.EqualTo(@"""C:\\\""quote\""\\bar\\\\\\\\"""));
            Assert.That(@"\\""quote\bar".QuoteCommandLineArgUnix(), Is.EqualTo(@"""\\\\\""quote\\bar"""));
            Assert.That(@"\\foo\bar\baz".QuoteCommandLineArgUnix(), Is.EqualTo(@"""\\\\foo\\bar\\baz"""));
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