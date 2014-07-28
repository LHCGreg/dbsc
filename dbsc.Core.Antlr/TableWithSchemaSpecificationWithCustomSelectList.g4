grammar TableWithSchemaSpecificationWithCustomSelectList;

tableSpecificationList : tableSpecificationLine (NEWLINE tableSpecificationLine)* EOF ;
tableSpecificationLine : possiblyQualifiedTableLine | ; // allow blank lines
possiblyQualifiedTableLine : NEGATER? possiblyQualifiedTable CUSTOM_SELECT? ;
possiblyQualifiedTable : unqualifiedTable | qualifiedTable;
unqualifiedTable : identifier;
qualifiedTable : schema=identifier '.' table=identifier;
identifier : id=(UNENCLOSED_ID_NAME | BRACKET_ENCLOSED_ID);
                           
UNENCLOSED_ID_NAME : (LETTER | '_' | '@' | '#' | WILDCARD) (LETTER | NUMBER | '_' | '@' | '#' | WILDCARD)* ;
BRACKET_ENCLOSED_ID : '[' (~[\r\n\*\]] | ']]' | WILDCARD)+ ']' ;
WS_NO_NEWLINE : [ \t] -> skip;
NEWLINE : '\r'? '\n';
LETTER : [a-zA-Z\u0080-\u00FF] ;
NUMBER : [0-9] ;
WILDCARD : '*' ;
NEGATER : '-' ;
CUSTOM_SELECT : ':' WS_NO_NEWLINE* ~[\r\n]+ ; // Can't use two tokens because the token definition of the SELECT would match anything. Could use a separate lexer mode trigerred by :, but then would have to have separate lexer and parser grammar files, don't know if the msbuild integration supports that. WS_NO_NEWLINE* is necessary because this is a token definition, not a rule definition. The WS_NO_NEWLINE rule is copied into this token definition, not referenced.

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