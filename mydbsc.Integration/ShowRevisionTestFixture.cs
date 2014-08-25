﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TestUtils.Sql;

namespace dbsc.MySql.Integration
{
    [TestFixture]
    class ShowRevisionTestFixture : AbstractShowRevisionTestFixture<MySqlTestHelper>
    {
        protected override int? Port { get { return 3306; } }
        protected override bool ExtendedTableSpecsSupported { get { return true; } }
        protected override bool CustomSelectImportSupported { get { return false; } }
    }
}

// Copyright (C) 2014 Greg Najda
//
// This file is part of mydbsc.Integration.
//
// mydbsc.Integration is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc.Integration is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.Integration.  If not, see <http://www.gnu.org/licenses/>.