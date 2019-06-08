using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Tests
{
    public class StringExtensionsFixture
    {
        [Fact]
        public void QuoteCommandLineArgWindowsTest()
        {
            Assert.Equal("\"\"", @"".QuoteCommandLineArgWindows());
            Assert.Equal(@"""\\""", @"\".QuoteCommandLineArgWindows());
            Assert.Equal(@"""C:\foo\bar\\""", @"C:\foo\bar\".QuoteCommandLineArgWindows());
            Assert.Equal(@"""C:\\\""quote\""\bar\\\\\\\\""", @"C:\""quote""\bar\\\\".QuoteCommandLineArgWindows());
            Assert.Equal(@"""\\\\\""quote\bar""", @"\\""quote\bar".QuoteCommandLineArgWindows());
            Assert.Equal(@"""\\foo\bar\baz""", @"\\foo\bar\baz".QuoteCommandLineArgWindows());
        }

        [Fact]
        public void QuoteCommandLineArgUnixTest()
        {
            Assert.Equal("\"\"", @"".QuoteCommandLineArgUnix());
            Assert.Equal(@"""\\""", @"\".QuoteCommandLineArgUnix());
            Assert.Equal(@"""C:\\foo\\bar\\""", @"C:\foo\bar\".QuoteCommandLineArgUnix());
            Assert.Equal(@"""C:\\\""quote\""\\bar\\\\\\\\""", @"C:\""quote""\bar\\\\".QuoteCommandLineArgUnix());
            Assert.Equal(@"""\\\\\""quote\\bar""", @"\\""quote\bar".QuoteCommandLineArgUnix());
            Assert.Equal(@"""\\\\foo\\bar\\baz""", @"\\foo\bar\baz".QuoteCommandLineArgUnix());
        }
    }
}
