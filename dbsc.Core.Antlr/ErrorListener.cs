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
            string error = FormErrorMessage(_inputFileName, line, charPositionInLine, msg);
            Errors.Add(error);
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string error = FormErrorMessage(_inputFileName, line, charPositionInLine, msg);
            Errors.Add(error);
        }

        public static string FormErrorMessage(string inputFileName, int line, int charPositionInLine, string msg)
        {
            return string.Format("{0} line {1}:{2} {3}", inputFileName, line, charPositionInLine, msg);
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
