using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.MySql;

namespace mydbsc.Tests
{
    [TestFixture]
    public class MySqlDbscEngineFixture
    {
        [Test]
        public void QuoteCommandLineArgWindowsTest()
        {
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@""), Is.EqualTo("\"\""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@"\"), Is.EqualTo(@"""\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@"C:\foo\bar\"), Is.EqualTo(@"""C:\foo\bar\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@"C:\""quote""\bar\\\\"), Is.EqualTo(@"""C:\\\""quote\""\bar\\\\\\\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@"\\""quote\bar"), Is.EqualTo(@"""\\\\\""quote\bar"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgWindows(@"\\foo\bar\baz"), Is.EqualTo(@"""\\foo\bar\baz"""));
        }

        [Test]
        public void QuoteCommandLineArgUnixTest()
        {
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@""), Is.EqualTo("\"\""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@"\"), Is.EqualTo(@"""\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@"C:\foo\bar\"), Is.EqualTo(@"""C:\\foo\\bar\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@"C:\""quote""\bar\\\\"), Is.EqualTo(@"""C:\\\""quote\""\\bar\\\\\\\\"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@"\\""quote\bar"), Is.EqualTo(@"""\\\\\""quote\\bar"""));
            Assert.That(MySqlDbscEngine.QuoteCommandLineArgUnix(@"\\foo\bar\baz"), Is.EqualTo(@"""\\\\foo\\bar\\baz"""));
        }
    }
}

// Copyright (C) 2013 Greg Najda
//
// This file is part of mydbsc.Tests.
//
// mydbsc.Tests is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc.Tests is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.Tests.  If not, see <http://www.gnu.org/licenses/>.