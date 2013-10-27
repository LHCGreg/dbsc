using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public static class ImportUtils
    {
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
    }
}
