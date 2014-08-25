﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Core.Antlr
{
    /// <summary>
    /// Parses a table specification file that can contain negations, wildcards, and custom SELECTs.
    /// </summary>
    public class TableSpecListParser
    {
        public TableSpecListParser()
        {

        }

        /// <summary>
        /// If the flavor does not support schemas, all rules returned will simply not have a schema.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputFileName">Used for error messages</param>
        /// <returns></returns>
        /// <exception cref="dbsc.Core.TableSpecificationParseException"></exception>
        public IList<TableWithSchemaSpecificationWithCustomSelect> Parse(TextReader input, IdentifierSyntax flavor, bool allowCustomSelect, string inputFileName)
        {
            string text = input.ReadToEnd();
            IList<TableWithSchemaSpecificationWithCustomSelect> specs = Parse(text, flavor, allowCustomSelect, inputFileName);
            return specs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        /// <exception cref="dbsc.Core.TableSpecificationParseException"></exception>
        private IList<TableWithSchemaSpecificationWithCustomSelect> Parse(string input, IdentifierSyntax flavor, bool allowCustomSelect, string inputFileName)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);

            TableSpecificationListLexer lexer = new TableSpecificationListLexer(inputStream, flavor, allowCustomSelect);
            ErrorListener errorListener = new ErrorListener(inputFileName);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);

            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            TableSpecificationListParser parser = new TableSpecificationListParser(commonTokenStream, flavor, allowCustomSelect);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
            parser.BuildParseTree = true;

            TableSpecificationListParser.TableSpecificationListContext tree = parser.tableSpecificationList();
            errorListener.ThrowIfErrors();

            TableSpecificationListVisitor visitor = new TableSpecificationListVisitor(inputFileName, flavor, allowCustomSelect);
            List<TableWithSchemaSpecificationWithCustomSelect> result = visitor.VisitTableSpecificationList(tree);
            return result;
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