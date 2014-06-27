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
    [Serializable]
    public class KernelDensityDCM : DiscreteChoiceModel
    {
        static KernelDensityDCM()
        {
            if (Configuration.RCranMirror != null)
                R.InstallPackages(R.CheckForMissingPackages(new string[] { "ks" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
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
            using (StreamWriter inputPointsFile = new StreamWriter(inputPointsPath))
            {
                foreach (PostGIS.Point inputPoint in inputPoints)
                    inputPointsFile.Write(inputPoint.X + "," + inputPoint.Y + "\n");
                inputPointsFile.Close();
            }

            inputPoints = null;

            string evalPointsPath = Path.GetTempFileName();
            int numEvalPoints = 0;
            using (StreamWriter evalPointsFile = new StreamWriter(evalPointsPath))
            {
                foreach (PostGIS.Point evalPoint in evalPoints)
                {
                    evalPointsFile.Write(evalPoint.X + "," + evalPoint.Y + "\n");
                    ++numEvalPoints;
                }
                evalPointsFile.Close();
            }

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

        private static IEnumerable<Tuple<string, Parameter>> GetPointPredictionValues(Dictionary<int, float> pointIdOverallDensity, Dictionary<int, Dictionary<string, double>> pointIdIncidentDensity)
        {
            foreach (int pointId in pointIdIncidentDensity.Keys)
            {
                string timeParameterName = "@time_" + pointId;
                yield return new Tuple<string, Parameter>(PointPrediction.GetValue(pointId, timeParameterName, pointIdIncidentDensity[pointId], pointIdOverallDensity[pointId]), new Parameter(timeParameterName, NpgsqlDbType.Timestamp, DateTime.MinValue));
            }
        }

        private int _trainingSampleSize;
        private bool _normalize;

        public int TrainingSampleSize
        {
            get { return _trainingSampleSize; }
            set
            {
                _trainingSampleSize = value;
                Update();
            }
        }

        public bool Normalize
        {
            get { return _normalize; }
            set
            {
                _normalize = value;
                Update();
            }
        }

        public KernelDensityDCM() : base() { }

        public KernelDensityDCM(string name,
                                int pointSpacing,
                                IEnumerable<string> incidentTypes,
                                Area trainingArea,
                                DateTime trainingStart,
                                DateTime trainingEnd,
                                IEnumerable<Smoother> smoothers,
                                int trainingSampleSize,
                                bool normalize)
            : base(name, pointSpacing, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers)
        {
            _trainingSampleSize = trainingSampleSize;
            _normalize = normalize;

            Update();
        }

        protected override void Run(Prediction prediction)
        {
            List<PostGIS.Point> nullPoints = new List<PostGIS.Point>();
            Area predictionArea = prediction.PredictionArea;
            double areaMinX = predictionArea.BoundingBox.MinX;
            double areaMaxX = predictionArea.BoundingBox.MaxX;
            double areaMinY = predictionArea.BoundingBox.MinY;
            double areaMaxY = predictionArea.BoundingBox.MaxY;
            for (double x = areaMinX + PointSpacing / 2d; x <= areaMaxX; x += PointSpacing)  // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + PointSpacing / 2d; y <= areaMaxY; y += PointSpacing)
                    nullPoints.Add(new PostGIS.Point(x, y, predictionArea.Shapefile.SRID));

            nullPoints = predictionArea.Contains(nullPoints).Select(i => nullPoints[i]).ToList();

            NpgsqlConnection connection = DB.Connection.OpenConnection;

            try
            {
                Console.Out.WriteLine("Running KDE for all incident types");

                Point.CreateTable(prediction, predictionArea.Shapefile.SRID);
                List<int> nullPointIds = Point.Insert(connection, nullPoints.Select(p => new Tuple<PostGIS.Point, string, DateTime>(p, PointPrediction.NullLabel, DateTime.MinValue)), prediction, predictionArea, false, false);

                List<PostGIS.Point> incidentPoints = new List<PostGIS.Point>(Incident.Get(TrainingStart, TrainingEnd, predictionArea, IncidentTypes.ToArray()).Select(i => i.Location));
                List<float> density = GetDensityEstimate(incidentPoints, _trainingSampleSize, false, 0, 0, nullPoints, _normalize);
                Dictionary<int, float> pointIdOverallDensity = new Dictionary<int, float>();
                int pointNum = 0;
                foreach (int nullPointId in nullPointIds)
                    pointIdOverallDensity.Add(nullPointId, density[pointNum++]);

                Dictionary<int, Dictionary<string, double>> pointIdIncidentDensity = new Dictionary<int,Dictionary<string,double>>();

                if (IncidentTypes.Count == 1)
                {
                    string incident = IncidentTypes.First();
                    foreach (int pointId in pointIdOverallDensity.Keys)
                    {
                        Dictionary<string, double> incidentDensity = new Dictionary<string, double>();
                        incidentDensity.Add(incident, pointIdOverallDensity[pointId]);
                        pointIdIncidentDensity.Add(pointId, incidentDensity);
                    }
                }
                else
                    foreach (string incidentType in IncidentTypes)
                    {
                        incidentPoints = new List<PostGIS.Point>(Incident.Get(TrainingStart, TrainingEnd, predictionArea, incidentType).Select(i => i.Location));

                        Console.Out.WriteLine("Running KDE for incident \"" + incidentType + "\"");

                        density = GetDensityEstimate(incidentPoints, _trainingSampleSize, false, 0, 0, nullPoints, _normalize);
                        if (density.Count > 0)
                        {
                            pointNum = 0;
                            foreach (int nullPointId in nullPointIds)
                            {
                                pointIdIncidentDensity.EnsureContainsKey(nullPointId, typeof(Dictionary<string, double>));
                                pointIdIncidentDensity[nullPointId].Add(incidentType, density[pointNum++]);
                            }
                        }
                    }

                PointPrediction.CreateTable(prediction);
                PointPrediction.Insert(GetPointPredictionValues(pointIdOverallDensity, pointIdIncidentDensity), prediction, false);

                Smooth(prediction);
            }
            finally
            {
                DB.Connection.Return(connection);
            }
        }

        public override string GetDetails(Prediction prediction)
        {
            return "No information is available for " + typeof(KernelDensityDCM).Name + " models.";
        }

        public override DiscreteChoiceModel Copy()
        {
            return new KernelDensityDCM(Name, PointSpacing, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, _trainingSampleSize, _normalize);
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
                   indent + "Normalize:  " + _normalize + Environment.NewLine +
                   indent + "Training sample size:  " + _trainingSampleSize;
        }

        public override string GetPointIdForLog(int id, DateTime time)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }

        public override Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, LAIR.Collections.Generic.Set<string> pointIds = null)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }

        public override void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath)
        {
            throw new NotImplementedException("Point prediction log not implemented for " + GetType().FullName);
        }
    }
}
