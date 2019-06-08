using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IImportTableListParser<TParseResult>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        /// <exception cref="dbsc.Core.TableSpecificationParseException"></exception>
        TParseResult Parse(TextReader input, string inputFileName);
    }
}
