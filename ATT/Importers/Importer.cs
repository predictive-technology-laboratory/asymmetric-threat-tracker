#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.XML;

namespace PTL.ATT.Importers
{
    public abstract class Importer
    {
        private string _table;
        private string _insertColumns;

        public string Table
        {
            get { return _table; }
        }

        public string InsertColumns
        {
            get { return _insertColumns; }
        }

        public Importer(string table, string insertColumns)
        {
            _table = table;
            _insertColumns = insertColumns;
        }

        public abstract void Import(string path);
    }
}
