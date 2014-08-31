using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TestUtils.Sql;

namespace oradbsc.Integration
{
    [TestFixture]
    class CheckoutTestFixture : AbstractCheckoutTestFixture<OracleTestHelper>
    {
        protected override int? Port { get { return 1521; } }
        protected override bool ImportSupported { get { return false; } }
        protected override bool ExtendedTableSpecsSupported { get { return false; } }
        protected override bool CustomSelectImportSupported { get { return false; } }
        protected override bool TemplateSupported { get { return false; } }
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