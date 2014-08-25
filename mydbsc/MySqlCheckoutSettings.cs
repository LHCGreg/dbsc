using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.MySql
{
    class MySqlCheckoutSettings : ISqlCheckoutSettings<DbConnectionInfo, MySqlImportSettings, MySqlUpdateSettings>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public string CreationTemplate { get; set; }
        public MySqlImportSettings ImportOptions { get; set; }

        public MySqlCheckoutSettings(DbConnectionInfo targetDatabase)
        {
            TargetDatabase = targetDatabase;
            Directory = Environment.CurrentDirectory;
            CreationTemplate = dbsc.Core.Options.DBCreateTemplateOptionBundle.DefaultSQLTemplate;
        }

        public MySqlUpdateSettings UpdateOptions { get { return new MySqlUpdateSettings(this); } }

        public MySqlCheckoutSettings Clone()
        {
            MySqlCheckoutSettings clone = new MySqlCheckoutSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;
            clone.CreationTemplate = CreationTemplate;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        ICheckoutOptions<DbConnectionInfo, MySqlImportSettings, MySqlUpdateSettings> ICheckoutOptions<DbConnectionInfo, MySqlImportSettings, MySqlUpdateSettings>.Clone()
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