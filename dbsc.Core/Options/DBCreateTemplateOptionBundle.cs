using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;

namespace dbsc.Core.Options
{
    public class DBCreateTemplateOptionBundle : IOptionBundle
    {
        public string TemplatePath { get; private set; }

        /// <summary>
        /// Defaults to CREATE DATABASE $DatabaseName$
        /// </summary>
        public string Template { get; private set; }
        public string DefaultTemplate { get; private set; }

        /// <summary>
        /// Set this to change the message when displaying command line help.
        /// </summary>
        public string HelpMessage { get; set; }

        public static string DefaultSQLTemplate { get { return "CREATE DATABASE $DatabaseName$"; } }

        public DBCreateTemplateOptionBundle(string defaultTemplate)
        {
            DefaultTemplate = defaultTemplate;
            HelpMessage = string.Format(@"File with a template to use when creating the database in a checkout. $DatabaseName$ will be replaced with the database name. If not specified, a simple ""{0}"" will be used. This is a good place to set database options or grant permissions.", DefaultTemplate);
        }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("dbCreateTemplate|createDbTemplate=", HelpMessage, arg => TemplatePath = arg);
        }

        public void Validate()
        {
            ;
        }

        public void PostValidate()
        {
            if (TemplatePath == null)
            {
                Template = DefaultTemplate;
            }
            else
            {
                try
                {
                    Template = File.ReadAllText(TemplatePath);
                }
                catch (Exception ex)
                {
                    throw new DbscOptionException(string.Format("Error reading DB creation template {0}: {1}.", TemplatePath, ex.Message), ex);
                }
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