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
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using LAIR.ResourceAPIs.PostgreSQL;

namespace PTL.ATT
{
    public static class DB
    {
        public static ConnectionPool Connection;

        public static void Initialize()
        {
            Connection = new ConnectionPool(Configuration.PostgresHost, Configuration.PostgresPort, Configuration.PostgresSSL, Configuration.PostgresUser, Configuration.PostgresPassword, Configuration.PostgresDatabase, Configuration.PostgresConnectionTimeout, Configuration.PostgresRetryLimit, Configuration.PostgresCommandTimeout, Configuration.PostgresMaxPoolSize);
            Connection.CreateTables(new Assembly[] { Assembly.GetExecutingAssembly() });
        }
    }
}
