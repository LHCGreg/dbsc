using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Core.Antlr
{
    // Don't derive from the base visitor so that we can get strongly typed methods that return different types
    public class TableWithSchemaSpecificationWithCustomSelectListVisitor
    {
        public List<TableWithSchemaSpecificationWithCustomSelect> VisitTableSpecificationList(TableWithSchemaSpecificationWithCustomSelectListParser.TableSpecificationListContext context)
        {
            // tableSpecificationList : tableSpecificationLine (NEWLINE tableSpecificationLine)* EOF ;
            List<TableWithSchemaSpecificationWithCustomSelect> specifications = new List<TableWithSchemaSpecificationWithCustomSelect>();
            foreach (TableWithSchemaSpecificationWithCustomSelectListParser.TableSpecificationLineContext line in context.tableSpecificationLine())
            {
                TableWithSchemaSpecificationWithCustomSelect possiblyBlankSpecification = VisitTableSpecificationLine(line);
                if (possiblyBlankSpecification != null)
                {
                    specifications.Add(possiblyBlankSpecification);
                }
            }
            return specifications;
        }

        public TableWithSchemaSpecificationWithCustomSelect VisitTableSpecificationLine(TableWithSchemaSpecificationWithCustomSelectListParser.TableSpecificationLineContext context)
        {
            // tableSpecificationLine : possiblyQualifiedTableLine | ; // allow blank lines
            if (context.possiblyQualifiedTableLine() == null)
            {
                return null;
            }
            else
            {
                return VisitPossiblyQualifiedTableLine(context.possiblyQualifiedTableLine());
            }
        }

        public TableWithSchemaSpecificationWithCustomSelect VisitPossiblyQualifiedTableLine(TableWithSchemaSpecificationWithCustomSelectListParser.PossiblyQualifiedTableLineContext context)
        {
            // possiblyQualifiedTableLine : NEGATER? possiblyQualifiedTable CUSTOM_SELECT? ;
            // CUSTOM_SELECT : ':' WS_NO_NEWLINE* ~[\r\n]+ ;
            bool negated;
            if (context.NEGATER() != null)
            {
                negated = true;
            }
            else
            {
                negated = false;
            }

            string customSelect = null;
            if (context.CUSTOM_SELECT() != null)
            {
                string rawTokenText = context.CUSTOM_SELECT().GetText();
                string customSelectWithLeadingWhitespace = rawTokenText.Substring(1);
                customSelect = customSelectWithLeadingWhitespace.TrimStart(' ', '\t');
            }

            return VisitPossiblyQualifiedTable(context.possiblyQualifiedTable(), negated: negated, customSelect: customSelect);
        }

        public TableWithSchemaSpecificationWithCustomSelect VisitPossiblyQualifiedTable(TableWithSchemaSpecificationWithCustomSelectListParser.PossiblyQualifiedTableContext context, bool negated, string customSelect)
        {
            // possiblyQualifiedTable : unqualifiedTable | qualifiedTable;
            if (context.unqualifiedTable() != null)
            {
                return VisitUnqualifiedTable(context.unqualifiedTable(), negated: negated, customSelect: customSelect);
            }
            else
            {
                return VisitQualifiedTable(context.qualifiedTable(), negated: negated, customSelect: customSelect);
            }
        }

        public TableWithSchemaSpecificationWithCustomSelect VisitUnqualifiedTable(TableWithSchemaSpecificationWithCustomSelectListParser.UnqualifiedTableContext context, bool negated, string customSelect)
        {
            // unqualifiedTable: identifier;
            TableSpecificationFragment table = VisitIdentifier(context.identifier());

            // If identifier is just *, convert the spec to *.* as a special case
            if (table.Pattern.Count == 1 && table.Pattern[0].Type == WildcardType.Star)
            {
                return new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: table, negated: negated, defaultSchemaIsCaseSensitive: false, customSelect: customSelect);
            }
            else
            {
                return new TableWithSchemaSpecificationWithCustomSelect(schema: null, table: table, negated: negated, defaultSchemaIsCaseSensitive: false, customSelect: customSelect);
            }
        }
      
        public TableWithSchemaSpecificationWithCustomSelect VisitQualifiedTable(TableWithSchemaSpecificationWithCustomSelectListParser.QualifiedTableContext context, bool negated, string customSelect)
        {
            // qualifiedTable : schema=identifier '.' table=identifier;
            TableSpecificationFragment schema = VisitIdentifier(context.schema);
            TableSpecificationFragment table = VisitIdentifier(context.table);
            return new TableWithSchemaSpecificationWithCustomSelect(schema, table, negated: negated, defaultSchemaIsCaseSensitive: false, customSelect: customSelect);
        }

        public TableSpecificationFragment VisitIdentifier(TableWithSchemaSpecificationWithCustomSelectListParser.IdentifierContext context)
        {
            //        identifier : 
            //{Flavor == IdentifierSyntax.SqlServer}? MS_UNENCLOSED_ID_NAME
            //| {Flavor == IdentifierSyntax.SqlServer}? MS_BRACKET_ENCLOSED_ID
            //| {Flavor == IdentifierSyntax.Postgres}? PG_UNENCLOSED_ID_NAME
            //| {Flavor == IdentifierSyntax.Postgres}? PG_QUOTE_ENCLOSED_ID;
            if (context.MS_UNENCLOSED_ID_NAME() != null)
            {
                return ParseUnenclosedIdentifier(context.MS_UNENCLOSED_ID_NAME().Symbol.Text);
            }
            else if (context.MS_BRACKET_ENCLOSED_ID() != null)
            {
                return ParseSqlServerBracketEnclosedIdentifier(context.MS_BRACKET_ENCLOSED_ID().Symbol.Text);
            }
            else if (context.PG_UNENCLOSED_ID_NAME() != null)
            {
                return ParseUnenclosedIdentifier(context.PG_UNENCLOSED_ID_NAME().Symbol.Text);
            }
            else if (context.PG_QUOTE_ENCLOSED_ID() != null)
            {
                return ParsePgQuoteEnclosedIdentifier(context.PG_QUOTE_ENCLOSED_ID().Symbol.Text);
            }
            else
            {
                throw new Exception("Oops, missed an identifier token type.");
            }
        }

        private TableSpecificationFragment ParseUnenclosedIdentifier(string rawText)
        {
            List<StringOrWildcard> pattern = new List<StringOrWildcard>();
            StringBuilder accumulatedString = new StringBuilder();
            foreach (char c in rawText)
            {
                if (c == '*')
                {
                    if (accumulatedString.Length > 0)
                    {
                        pattern.Add(new StringOrWildcard(accumulatedString.ToString()));
                        accumulatedString.Length = 0;
                    }
                    pattern.Add(StringOrWildcard.Star);
                }
                else
                {
                    accumulatedString.Append(c);
                }
            }

            if (accumulatedString.Length > 0)
            {
                pattern.Add(new StringOrWildcard(accumulatedString.ToString()));
            }

            return new TableSpecificationFragment(pattern, caseSensitive: false);
        }

        private TableSpecificationFragment ParseSqlServerBracketEnclosedIdentifier(string rawText)
        {
            string withoutBrackets = rawText.Substring(1, rawText.Length - 2);
            string unescapedName = withoutBrackets.Replace("]]", "]");
            return ParseUnenclosedIdentifier(unescapedName);
        }

        private TableSpecificationFragment ParsePgQuoteEnclosedIdentifier(string rawText)
        {
            string withoutQuotes = rawText.Substring(1, rawText.Length - 2);
            string unescapedName = withoutQuotes.Replace("\"\"", "\"");
            return ParseUnenclosedIdentifier(unescapedName);
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