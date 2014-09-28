using NUnit.Framework;
using PTL.ATT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttTest.Importers
{
    [TestFixture]
    public class IncidentShapefileImporterTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            if (Configuration.Initialized)
                Configuration.Reset(null);
            else
                Configuration.Initialize("att_config.xml", true);
        }

        [Test]
        public void Test()
        {
        }

        public static void UnzipTestIncidents()
        {

        }

        public static void ImportTestIncidents()
        {

        }
    }
}
