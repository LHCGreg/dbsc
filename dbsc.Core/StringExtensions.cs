using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dbsc.Core
{
    public static class StringExtensions
    {
        public static string QuoteCommandLineArg(this string arg)
        {
            if (Utils.RunningOnWindows())
            {
                return QuoteCommandLineArgWindows(arg);
            }
            else
            {
                return QuoteCommandLineArgUnix(arg);
            }
        }

        public static string QuoteCommandLineArgWindows(this string arg)
        {
            // If a double quotation mark follows two or an even number of backslashes,
            // each proceeding backslash pair is replaced with one backslash and the double quotation mark is removed.
            // If a double quotation mark follows an odd number of backslashes, including just one,
            // each preceding pair is replaced with one backslash and the remaining backslash is removed;
            // however, in this case the double quotation mark is not removed. 
            // - http://msdn.microsoft.com/en-us/library/system.environment.getcommandlineargs.aspx
            //
            // Windows command line processing is funky

            string escapedArg;
            Regex backslashSequenceBeforeQuotes = new Regex(@"(\\+)""");
            // Double \ sequences before "s, Replace " with \", double \ sequences at end
            escapedArg = backslashSequenceBeforeQuotes.Replace(arg, (match) => new string('\\', match.Groups[1].Length * 2) + "\"");
            escapedArg = escapedArg.Replace("\"", @"\""");
            Regex backslashSequenceAtEnd = new Regex(@"(\\+)$");
            escapedArg = backslashSequenceAtEnd.Replace(escapedArg, (match) => new string('\\', match.Groups[1].Length * 2));
            // C:\blah\"\\
            // "C:\blah\\\"\\\\"
            escapedArg = "\"" + escapedArg + "\"";
            return escapedArg;
        }

        public static string QuoteCommandLineArgUnix(this string arg)
        {
            // Mono uses the GNOME g_shell_parse_argv() function to convert the arg string into an argv
            // Just prepend " and \ with \ and enclose in quotes.
            // Much simpler than Windows!

            Regex backslashOrQuote = new Regex(@"\\|""");
            return "\"" + backslashOrQuote.Replace(arg, (match) => @"\" + match.ToString()) + "\"";
        }
    }
}

/*
 Copyright 2014 Greg Najda

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