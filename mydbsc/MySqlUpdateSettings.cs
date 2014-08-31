using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.MySql
{
    class MySqlUpdateSettings : ISqlUpdateSettings<DbConnectionInfo, MySqlImportSettings>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public MySqlImportSettings ImportOptions { get; set; }

        public MySqlUpdateSettings(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
        }

        public MySqlUpdateSettings(MySqlCheckoutSettings checkoutSettings)
        {
            Directory = checkoutSettings.Directory;
            TargetDatabase = checkoutSettings.TargetDatabase.Clone();
            Revision = checkoutSettings.Revision;

            if (checkoutSettings.ImportOptions != null)
            {
                ImportOptions = checkoutSettings.ImportOptions.Clone();
            }
        }

        public MySqlUpdateSettings Clone()
        {
            MySqlUpdateSettings clone = new MySqlUpdateSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        IUpdateSettings<DbConnectionInfo, MySqlImportSettings> IUpdateSettings<DbConnectionInfo, MySqlImportSettings>.Clone()
        {
            return Clone();
        }
    }
}

// Copyright (C) 2014 Greg Najda
//
// This file is part of mydbsc.
//
// mydbsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.  If not, see <http://www.gnu.org/licenses/>.