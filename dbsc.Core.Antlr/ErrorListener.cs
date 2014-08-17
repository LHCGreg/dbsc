using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Core.Antlr
{
    /// <summary>
    /// Stores syntax errors and lets you throw an exception after parsing completes if there were any errors.
    /// </summary>
    public class ErrorListener : IAntlrErrorListener<int> /* Lexer errors */, IAntlrErrorListener<IToken> /* Parser errors */
    {
        public IList<string> Errors { get; private set; }

        private string _inputFileName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFileName">File name to use in error messages.</param>
        public ErrorListener(string inputFileName)
        {
            Errors = new List<string>();
            _inputFileName = inputFileName;
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string error = string.Format("{0} line {1}:{2} {3}", _inputFileName, line, charPositionInLine, msg);
            Errors.Add(error);
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string error = string.Format("{0} line {1}:{2} {3}", _inputFileName, line, charPositionInLine, msg);
            Errors.Add(error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="dbsc.Core.TableSpecificationParseException"></exception>
        public void ThrowIfErrors()
        {
            if (Errors.Count > 0)
            {
                throw new TableSpecificationParseException(Errors);
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