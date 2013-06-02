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
