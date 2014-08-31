using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.Options;

namespace dbsc.Core
{
    /// <summary>
    /// Serves as an entry point for the a dbsc application. An application's Main(string[] args)
    /// creates a DbscApp object and calls Run(args).
    /// </summary>
    /// <typeparam name="TCommandLineArgs"></typeparam>
    /// <typeparam name="TCheckoutSettings"></typeparam>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    /// <typeparam name="TUpdateSettings"></typeparam>
    public abstract class DbscApp<TCommandLineArgs, TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings>
        where TCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<TCheckoutSettings, TUpdateSettings>, new()
        where TConnectionSettings : IConnectionSettings
        where TCheckoutSettings : ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateSettings>
        where TImportSettings : IImportSettings<TConnectionSettings>
        where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
    {
        private DbscEngine<TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings> _engine;
        
        public DbscApp(DbscEngine<TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings> engine)
        {
            _engine = engine;
        }

        public void Run(string[] args)
        {
            TCommandLineArgs commandLine = new TCommandLineArgs();
            try
            {
                commandLine.Parse(args);
                if (commandLine.Operation == DbscOperation.Checkout)
                {
                    TCheckoutSettings options = commandLine.GetCheckoutSettings();
                    _engine.Checkout(options);
                }
                else if (commandLine.Operation == DbscOperation.Update)
                {
                    TUpdateSettings options = commandLine.GetUpdateSettings();
                    _engine.Update(options);
                }
                else if (commandLine.Operation == DbscOperation.Revision)
                {
                    TCheckoutSettings options = commandLine.GetCheckoutSettings();
                    _engine.ShowRevision(options);
                }
                else
                {
                    throw new DbscException(string.Format("Oops, missed an operation: {0}", commandLine.Operation));
                }
            }
            catch (DbscOptionException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.ShowHelp)
                {
                    commandLine.DisplayHelp(Console.Out);
                }
                Environment.ExitCode = 1;
            }
            catch (DbscException ex)
            {
                // DbscException has the error message already how it should be shown, no "Error:" or inner exception output needed.
                Console.WriteLine(ex.Message);
                Environment.ExitCode = 1;
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