using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using MySql.Data.MySqlClient;

namespace dbsc.MySql
{
    class MySqlDbscApp : DbscApp<BaseCommandLineArgs, SqlCheckoutOptions, SqlUpdateOptions>
    {
        public MySqlDbscApp()
            : base(
            engine: new MySqlDbscEngine(),
            parseArgsFunc: args => { BaseCommandLineArgs commandLine = new BaseCommandLineArgs(); commandLine.Parse(args); return commandLine; },
            getCheckoutOptionsFunc: commandLine => commandLine.GetCheckoutOptions(),
            getUpdateOptionsFunc: commandLine => commandLine.GetUpdateOptions()
            )
        {
            ;
        }
    }
}

// Copyright (C) 2013 Greg Najda
//
// This file is part of mydbsc.
//
// mydbsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.  If not, see <http://www.gnu.org/licenses/>.