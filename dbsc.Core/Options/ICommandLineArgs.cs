using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Options
{
    public interface ICommandLineArgs<TCheckoutSettings, TUpdateSettings>
    {
        TCheckoutSettings GetCheckoutSettings();
        TUpdateSettings GetUpdateSettings();
    }
}
