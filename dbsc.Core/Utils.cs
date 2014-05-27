using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public static class Utils
    {
        /// <summary>
        /// Checks if an executable is on the user's PATH by running it, passing
        /// <paramref name="versionCommandLineOptions"/> as the command line options. If the process starts
        /// successfully, it's on the user's PATH.
        /// </summary>
        /// <param name="executable">Executable name, without .exe for compatibility with non-windows systems.</param>
        /// <param name="versionCommandLineOptions">Command line arguments that cause the executable to not do
        /// anything special. For example, "--version".</param>
        /// <returns></returns>
        public static bool ExecutableIsOnPath(string executable, string versionCommandLineOptions)
        {
            using (Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(executable, versionCommandLineOptions)
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false
                },
            })
            {
                try
                {
                    process.Start();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool RunningOnWindows()
        {
            return Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix;
        }

        /// <summary>
        /// Prints a message signalling the beginning of an import event, runs some code, then prints the time taken.
        /// </summary>
        /// <param name="beginningMessage">Do not put an ellipsis at the end</param>
        /// <param name="code"></param>
        public static void DoTimedOperation(string beginningMessage, Action code)
        {
            DoTimedOperationImpl(beginningMessage + "...", code);
        }

        public static void DoTimedOperationThatOuputsStuff(string beginningMessage, Action code)
        {
            DoTimedOperationImpl(beginningMessage + "..." + Environment.NewLine, code);
            Console.WriteLine();
        }

        private static void DoTimedOperationImpl(string beginningMessage, Action code)
        {
            Console.Write(beginningMessage);
            try
            {
                Stopwatch timer = Stopwatch.StartNew();
                code();
                timer.Stop();
                Console.WriteLine(timer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }
        }

        public static int ParseIntOption(string arg, string optionFriendlyName)
        {
            int value;
            if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new DbscOptionException(string.Format("{0} must be an integer.", optionFriendlyName));
            }
            return value;
        }

        public static string ReadPassword()
        {
            // If Console.ReadKey returns a ConsoleKeyInfo with KeyChar and Key of 0 on Mono, stdin or stdout is redirected.
            // stdout being redirected affecting the value of Console.ReadKey is a bug (https://bugzilla.xamarin.com/show_bug.cgi?id=12552).
            // Returning 0 when stdin is redirected is also a bug (https://bugzilla.xamarin.com/show_bug.cgi?id=12551).
            // The documented behavior is to throw an InvalidOperationException.

            bool runningOnMono = Type.GetType("Mono.Runtime") != null;
            StringBuilder textEntered = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key;
                try
                {
                    key = Console.ReadKey(intercept: true);
                }
                catch (InvalidOperationException)
                {
                    // Console.ReadKey throws InvalidOperationException if stdin is not a console
                    // .NET 4.5 provides Console.IsInputRedirected.
                    // Switch to 4.5 once Linux distros start packaging a Mono version that support 4.5.
                    throw new DbscOptionException("Cannot prompt for password because stdin is redirected.");
                }
                if (runningOnMono && key.KeyChar == '\0' && (int)key.Key == 0)
                {
                    throw new DbscOptionException("Cannot prompt for password because stdin is redirected.");
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (textEntered.Length > 0)
                    {
                        textEntered.Length = textEntered.Length - 1;
                    }
                }
                else
                {
                    char c = key.KeyChar;
                    textEntered.Append(c);
                }
            }

            return textEntered.ToString();
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