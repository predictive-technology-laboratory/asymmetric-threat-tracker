#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LAIR.MachineLearning;
using LAIR.ResourceAPIs.R;
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;
using PTL.ATT.Models;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using NpgsqlTypes;
using LAIR.Extensions;
using PTL.ATT.Smoothers;

namespace PTL.ATT.Models
{
    public class KernelDensityDCM : DiscreteChoiceModel
    {
        static KernelDensityDCM()
        {
            if (Configuration.RCranMirror != null)
                R.InstallPackages(R.CheckForMissingPackages(new string[] { "ks" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }

        private new const string Table = "kernel_density_dcm";

        private new class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Normalize = "normalize";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return DiscreteChoiceModel.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinDiscreteChoiceModel { get { return DiscreteChoiceModel.Table + " JOIN " + Table + " ON " + DiscreteChoiceModel.Table + "." + DiscreteChoiceModel.Columns.Id + "=" + Table + "." + Id; } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                    Columns.Id + " INT PRIMARY KEY REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE CASCADE," +
                    Columns.Normalize + " BOOLEAN);";
        }

        public static int Create(string name,
                                 int pointSpacing,
                                 Area trainingArea,
                                 DateTime trainingStart,
                                 DateTime trainingEnd,
                                 int trainingSampleSize,
                                 int predictionSampleSize,
                                 IEnumerable<string> incidentTypes,
                                 bool normalize,
                                 IEnumerable<Smoother> smoothers)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("BEGIN");
            cmd.ExecuteNonQuery();

            int dcmId = DiscreteChoiceModel.Create(cmd.Connection, name, pointSpacing, typeof(KernelDensityDCM), trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, smoothers);

            cmd.CommandText = "INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + dcmId + "," + normalize + ");COMMIT;";
            cmd.ExecuteNonQuery();

            DB.Connection.Return(cmd.Connection);

            return dcmId;
        }

        public static List<float> GetDensityEstimate(IEnumerable<PostGIS.Point> inputPoints, int inputSampleSize, bool binned, float bGridSizeX, float bGridSizeY, IEnumerable<PostGIS.Point> evalPoints, bool normalize)
        {
            if (binned)
                if (bGridSizeX <= 0 || bGridSizeY <= 0)
                    throw new ArgumentException("bGridSizeX and bGridSizeY must be > 0 when performing binning");

            int numInputPoints = inputPoints.Count();

            if (inputSampleSize < numInputPoints)
            {
                List<PostGIS.Point> sample = new List<PostGIS.Point>(numInputPoints);
                sample.AddRange(inputPoints);
                sample.Randomize(new Random(8479849));
                sample.RemoveRange(0, sample.Count - inputSampleSize);

                if (sample.Count != inputSampleSize)
                    throw new Exception("inputPoints iterator returned inconsistent results across multiple runs");

                inputPoints = sample;
            }

            string inputPointsPath = Path.GetTempFileName();
            StreamWriter inputPointsFile = new StreamWriter(inputPointsPath);
            foreach (PostGIS.Point inputPoint in inputPoints)
                inputPointsFile.Write(inputPoint.X + "," + inputPoint.Y + "\n");
            inputPointsFile.Close();
            inputPoints = null;

            string evalPointsPath = Path.GetTempFileName();
            StreamWriter evalPointsFile = new StreamWriter(evalPointsPath);
            int numEvalPoints = 0;
            foreach (PostGIS.Point evalPoint in evalPoints)
            {
                evalPointsFile.Write(evalPoint.X + "," + evalPoint.Y + "\n");
                ++numEvalPoints;
            }
            evalPointsFile.Close();
            evalPoints = null;

            string bGridSizes = "c(" + bGridSizeX + "," + bGridSizeY + ")";
            string outputPath = Path.GetTempFileName();

            string rOutput = null;
            string rError = null;
            try
            {
                R.Execute(@"
library(ks)
set.seed(12512435)
input.points = read.csv(""" + inputPointsPath.Replace(@"\", @"\\") + @""",header=FALSE)
eval.points = read.csv(""" + evalPointsPath.Replace(@"\", @"\\") + @""",header=FALSE)
h = Hpi(input.points,pilot=""dscalar""" + (binned ? ",binned=TRUE,bgridsize=" + bGridSizes : "") + @")
est = kde(input.points,H=h," + (binned ? "binned=TRUE,bgridsize=" + bGridSizes + "," : "") + @"eval.points=eval.points)$estimate
" + (normalize ? "est = (est - min(est))  / (max(est) - min(est))" : "") + @"
write.table(est,file=""" + outputPath.Replace(@"\", @"\\") + @""",row.names=FALSE,col.names=FALSE)", out rOutput, out rError, false);

            }
            catch (Exception ex)
            {
                try { File.Delete(outputPath); }
                catch (Exception) { }

                if (rOutput != null)
                    Console.Out.WriteLine("R output:  " + rOutput);

                if (rError != null)
                    Console.Out.WriteLine("R error:  " + rError);

                throw ex;
            }
            finally
            {
                try { File.Delete(inputPointsPath); }
                catch (Exception ex) { Console.Out.WriteLine("Failed to delete file \"" + inputPointsPath + "\":  " + ex.Message); }
                try { File.Delete(evalPointsPath); }
                catch (Exception ex) { Console.Out.WriteLine("Failed to delete file \"" + evalPointsPath + "\":  " + ex.Message); }
            }

            try
            {
                List<float> density = File.ReadLines(outputPath).Select(line => float.Parse(line)).ToList();

                if (density.Count != numEvalPoints)
                {
                    Console.Out.WriteLine("WARNING:  Number of density estimation output points (" + density.Count + ") does not match the number of evaluation points (" + numEvalPoints + ")" + Environment.NewLine +
                                          "\tR output:  " + rOutput + Environment.NewLine +
                                          "\tR error:  " + rError);
                }

                return density;
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                try { File.Delete(outputPath); }
                catch (Exception ex) { Console.Out.WriteLine("Failed to delete file \"" + outputPath + "\":  " + ex.Message); }
            }
        }

        private static IEnumerable<Tuple<string, Parameter>> GetPointPredictionValues(Dictionary<int, float> pointIdOverallDensity, Dictionary<int, Dictionary<string, float>> pointIdIncidentDensity)
        {
            foreach (int pointId in pointIdIncidentDensity.Keys)
            {
                StringBuilder labels = new StringBuilder();
                StringBuilder threats = new StringBuilder();
                foreach (string label in pointIdIncidentDensity[pointId].Keys)
                {
                    labels.Append((labels.Length == 0 ? "'{" : ",") + "\"" + label + "\"");
                    threats.Append((threats.Length == 0 ? "'{" : ",") + pointIdIncidentDensity[pointId][label]);
                }

                labels.Append("}'");
                threats.Append("}'");

                float totalThreat = pointIdOverallDensity[pointId];

                string timeParameterName = "@time_" + pointId;
                Parameter p = new Parameter(timeParameterName, NpgsqlDbType.Timestamp, DateTime.MinValue);

                yield return new Tuple<string, Parameter>("(" + labels + "," + pointId + "," + threats + "," + timeParameterName + "," + totalThreat + ")", p);
            }
        }

        private bool _normalize;

        public bool Normalize
        {
            get { return _normalize; }
        }

        internal KernelDensityDCM(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " +
                                                         "FROM " + Columns.JoinDiscreteChoiceModel + " " +
                                                         "WHERE " + Table + "." + Columns.Id + "=" + id);

            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        protected override void Construct(NpgsqlDataReader reader)
        {
            base.Construct(reader);

            _normalize = Convert.ToBoolean(reader[Table + "_" + Columns.Normalize]);
        }

        public void Update(string name, int pointSpacing, Area trainingArea, DateTime trainingStart, DateTime trainingEnd, int trainingSampleSize, int predictionSampleSize, IEnumerable<string> incidentTypes, bool normalize, IEnumerable<Smoother> smoothers)
        {
            base.Update(name, pointSpacing, trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, smoothers);

            _normalize = normalize;

            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " +
                                          Columns.Normalize + "=" + normalize + " " +
                                          "WHERE " + Columns.Id + "=" + Id);
        }

        internal override void Run(Prediction prediction, int idOfSpatiotemporallyIdenticalPrediction)
        {
            List<PostGIS.Point> nullPoints = new List<PostGIS.Point>();
            Area predictionArea = prediction.PredictionArea;
            double areaMinX = predictionArea.BoundingBox.MinX;
            double areaMaxX = predictionArea.BoundingBox.MaxX;
            double areaMinY = predictionArea.BoundingBox.MinY;
            double areaMaxY = predictionArea.BoundingBox.MaxY;
            for (double x = areaMinX + PointSpacing / 2d; x <= areaMaxX; x += PointSpacing)  // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + PointSpacing / 2d; y <= areaMaxY; y += PointSpacing)
                    nullPoints.Add(new PostGIS.Point(x, y, predictionArea.SRID));

            nullPoints = predictionArea.Contains(nullPoints).Select(i => nullPoints[i]).ToList();

            NpgsqlConnection connection = DB.Connection.OpenConnection;

            try
            {
                Console.Out.WriteLine("Running KDE for all incident types");

                Point.CreateTable(prediction.Id, predictionArea.SRID);
                List<int> nullPointIds = Point.Insert(connection, nullPoints.Select(p => new Tuple<PostGIS.Point, string, DateTime>(p, PointPrediction.NullLabel, DateTime.MinValue)), prediction.Id, predictionArea, false, false);

                List<PostGIS.Point> incidentPoints = new List<PostGIS.Point>(Incident.Get(TrainingStart, TrainingEnd, predictionArea, IncidentTypes.ToArray()).Select(i => i.Location));
                List<float> density = GetDensityEstimate(incidentPoints, TrainingSampleSize, false, 0, 0, nullPoints, _normalize);
                Dictionary<int, float> pointIdOverallDensity = new Dictionary<int, float>();
                int pointNum = 0;
                foreach (int nullPointId in nullPointIds)
                    pointIdOverallDensity.Add(nullPointId, density[pointNum++]);

                Dictionary<int, Dictionary<string, float>> pointIdIncidentDensity = new Dictionary<int, Dictionary<string, float>>();

                if (IncidentTypes.Count == 1)
                {
                    string incident = IncidentTypes.First();
                    foreach (int pointId in pointIdOverallDensity.Keys)
                    {
                        Dictionary<string, float> incidentDensity = new Dictionary<string, float>();
                        incidentDensity.Add(incident, pointIdOverallDensity[pointId]);
                        pointIdIncidentDensity.Add(pointId, incidentDensity);
                    }
                }
                else
                    foreach (string incidentType in IncidentTypes)
                    {
                        incidentPoints = new List<PostGIS.Point>(Incident.Get(TrainingStart, TrainingEnd, predictionArea, incidentType).Select(i => i.Location));

                        Console.Out.WriteLine("Running KDE for incident \"" + incidentType + "\"");

                        density = GetDensityEstimate(incidentPoints, TrainingSampleSize, false, 0, 0, nullPoints, _normalize);
                        if (density.Count > 0)
                        {
                            pointNum = 0;
                            foreach (int nullPointId in nullPointIds)
                            {
                                pointIdIncidentDensity.EnsureContainsKey(nullPointId, typeof(Dictionary<string, float>));
                                pointIdIncidentDensity[nullPointId].Add(incidentType, density[pointNum++]);
                            }
                        }
                    }

                PointPrediction.CreateTable(prediction.Id);
                PointPrediction.Insert(GetPointPredictionValues(pointIdOverallDensity, pointIdIncidentDensity), prediction.Id, false);

                Smooth(prediction);
            }
            finally
            {
                DB.Connection.Return(connection);
            }
        }

        public override string GetDetails(Prediction prediction)
        {
            return "";
        }

        public override int Copy()
        {
            return Create(Name, PointSpacing, TrainingArea, TrainingStart, TrainingEnd, TrainingSampleSize, PredictionSampleSize, IncidentTypes, _normalize, Smoothers);
        }

        public override void UpdateFeatureIdsFrom(DiscreteChoiceModel original)
        {
        }

        public override string ToString()
        {
            return "KDE DCM:  " + Name;
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Normalize:  " + _normalize;
        }

        public override string GetPointIdForLog(int id, DateTime time)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }

        public override Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, LAIR.Collections.Generic.Set<string> pointIds = null)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }

        public override void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }
    }
}
