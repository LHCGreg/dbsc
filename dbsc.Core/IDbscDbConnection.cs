using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IDbscDbConnection : IDisposable
    {
        /// <summary>
        /// Executes a user-provided script - an upgrade script or a DB creation script.
        /// </summary>
        /// <param name="sql"></param>
        void ExecuteSqlScript(string sql);

        /// <summary>
        /// Executes SQL requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <param name="sql"></param>
        void ExecuteSql(string sql);

        /// <summary>
        /// Executes SQL requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParams"></param>
        void ExecuteSql(string sql, IDictionary<string, object> sqlParams);

        /// <summary>
        /// Runs a query requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql);

        /// <summary>
        /// Runs a query requested by the DBSC engine itself, not a user-provided script.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, IDictionary<string, object> sqlParams);
    }
}

/*
 Copyright 2013 Greg Najda

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