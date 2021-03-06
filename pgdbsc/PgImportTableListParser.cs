﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Antlr;
using System.IO;
using dbsc.Core;

namespace dbsc.Postgres
{
    class PgImportTableListParser : IImportTableListParser<TableWithSchemaSpecificationWithCustomSelectCollection<PgTable>>
    {
        public PgImportTableListParser()
        {
            ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        /// <exception cref="dbsc.Core.TableSpecificationParseException"></exception>
        public TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> Parse(TextReader input, string inputFileName)
        {
            TableSpecListParser parser = new TableSpecListParser();
            bool allowCustomSelect = true;
            IList<TableWithSchemaSpecificationWithCustomSelect> result = parser.Parse(input, IdentifierSyntax.Postgres, allowCustomSelect, inputFileName);
            return new TableWithSchemaSpecificationWithCustomSelectCollection<PgTable>(result, "public");
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