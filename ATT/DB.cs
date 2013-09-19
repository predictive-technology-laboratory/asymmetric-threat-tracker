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
