using NUnit.Framework;
using PTL.ATT;
using PTL.ATT.Importers;
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
    public class FeatureShapefileImporterTest
    {
        public static string FeaturesDirectory
        {
            get { return Path.Combine(Configuration.PostGisShapefileDirectory, "Chicago", "Features"); }
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            if (Configuration.Initialized)
                Configuration.Reset(null);
            else
                Configuration.Initialize("att_config.xml", true);

            UnzipTestFeatures();
        }

        [Test]
        public void Test()
        {
            ImportTestFeatureShapefiles();
        }

        public static void UnzipTestFeatures()
        {
            foreach (string featureZipFilePath in Directory.GetFiles(FeaturesDirectory, "*.zip"))
            {
                string unzippedPath = featureZipFilePath + "_unzipped";
                if (!Directory.Exists(unzippedPath))
                    ZipFile.ExtractToDirectory(featureZipFilePath, unzippedPath);
            }
        }

        public static void ImportTestFeatureShapefiles()
        {
            int shapefileCount = 0;
            foreach (string distanceShapefilePath in Directory.GetFiles(FeaturesDirectory, "*.shp"))
            {
                new FeatureShapefileImporter(Path.GetFileName(distanceShapefilePath), distanceShapefilePath, null, null, -1, -1, null).Import();
                ++shapefileCount;
            }

            Assert.AreEqual(shapefileCount, Shapefile.GetAll().Count());
        }
    }
}
