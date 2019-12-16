using Xunit;
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
        public static int RunDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory)
        {
            string stdout;
            string stderr;
            return RunDbscCommand(exeDllPath, arguments, workingDirectory, out stdout, out stderr);
        }
        
        /// <summary>
        /// Runs a dbsc command using the given dbsc executable and argument string
        /// </summary>
        /// <param name="exeDllPath"></param>
        /// <param name="arguments"></param>
        /// <returns>The return code of the process</returns>
        public static int RunDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory, out string stdout, out string stderr)
        {
            using (Process dbsc = new Process())
            {
                dbsc.StartInfo.FileName = "dotnet";
                dbsc.StartInfo.ArgumentList.Add(exeDllPath);
                foreach (string arg in arguments)
                {
                    dbsc.StartInfo.ArgumentList.Add(arg);
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

        public static void RunSuccessfulDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory)
        {
            int returnCode = RunDbscCommand(exeDllPath, arguments, workingDirectory);
            Assert.Equal(0, returnCode);
        }

        public static void RunSuccessfulDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory, out string stdout, out string stderr)
        {
            int returnCode = RunDbscCommand(exeDllPath, arguments, workingDirectory, out stdout, out stderr);
            Assert.Equal(0, returnCode);
        }

        public static void RunUnsuccessfulDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory)
        {
            int returnCode = RunDbscCommand(exeDllPath, arguments, workingDirectory);
            Assert.NotEqual(0, returnCode);
        }

        public static void RunUnsuccessfulDbscCommand(string exeDllPath, IReadOnlyCollection<string> arguments, string workingDirectory, out string stdout, out string stderr)
        {
            int returnCode = RunDbscCommand(exeDllPath, arguments, workingDirectory, out stdout, out stderr);
            Assert.NotEqual(0, returnCode);
        }
    }
}
