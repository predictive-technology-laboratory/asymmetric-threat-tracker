using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;

namespace PTL.ATT.Incidents
{
    public abstract class Importer
    {
        public Importer() { }

        public abstract void Import(string path);
    }
}
