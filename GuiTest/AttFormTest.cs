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
using NUnit.Framework;
using PTL.ATT;
using PTL.ATT.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace PTL.ATT.GuiTest
{
    [TestFixture]
    public class AttFormTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            PTL.ATT.Configuration.Initialize(Path.Combine("Config", "att_config.xml"), true);
            PTL.ATT.GUI.Configuration.Initialize(Path.Combine("Config", "gui_config.xml"));

            int numImporters = 0;
            string aoZipPath = Path.Combine(PTL.ATT.GUI.Configuration.PostGisShapefileDirectory, "Chicago", "AO", "chicago_city_boundary.zip");
            string unzippedAoPath = Path.Combine(aoZipPath + "_unzipped", "chicago_city_boundary.shp");
            ZipFile.ExtractToDirectory(aoZipPath, Directory.GetParent(unzippedAoPath).FullName);
            Importer importer = new AreaShapefileImporter("Chicago", unzippedAoPath, null, null, -1, -1, null, 1000);
            importer.Import();
            importer.Save(false);
            ++numImporters;

            List<Area> areas = Area.GetAll();
            Assert.AreEqual(areas.Count, 1);
            Area chicago = areas[0];

            foreach (string distanceShapefilePath in Directory.GetFiles(Path.Combine(PTL.ATT.GUI.Configuration.PostGisShapefileDirectory, "Chicago", "Distance features"), "*.shp"))
            {
                importer = new FeatureShapefileImporter(Path.GetFileName(distanceShapefilePath), distanceShapefilePath, null, null, -1, -1, null);
                importer.Import();
                importer.Save(false);
                ++numImporters;
            }

            Dictionary<string, string> dbColInputCol = new Dictionary<string,string>();
            dbColInputCol.Add(Incident.Columns.NativeId, "id");
            dbColInputCol.Add(Incident.Columns.Time, "date");
            dbColInputCol.Add(Incident.Columns.Type, "primary_type");
            dbColInputCol.Add(Incident.Columns.X(chicago), "longitude");
            dbColInputCol.Add(Incident.Columns.Y(chicago), "latitude");
            XmlImporter.IncidentXmlRowInserter incidentRowInserter = new XmlImporter.IncidentXmlRowInserter(dbColInputCol, chicago, 0, 4326);
            string crimeZipPath = Path.Combine(PTL.ATT.GUI.Configuration.IncidentsImportDirectory, "chicago_crimes_december_2012-march_2013.zip");
            string crimeUnzippedPath = Path.Combine(crimeZipPath + "_unzipped", "chicago_crimes_december_2012-march_2013.xml");
            importer = new XmlImporter("Crime", crimeUnzippedPath, null, null, incidentRowInserter, "row", "row");
            importer.Import();
            importer.Save(false);
            ++numImporters;

            dbColInputCol = new Dictionary<string, string>();
            XmlImporter.PointfileXmlRowInserter pointFileRowInserter = new XmlImporter.PointfileXmlRowInserter(dbColInputCol, 4326, chicago);
            string buildingViolationsZipPath = Path.Combine(PTL.ATT.GUI.Configuration.EventsImportDirectory, "")
            importer = new XmlImporter("Building violations", "building_violations_jan-mar_2013.zip", null, null, pointFileRowInserter, "row", "row");
            importer.Import();
            importer.Save(false);
            ++numImporters;

            Assert.AreEqual(numImporters, Importer.GetAll().Count());
        }

        [Test]
        public void Test()
        {
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            DB.Connection.Dispose();
        }
    }
}
