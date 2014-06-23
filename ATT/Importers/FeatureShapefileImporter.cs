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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Importers
{
    [Serializable]
    public class FeatureShapefileImporter : ShapefileImporter
    {
        public FeatureShapefileImporter(string name, string path, string relativePath, string sourceURI, int sourceSRID, int targetSRID, IShapefileInfoRetriever shapefileInfoRetriever)
            : base(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever)
        {
        }

        public override void Import()
        {
            base.Import();

            // add time column - this is an interim solution, since we might want to keep a shapefile's time column but we're not quite sure how to do that in a way that will work
            string shapefileGeometryTable = ShapefileGeometry.GetTableName(ImportedShapefile);
            DB.Connection.ExecuteNonQuery("ALTER TABLE " + shapefileGeometryTable + " DROP COLUMN IF EXISTS " + ShapefileGeometry.Columns.Time + ";" +
                                          "ALTER TABLE " + shapefileGeometryTable + " ADD COLUMN " + ShapefileGeometry.Columns.Time + " TIMESTAMP;" +
                                          "UPDATE " + shapefileGeometryTable + " SET " + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp;" +
                                          "CREATE INDEX ON " + shapefileGeometryTable + " (" + ShapefileGeometry.Columns.Time + ");");
        }
    }
}
