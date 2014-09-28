using NUnit.Framework;
using PTL.ATT;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttTest.Importers
{
    [TestFixture]
    public class XmlImporterTest
    {
        public static string ZippedIncidentsPath
        {
            get {  return Path.Combine(Configuration.IncidentsImportDirectory, "chicago_crimes_december_2012-march_2013.zip"); }
        }

        public static string ZippedEventsPath
        {
            get { return Path.Combine(Configuration.EventsImportDirectory, "building_violations_jan-mar_2013.zip"); }
        }

        public static string UnzippedIncidentsDirectory
        {
            get { return ZippedIncidentsPath + "_unzipped"; }
        }

        public static string UnzippedEventsDirectory
        {
            get { return ZippedEventsPath + "_unzipped"; }
        }

        public static string UnzippedIncidentsPath
        {
            get { return Path.Combine(UnzippedIncidentsDirectory, "chicago_crimes_december_2012-march_2013.xml"); }
        }

        public static string UnzippedEventsPath
        {
            get { return Path.Combine(UnzippedEventsDirectory, "building_violations_jan-mar_2013.xml"); }
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            if (Configuration.Initialized)
                Configuration.Reset(null);
            else
                Configuration.Initialize("att_config.xml", true);

            UnzipTestIncidents();
        }

        [Test]
        public void TestIncidentXmlImporter()
        {
        }

        [Test]
        public void TestPointXmlImporter()
        {
        }

        public static void UnzipTestIncidents()
        {
            if (!Directory.Exists(UnzippedIncidentsDirectory))
                ZipFile.ExtractToDirectory(ZippedIncidentsPath, UnzippedIncidentsDirectory);
        }

        public static void UnzipTestPoints()
        {

        }
    }
}
