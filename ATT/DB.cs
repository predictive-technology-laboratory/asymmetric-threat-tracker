#region copyright
// Copyright 2013-2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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

        public static string[] Tables
        {
            get { return Connection.GetTables().Where(t => t != "spatial_ref_sys").ToArray(); }
        }

        public static void Initialize()
        {
            Connection = new ConnectionPool(Configuration.PostgresHost, Configuration.PostgresPort, Configuration.PostgresSSL, Configuration.PostgresUser, Configuration.PostgresPassword, Configuration.PostgresDatabase, Configuration.PostgresConnectionTimeout, Configuration.PostgresRetryLimit, Configuration.PostgresCommandTimeout, Configuration.PostgresMaxPoolSize);
            Connection.CreateTables(new Assembly[] { Assembly.GetExecutingAssembly() });
        }
    }
}
