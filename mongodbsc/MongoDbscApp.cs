﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.Mongo
{
    class MongoDbscApp : DbscApp<MongoCommandLineArgs, MongoCheckoutOptions, MongoUpdateOptions>
    {
        public MongoDbscApp()
            : base(
            engine: new MongoDbscEngine(),
            parseArgsFunc: args => { MongoCommandLineArgs commandLine = new MongoCommandLineArgs(); commandLine.Parse(args); return commandLine; },
            getCheckoutOptionsFunc: commandLine => commandLine.GetCheckoutOptions(),
            getUpdateOptionsFunc: commandLine => commandLine.GetUpdateOptions()
            )
        {
            ;
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