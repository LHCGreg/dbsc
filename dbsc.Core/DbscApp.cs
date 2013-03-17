using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class DbscApp<TCommandLineArgs, TCheckoutOptions, TUpdateOptions>
        where TCommandLineArgs : BaseCommandLineArgs
        where TCheckoutOptions : CheckoutOptions
        where TUpdateOptions : UpdateOptions
    {
        private Func<string[], TCommandLineArgs> m_parseArgsFunc;
        private Func<TCommandLineArgs, TCheckoutOptions> m_getCheckoutOptionsFunc;
        private Func<TCommandLineArgs, TUpdateOptions> m_getUpdateOptionsFunc;
        private DbscEngine m_engine;
        
        public DbscApp(DbscEngine engine, Func<string[], TCommandLineArgs> parseArgsFunc,
            Func<TCommandLineArgs, TCheckoutOptions> getCheckoutOptionsFunc, Func<TCommandLineArgs, TUpdateOptions> getUpdateOptionsFunc)
        {
            m_engine = engine;
            m_parseArgsFunc = parseArgsFunc;
            m_getCheckoutOptionsFunc = getCheckoutOptionsFunc;
            m_getUpdateOptionsFunc = getUpdateOptionsFunc;
        }

        public void Run(string[] args)
        {
            try
            {
                TCommandLineArgs commandLine = m_parseArgsFunc(args);
                if (commandLine.Operation == DbscOperation.Checkout)
                {
                    TCheckoutOptions options = m_getCheckoutOptionsFunc(commandLine);
                    m_engine.Checkout(options);
                }
                else if (commandLine.Operation == DbscOperation.Update)
                {
                    TUpdateOptions options = m_getUpdateOptionsFunc(commandLine);
                    m_engine.Update(options);
                }
                else
                {
                    throw new DbscException(string.Format("Oops, missed an operation: {0}", commandLine.Operation));
                }
            }
            catch (Exception ex)
            {
                StringBuilder errorMessage = new StringBuilder("Error");

                Exception exInChain = ex;
                while (exInChain != null)
                {
                    errorMessage.AppendFormat(": {0}", exInChain.Message);

                    exInChain = exInChain.InnerException;
                }

                Console.WriteLine(errorMessage.ToString());
                Environment.ExitCode = 1;
            }
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