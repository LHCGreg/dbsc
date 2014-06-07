using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUtils
{
    public static class ProcessUtils
    {
        public static int RunCommand(string exePath, string arguments, string workingDirectory)
        {
            string stdout;
            string stderr;
            return RunCommand(exePath, arguments, workingDirectory, out stdout, out stderr);
        }
        
        /// <summary>
        /// Runs a dbsc command using the given dbsc executable and argument string
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="arguments"></param>
        /// <returns>The return code of the process</returns>
        public static int RunCommand(string exePath, string arguments, string workingDirectory, out string stdout, out string stderr)
        {
            using (Process dbsc = new Process())
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    dbsc.StartInfo.FileName = "mono";
                    dbsc.StartInfo.Arguments = exePath + " " + arguments;
                }
                else
                {
                    dbsc.StartInfo.FileName = exePath;
                    dbsc.StartInfo.Arguments = arguments;
                }

                dbsc.StartInfo.RedirectStandardOutput = true;
                dbsc.StartInfo.RedirectStandardError = true;
                dbsc.StartInfo.UseShellExecute = false;
                dbsc.StartInfo.WorkingDirectory = workingDirectory;
                dbsc.StartInfo.CreateNoWindow = true;

                StringBuilder stdoutBuilder = new StringBuilder();
                StringBuilder stderrBuilder = new StringBuilder();

                dbsc.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); stdoutBuilder.AppendLine(e.Data); };
                dbsc.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); stderrBuilder.AppendLine(e.Data); };

                dbsc.Start();
                dbsc.BeginOutputReadLine();
                dbsc.BeginErrorReadLine();
                dbsc.WaitForExit();

                stdout = stdoutBuilder.ToString();
                stderr = stderrBuilder.ToString();

                return dbsc.ExitCode;
            }
        }

        public static void RunSuccessfulCommand(string exePath, string arguments, string workingDirectory)
        {
            int returnCode = RunCommand(exePath, arguments, workingDirectory);
            Assert.That(returnCode, Is.EqualTo(0));
        }

        public static void RunSuccessfulCommand(string exePath, string arguments, string workingDirectory, out string stdout, out string stderr)
        {
            int returnCode = RunCommand(exePath, arguments, workingDirectory, out stdout, out stderr);
            Assert.That(returnCode, Is.EqualTo(0));
        }

        public static void RunUnsuccessfulCommand(string exePath, string arguments, string workingDirectory)
        {
            int returnCode = RunCommand(exePath, arguments, workingDirectory);
            Assert.That(returnCode, Is.Not.EqualTo(0));
        }

        public static void RunUnsuccessfulCommand(string exePath, string arguments, string workingDirectory, out string stdout, out string stderr)
        {
            int returnCode = RunCommand(exePath, arguments, workingDirectory, out stdout, out stderr);
            Assert.That(returnCode, Is.Not.EqualTo(0));
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