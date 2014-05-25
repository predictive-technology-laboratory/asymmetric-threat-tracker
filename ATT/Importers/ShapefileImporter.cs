using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using LAIR.Extensions;

namespace PTL.ATT.Importers
{
    [Serializable]
    public abstract class ShapefileImporter : Importer
    {
        /// <summary>
        /// Gets shapefile information
        /// </summary>
        /// <param name="shapefilePath">Path to shapefile being imported</param>
        /// <param name="optionValuesToGet">Options for which a value is needed</param>
        /// <param name="optionValue">Dictionary in which to place retrieved option-value pairs</param>
        public delegate void GetShapefileInfoDelegate(string shapefilePath, List<string> optionValuesToGet, Dictionary<string, string> optionValue);

        [NonSerialized]
        private GetShapefileInfoDelegate _getShapefileInfo;
        private Shapefile _importedShapefile;

        protected Shapefile ImportedShapefile
        {
            get { return _importedShapefile; }
        }

        public ShapefileImporter(string name, string path, string sourceURI, GetShapefileInfoDelegate getShapefileInfo)
            : base(name, path, sourceURI)
        {
            _getShapefileInfo = getShapefileInfo;
        }

        public override void Import()
        {
            base.Import();

            Console.Out.WriteLine("Importing shapefile from \"" + Path + "\"...");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);
            int shapefileId = -1;
            string tempTable = null;
            try
            {
                Dictionary<string, string> importOptionValue = new Dictionary<string, string>();
                string importOptionsPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileNameWithoutExtension(Path) + ".att");
                if (File.Exists(importOptionsPath))
                    foreach (string line in File.ReadLines(importOptionsPath))
                    {
                        string[] parts = line.Split('=');
                        importOptionValue.Add(parts[0].Trim(), parts[1].Trim());
                    }

                if (!string.IsNullOrWhiteSpace(Name))
                    importOptionValue["name"] = Name;

                List<string> neededValues = new List<string>();
                if (!importOptionValue.ContainsKey("reprojection") || string.IsNullOrWhiteSpace(importOptionValue["reprojection"])) neededValues.Add("reprojection");
                if (!importOptionValue.ContainsKey("name") || string.IsNullOrWhiteSpace(importOptionValue["name"])) neededValues.Add("name");
                if (neededValues.Count > 0)
                    if (_getShapefileInfo == null)
                        throw new ArgumentNullException("Failed to provide shapefile import information or a suitable callback to get it.");
                    else
                        _getShapefileInfo(Path, neededValues, importOptionValue);

                string missingValues = neededValues.Where(v => !importOptionValue.ContainsKey(v) || string.IsNullOrWhiteSpace(importOptionValue[v])).Concatenate(", ");
                if (missingValues != "")
                    throw new Exception("Failed to provide needed values for shapefile import:  " + missingValues);

                string reprojection = importOptionValue["reprojection"];
                Match reprojectionMatch = new Regex("(?<from>[0-9]+):(?<to>[0-9]+)").Match(reprojection);
                if (!reprojectionMatch.Success)
                    throw new Exception("Invalid shapefile reprojection \"" + reprojection + "\". Must be in 1234:1234 format.");

                int fromSRID = int.Parse(reprojectionMatch.Groups["from"].Value);
                int toSRID = int.Parse(reprojectionMatch.Groups["to"].Value);
                if (fromSRID == toSRID)
                    reprojection = fromSRID.ToString();

                string name = importOptionValue["name"];
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Empty name given for shapefile \"" + Path + "\".");

                Name = name; // to make sure names retrieved from *.att files get back to the object and ultimately back to the DB

                File.WriteAllText(importOptionsPath, "reprojection=" + fromSRID + ":" + toSRID + Environment.NewLine +
                                                     "name=" + name);

                Shapefile.ShapefileType type;
                if (this is FeatureShapefileImporter)
                    type = Shapefile.ShapefileType.Feature;
                else if (this is AreaShapefileImporter)
                    type = Shapefile.ShapefileType.Area;
                else
                    throw new NotImplementedException("Unrecognized shapefile importer type:  " + GetType());

                shapefileId = Shapefile.Create(cmd.Connection, name, toSRID, type);
                tempTable = "shapefile_import_" + shapefileId;

                string sql;
                string error;
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = Configuration.Shp2PgsqlPath;
                    process.StartInfo.Arguments = "-I -g geom -s " + reprojection + " \"" + Path + "\" " + tempTable;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    Console.Out.WriteLine("Converting shapefile \"" + name + "\".");

                    sql = process.StandardOutput.ReadToEnd().Replace("BEGIN;", "").Replace("COMMIT;", "");
                    error = process.StandardError.ReadToEnd().Trim().Replace(Environment.NewLine, "; ").Replace("\n", "; ");

                    process.WaitForExit();
                }

                Console.Out.WriteLine(error);

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                Console.Out.WriteLine("Importing shapefile into database");
                _importedShapefile = new Shapefile(shapefileId);
                ShapefileGeometry.Create(cmd.Connection, _importedShapefile, tempTable, "geom");
            }
            catch (Exception ex)
            {
                try
                {
                    cmd.CommandText = "DELETE FROM " + Shapefile.Table + " WHERE " + Shapefile.Columns.Id + "=" + shapefileId;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete shapefile:  " + ex2.Message); }

                throw new Exception("Failed to import shape file(s):  " + ex.Message);
            }
            finally
            {
                try
                {
                    cmd.CommandText = "DROP TABLE " + tempTable + ";";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2) { Console.Out.WriteLine("Falied to drop table \"" + tempTable + "\":  " + ex2.Message); }

                DB.Connection.Return(cmd.Connection);
            }
        }
    }
}
