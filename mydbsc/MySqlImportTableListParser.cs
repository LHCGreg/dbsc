using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Antlr;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.MySql
{
    class MySqlImportTableListParser : IImportTableListParser<TableWithoutSchemaSpecificationCollection<MySqlTable>>
    {
        public TableWithoutSchemaSpecificationCollection<MySqlTable> Parse(TextReader input, string inputFileName)
        {
            TableSpecListParser parser = new TableSpecListParser();
            
            // Can't do custom selects when doing imports via mysqldump.
            // mysqldump does take a parameter allowing a custom WHERE clause, but we would have to either
            // use new special syntax or use existing syntax to mean something different for MySQL.
            bool allowCustomSelect = false;

            IList<TableWithSchemaSpecificationWithCustomSelect> result = parser.Parse(input, IdentifierSyntax.MySql, allowCustomSelect, inputFileName);

            List<TableWithoutSchemaSpecification> schemalessSpecs = result.Select(spec => new TableWithoutSchemaSpecification(spec.Table, spec.Negated)).ToList();
            return new TableWithoutSchemaSpecificationCollection<MySqlTable>(schemalessSpecs);
        }
    }
}

// Copyright (C) 2014 Greg Najda
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