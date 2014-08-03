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
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Importers
{
    [Serializable]
    public class IncidentShapefileImporter : ShapefileImporter
    {
        private Area _importArea;
        Dictionary<string, string> _incidentColumnShapefileColumn;
        private IIncidentTableShapefileTableMappingRetriever _incidentShapefileMappingRetriever;
        private int _hourOffset;

        public IncidentShapefileImporter(string name, string path, string relativePath, string sourceURI, int sourceSRID, int targetSRID, IShapefileInfoRetriever shapefileInfoRetriever, Area importArea, IIncidentTableShapefileTableMappingRetriever incidentShapefileMappingRetriever, int hourOffset)
            : base(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever)
        {
            _importArea = importArea;
            _incidentShapefileMappingRetriever = incidentShapefileMappingRetriever;
            _hourOffset = hourOffset;
        }

        public IncidentShapefileImporter(string name, string path, string relativePath, string sourceURI, int sourceSRID, int targetSRID, IShapefileInfoRetriever shapefileInfoRetriever, Area importArea, Dictionary<string, string> incidentColumnShapefileColumn, int hourOffset)
            : base(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever)
        {
            _importArea = importArea;
            _incidentColumnShapefileColumn = incidentColumnShapefileColumn;
            _hourOffset = hourOffset;
        }            

        public override void Import()
        {
            base.Import();

            Console.Out.WriteLine("Extracting incident data from shapefile.");

            if (_incidentColumnShapefileColumn == null)
                _incidentColumnShapefileColumn = _incidentShapefileMappingRetriever.MapIncidentColumnsToShapefileColumns(ImportedShapefile.GeometryTable, true);

            DB.Connection.ExecuteNonQuery("INSERT INTO " + Incident.GetTableName(_importArea, true) + " (" + Incident.Columns.Insert + ") " +

                                          "SELECT " + _incidentColumnShapefileColumn[Incident.Columns.Location] + "," +
                                                      (_incidentColumnShapefileColumn.ContainsKey(Incident.Columns.NativeId) ? _incidentColumnShapefileColumn[Incident.Columns.NativeId] + "::VARCHAR" : "DEFAULT") + "," +
                                                      "false," +
                                                      _incidentColumnShapefileColumn[Incident.Columns.Time] + "::TIMESTAMP + INTERVAL '" + _hourOffset + " HOUR'," +
                                                      "TRIM(BOTH FROM " + _incidentColumnShapefileColumn[Incident.Columns.Type] + "::VARCHAR) " +

                                          "FROM " + ImportedShapefile.GeometryTable + ";" +
                                          "DROP TABLE " + ImportedShapefile.GeometryTable + ";");

            Console.Out.WriteLine("Incident import from shapefile finished.");
        }
    }
}
