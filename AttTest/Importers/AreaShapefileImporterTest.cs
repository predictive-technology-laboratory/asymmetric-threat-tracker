using NUnit.Framework;
using PTL.ATT;
using PTL.ATT.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AttTest.Importers
{
    [TestFixture]
    public class AreaShapefileImporterTest
    {
        private static string AreaZipFilePath
        {
            get { return Path.Combine(Configuration.PostGisShapefileDirectory, "Chicago", "AO", "chicago_city_boundary.zip"); }
        }

        private static string UnzippedAreaDirectory
        {
            get { return Path.Combine(AreaZipFilePath, "_unzipped"); }
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            if (Configuration.Initialized)
                Configuration.Reset(null);
            else
                Configuration.Initialize("att_config.xml", true);

            UnzipTestArea();
        }

        [Test]
        public void Test()
        {
            ImportTestArea();
            Area importedArea1 = Area.GetAll().Last();
            ImportTestArea();
            Area importedArea2 = Area.GetAll().Last();
            Assert.AreEqual(importedArea1.Id, 1);
            Assert.AreEqual(importedArea2.Id, 2);
            Assert.AreEqual(Area.GetAll().Count, 2);
        }

        public static void UnzipTestArea()
        {
            if (!Directory.Exists(UnzippedAreaDirectory))
                ZipFile.ExtractToDirectory(AreaZipFilePath, UnzippedAreaDirectory);
        }

        public static void ImportTestArea()
        {
            new AreaShapefileImporter("Chicago", Path.Combine(UnzippedAreaDirectory, "chicago_city_boundary.shp"), null, null, -1, -1, null, 1000).Import();
        }
    }
}
