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
    public class AreaShapefileImporter : ShapefileImporter
    {
        private int _areaContainmentBoxSize;

        public AreaShapefileImporter(string name, string path, string relativePath, string sourceURI, int sourceSRID, int targetSRID, IShapefileInfoRetriever shapefileInfoRetriever, int areaContainmentBoxSize)
            : base(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever)
        {
            _areaContainmentBoxSize = areaContainmentBoxSize;
        }

        public override void Import()
        {
            base.Import();

            Area.Create(ImportedShapefile, ImportedShapefile.Name, _areaContainmentBoxSize);

            // we don't currently have anything to put in the Time column for areas -- maybe in the future
            DB.Connection.ExecuteNonQuery("ALTER TABLE " + ImportedShapefile.GeometryTable + " DROP COLUMN IF EXISTS " + ShapefileGeometry.Columns.Time + ";" +
                                          "ALTER TABLE " + ImportedShapefile.GeometryTable + " ADD COLUMN " + ShapefileGeometry.Columns.Time + " TIMESTAMP;" +
                                          "UPDATE " + ImportedShapefile.GeometryTable + " SET " + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp;" +
                                          "CREATE INDEX ON " + ImportedShapefile.GeometryTable + " (" + ShapefileGeometry.Columns.Time + ");");

            Console.Out.WriteLine("Area definition completed.");
        }

        public override void GetUpdateRequests(UpdateRequestDelegate updateRequest)
        {
            base.GetUpdateRequests(updateRequest);

            updateRequest("Area containment box size (meters)", _areaContainmentBoxSize, null, GetUpdateRequestId("containment_box_size"));
        }

        public override void Update(Dictionary<string, object> updateKeyValue)
        {
            base.Update(updateKeyValue);

            _areaContainmentBoxSize = Convert.ToInt32(updateKeyValue[GetUpdateRequestId("containment_box_size")]);
        }
    }
}
