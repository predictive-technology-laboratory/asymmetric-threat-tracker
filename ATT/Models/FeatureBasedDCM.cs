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
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;
using NpgsqlTypes;
using LAIR.Extensions;

using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.Collections.Generic;
using System.Drawing;
using PTL.ATT.Evaluation;
using LAIR.MachineLearning;
using System.Threading;
using System.IO;
using PTL.ATT.Smoothers;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using LAIR.XML;
using LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.Models
{
    [Serializable]
    public class FeatureBasedDCM : DiscreteChoiceModel, IFeatureBasedDCM
    {
        public enum FeatureType
        {
            /// <summary>
            /// Density of geometries in a shapefile
            /// </summary>
            GeometryDensity,

            /// <summary>
            /// Value of incident density derived from the training incidents
            /// </summary>
            IncidentKernelDensityEstimate,

            /// <summary>
            /// Shortest distance to geometries in a shapefile
            /// </summary>
            MinimumDistanceToGeometry
        }

        public static IEnumerable<Feature> GetAvailableFeatures(Area area)
        {
            // spatial distance features
            foreach (Shapefile shapefile in Shapefile.GetForSRID(area.SRID).OrderBy(s => s.Name))
                if (shapefile.Type == Shapefile.ShapefileType.Feature)
                {
                    Dictionary<string, string> parameterValue = new Dictionary<string, string>();
                    parameterValue.Add("Lag days", "30");
                    yield return new Feature(typeof(FeatureType), FeatureType.MinimumDistanceToGeometry, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (distance)", parameterValue);
                }

            // spatial density features
            foreach (Shapefile shapefile in Shapefile.GetForSRID(area.SRID).OrderBy(s => s.Name))
                if (shapefile.Type == Shapefile.ShapefileType.Feature)
                {
                    Dictionary<string, string> parameterValue = new Dictionary<string, string>();
                    parameterValue.Add("Sample size", "500");
                    parameterValue.Add("Lag days", "30");
                    yield return new Feature(typeof(FeatureType), FeatureType.GeometryDensity, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (density)", parameterValue);
                }

            // incident density features
            foreach (string incidentType in Incident.GetUniqueTypes(DateTime.MinValue, DateTime.MaxValue, area).OrderBy(i => i))
            {
                Dictionary<string, string> parameterValue = new Dictionary<string, string>();
                parameterValue.Add("Sample size", "500");
                parameterValue.Add("Lag days", "30");
                yield return new Feature(typeof(FeatureType), FeatureType.IncidentKernelDensityEstimate, incidentType, incidentType, "\"" + incidentType + "\" density", parameterValue);
            }

            // external features
            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(null, typeof(FeatureBasedDCM));
            if (externalFeatureExtractor != null)
                foreach (Feature f in externalFeatureExtractor.GetAvailableFeatures(area))
                    yield return f;
        }

        internal static IEnumerable<Tuple<string, Parameter>> GetPointPredictionValues(FeatureVectorList featureVectors)
        {
            int pointNum = 0; // must use this because point IDs get repeated for the timeslice model
            foreach (FeatureVector featureVector in featureVectors)
            {
                Point point = featureVector.DerivedFrom as Point;
                string timeParameterName = "@time_" + pointNum++;
                IEnumerable<KeyValuePair<string, double>> incidentScore = point.PredictionConfidenceScores.Where(kvp => kvp.Key != PointPrediction.NullLabel).Select(kvp => new KeyValuePair<string, double>(kvp.Key, kvp.Value));
                yield return new Tuple<string, Parameter>(PointPrediction.GetValue(point.Id, timeParameterName, incidentScore, incidentScore.Sum(kvp => kvp.Value)), new Parameter(timeParameterName, NpgsqlDbType.Timestamp, point.Time));
            }
        }

        public static FeatureExtractor InitializeExternalFeatureExtractor(IFeatureBasedDCM model, Type modelType)
        {
            FeatureExtractor externalFeatureExtractor;
            if (Configuration.TryGetFeatureExtractor(modelType, out externalFeatureExtractor))
                externalFeatureExtractor.Initialize(model, modelType, Configuration.GetFeatureExtractorConfigOptions(modelType));

            return externalFeatureExtractor;
        }

        private PTL.ATT.Classifiers.Classifier _classifier;
        private int _featureDistanceThreshold;
        private List<Feature> _features;
        private int _trainingSampleSize;
        private int _predictionSampleSize;
        private Dictionary<string, NumericFeature> _idNumericFeature;
        private Dictionary<string, NominalFeature> _idNominalFeature;

        public PTL.ATT.Classifiers.Classifier Classifier
        {
            get { return _classifier; }
            set
            {
                _classifier = value;
                _classifier.Model = this;
                Update();
            }
        }

        public int FeatureDistanceThreshold
        {
            get { return _featureDistanceThreshold; }
            set
            {
                _featureDistanceThreshold = value;
                Update();
            }
        }

        public List<Feature> Features
        {
            get { return _features; }
            set
            {
                _features = value;

                _idNumericFeature.Clear();
                _idNominalFeature.Clear();
                foreach (Feature f in _features)
                {
                    _idNumericFeature.Add(f.Id, new NumericFeature(f.Id));
                    _idNominalFeature.Add(f.Id, new NominalFeature(f.Id));
                }

                Update();
            }
        }

        public int TrainingSampleSize
        {
            get { return _trainingSampleSize; }
            set
            {
                _trainingSampleSize = value;
                Update();
            }
        }

        public int PredictionSampleSize
        {
            get { return _predictionSampleSize; }
            set
            {
                _predictionSampleSize = value;
                Update();
            }
        }

        protected Dictionary<string, NumericFeature> IdNumericFeature
        {
            get { return _idNumericFeature; }
        }

        protected Dictionary<string, NominalFeature> IdNominalFeature
        {
            get { return _idNominalFeature; }
        }

        public FeatureBasedDCM()
            : base()
        {
            Construct();
        }

        public FeatureBasedDCM(string name,
                               int pointSpacing,
                               IEnumerable<string> incidentTypes,
                               Area trainingArea,
                               DateTime trainingStart,
                               DateTime trainingEnd,
                               IEnumerable<Smoother> smoothers,
                               int featureDistanceThreshold,
                               int trainingSampleSize,
                               int predictionSampleSize,
                               PTL.ATT.Classifiers.Classifier classifier,
                               IEnumerable<Feature> features)
            : base(name, pointSpacing, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers)
        {
            _featureDistanceThreshold = featureDistanceThreshold;
            _trainingSampleSize = trainingSampleSize;
            _predictionSampleSize = predictionSampleSize;
            _classifier = classifier;
            _classifier.Model = this;

            Construct(); // initializes feature lookups

            Features = new List<Feature>(features); // saves features and fills feature lookups

            Update();
        }

        private void Construct()
        {
            _idNumericFeature = new Dictionary<string, NumericFeature>();
            _idNominalFeature = new Dictionary<string, NominalFeature>();
        }

        protected virtual void InsertPointsIntoPrediction(NpgsqlConnection connection, Prediction prediction, bool training, bool vacuum)
        {
            Area area = training ? prediction.Model.TrainingArea : prediction.Model.PredictionArea;

            List<Tuple<PostGIS.Point, string, DateTime>> incidentPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            Set<Tuple<double, double>> incidentLocations = new Set<Tuple<double, double>>(false);
            foreach (Incident i in Incident.Get(prediction.Model.TrainingStart, prediction.Model.TrainingEnd, area, prediction.Model.IncidentTypes.ToArray()))
            {
                incidentPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(new PostGIS.Point(i.Location.X, i.Location.Y, area.SRID), training ? i.Type : PointPrediction.NullLabel, training ? i.Time : DateTime.MinValue)); // training points are labeled and have a time associated with them
                incidentLocations.Add(new Tuple<double, double>(i.Location.X, i.Location.Y));
            }

            Set<int> incidentPointIndexesInArea = new Set<int>(area.Contains(incidentPoints.Select(p => p.Item1).ToList()).ToArray());
            incidentPoints = incidentPoints.Where((p, i) => incidentPointIndexesInArea.Contains(i)).ToList();

            List<Tuple<PostGIS.Point, string, DateTime>> nullPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            double areaMinX = area.BoundingBox.MinX;
            double areaMaxX = area.BoundingBox.MaxX;
            double areaMinY = area.BoundingBox.MinY;
            double areaMaxY = area.BoundingBox.MaxY;
            for (double x = areaMinX + prediction.Model.PointSpacing / 2d; x <= areaMaxX; x += prediction.Model.PointSpacing) // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + prediction.Model.PointSpacing / 2d; y <= areaMaxY; y += prediction.Model.PointSpacing)
                {
                    Tuple<double, double> location = new Tuple<double, double>(x, y);
                    PostGIS.Point point = new PostGIS.Point(x, y, area.SRID);
                    if (!incidentLocations.Contains(location))
                        nullPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(point, PointPrediction.NullLabel, DateTime.MinValue)); // null points are never labeled and never have times
                }

            List<int> nullPointIds = Point.Insert(connection, nullPoints, prediction.Id, area, true, vacuum);

            int maxSampleSize = training ? _trainingSampleSize : _predictionSampleSize;
            int numIncidentsToRemove = (nullPointIds.Count + incidentPoints.Count) - maxSampleSize;
            string sample = training ? "training" : "prediction";
            if (numIncidentsToRemove > 0)
                if (incidentPoints.Count > 0)
                {
                    numIncidentsToRemove = Math.Min(incidentPoints.Count, numIncidentsToRemove);
                    if (numIncidentsToRemove > 0)
                    {
                        Console.Out.WriteLine("WARNING:  the " + sample + " sample size is too large. We are forced to remove " + numIncidentsToRemove + " random incidents in order to meet the required sample size. In order to use all incidents, increase the sample size to " + (nullPointIds.Count + incidentPoints.Count));
                        incidentPoints.Randomize(new Random(1240894));
                        incidentPoints.RemoveRange(0, numIncidentsToRemove);
                    }
                }
                else
                    Console.Out.WriteLine("WARNING:  we are using " + nullPointIds.Count + " points, but the maximum " + sample + " sample size is " + maxSampleSize + ". Be aware that this could be going beyond the memory limits of your machine.");

            if (training && incidentPoints.Count == 0)
                Console.Out.WriteLine("WARNING:  Zero positive incident points retrieved for \"" + prediction.Model.IncidentTypes.Concatenate(", ") + "\" during the training period \"" + prediction.Model.TrainingStart.ToShortDateString() + " " + prediction.Model.TrainingStart.ToShortTimeString() + " -- " + prediction.Model.TrainingEnd.ToShortDateString() + " " + prediction.Model.TrainingEnd.ToShortTimeString() + "\"");

            Point.Insert(connection, incidentPoints, prediction.Id, area, false, vacuum);
        }

        protected virtual int GetNumFeaturesExtractedFor(Prediction prediction)
        {
            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(FeatureBasedDCM));
            return Features.Count(f => f.EnumType == typeof(FeatureType)) + (externalFeatureExtractor == null ? 0 : externalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(FeatureBasedDCM)));
        }

        /// <summary>
        /// Extracts feature vectors from points in a time range.
        /// </summary>
        /// <param name="prediction">Prediction to extract vectors for.</param>
        /// <param name="training">Whether or not this is the training phase.</param>
        /// <param name="start">Start time (points without a time are always included).</param>
        /// <param name="end">End time (points without a time are always included).</param>
        /// <returns></returns>
        protected virtual IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training, DateTime start, DateTime end)
        {
            // this can be called concurrently, so lock on prediction to get the point objects and their vectors
            FeatureVectorList featureVectors;
            Dictionary<int, FeatureVector> pointIdFeatureVector;
            int numFeatures;
            lock (prediction)
            {
                prediction.ReleasePoints(); // so that we get new point objects each time -- their times might be modified by a sub-class (e.g., TimeSliceDCM).
                featureVectors = new FeatureVectorList(prediction.Points.Count);
                pointIdFeatureVector = new Dictionary<int, FeatureVector>(prediction.Points.Count);
                numFeatures = GetNumFeaturesExtractedFor(prediction);
                foreach (Point point in prediction.Points)
                    if (point.Time == DateTime.MinValue || (point.Time >= start && point.Time <= end))
                    {
                        point.TrueClass = point.IncidentType;
                        FeatureVector vector = new FeatureVector(point, numFeatures);
                        featureVectors.Add(vector);
                        pointIdFeatureVector.Add(point.Id, vector);
                    }
            }

            Area area = training ? prediction.Model.TrainingArea : prediction.PredictionArea;
            Set<Thread> threads = new Set<Thread>();

            #region spatial distance features
            List<Feature> spatialDistanceFeatures = Features.Where(f => f.EnumValue.Equals(FeatureType.MinimumDistanceToGeometry)).ToList();
            if (spatialDistanceFeatures.Count > 0)
            {
                Console.Out.WriteLine("Extracting spatial distance feature values");
                float distanceWhenBeyondThreshold = (float)Math.Sqrt(2.0 * Math.Pow(FeatureDistanceThreshold, 2)); // with a bounding box of FeatureDistanceThreshold around each point, the maximum distance between a point and some feature shapefile geometry would be sqrt(2*FeatureDistanceThreshold^2). That is, the feature shapefile geometry would be positioned in one of the corners of the bounding box.
                threads.Clear();
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int core = (int)o;
                            NpgsqlConnection threadConnection = DB.Connection.OpenConnection;
                            string pointTableName = Point.GetTableName(prediction.Id);
                            foreach (Feature spatialDistanceFeature in spatialDistanceFeatures)
                            {
                                Shapefile shapefile = new Shapefile(int.Parse(training ? spatialDistanceFeature.TrainingResourceId : spatialDistanceFeature.PredictionResourceId));
                                string shapefileGeometryTableName = ShapefileGeometry.GetTableName(shapefile);
                                NpgsqlCommand cmd = new NpgsqlCommand("SELECT points." + Point.Columns.Id + " as points_" + Point.Columns.Id + "," +
                                                                             "CASE WHEN COUNT(" + shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Geometry + ")=0 THEN " + distanceWhenBeyondThreshold + " " +
                                                                             "ELSE min(st_distance(st_closestpoint(" + shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Geometry + ",points." + Point.Columns.Location + "),points." + Point.Columns.Location + ")) " +
                                                                             "END as feature_value " +

                                                                      "FROM (SELECT *,st_expand(" + pointTableName + "." + Point.Columns.Location + "," + FeatureDistanceThreshold + ") as bounding_box " +
                                                                            "FROM " + pointTableName + " " +
                                                                            "WHERE " + pointTableName + "." + Point.Columns.Core + "=" + core + " AND " +
                                                                                       "(" +
                                                                                          pointTableName + "." + Point.Columns.Time + "='-infinity'::timestamp OR " +
                                                                                            "(" +
                                                                                                pointTableName + "." + Point.Columns.Time + ">=@point_start AND " +
                                                                                                pointTableName + "." + Point.Columns.Time + "<=@point_end" +
                                                                                            ")" +
                                                                                       ")" +
                                                                            ") points " +

                                                                            "LEFT JOIN " + shapefileGeometryTableName + " " +

                                                                            "ON points.bounding_box && " + shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Geometry + " AND " +
                                                                                "(" +
                                                                                    shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp OR " +
                                                                                    "(" +
                                                                                        shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Time + ">=@geometry_start AND " +
                                                                                        shapefileGeometryTableName + "." + ShapefileGeometry.Columns.Time + "<=@geometry_end" +
                                                                                    ")" +
                                                                                ")" +

                                                                      "GROUP BY points." + Point.Columns.Id, threadConnection);

                                TimeSpan spatialDistanceFeatureLag = new TimeSpan(spatialDistanceFeature.GetIntegerParameterValue("Lag days"), 0, 0, 0);
                                ConnectionPool.AddParameters(cmd, new Parameter("point_start", NpgsqlDbType.Timestamp, start),
                                                                  new Parameter("point_end", NpgsqlDbType.Timestamp, end),
                                                                  new Parameter("geometry_start", NpgsqlDbType.Timestamp, start - spatialDistanceFeatureLag),
                                                                  new Parameter("geometry_end", NpgsqlDbType.Timestamp, start - new TimeSpan(1)));

                                NpgsqlDataReader reader = cmd.ExecuteReader();
                                NumericFeature distanceFeature = _idNumericFeature[spatialDistanceFeature.Id];
                                while (reader.Read())
                                {
                                    FeatureVector vector = pointIdFeatureVector[Convert.ToInt32(reader["points_" + Point.Columns.Id])];
                                    double value = Convert.ToDouble(reader["feature_value"]);

                                    // value > threshold shouldn't happen here, since we exluced such objects from consideration above; however, the calculations aren't perfect in postgis, so we check again and reset appropriately
                                    if (value > distanceWhenBeyondThreshold)
                                        value = distanceWhenBeyondThreshold;

                                    vector.Add(distanceFeature, value, false); // don't update range here because the features are being accessed concurrently
                                }
                                reader.Close();
                            }

                            DB.Connection.Return(threadConnection);
                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                // update feature ranges since they weren't updated above due to concurrent access
                foreach (FeatureVector vector in featureVectors)
                    foreach (LAIR.MachineLearning.Feature f in vector)
                        f.UpdateRange(vector[f]);
            }
            #endregion

            #region spatial density features
            List<Feature> spatialDensityFeatures = Features.Where(f => f.EnumValue.Equals(FeatureType.GeometryDensity)).ToList();
            if (spatialDensityFeatures.Count > 0)
            {
                List<PostGIS.Point> densityEvalPoints = featureVectors.Select(v => (v.DerivedFrom as Point).Location).ToList();
                Dictionary<string, List<float>> featureIdDensityEstimates = new Dictionary<string, List<float>>(spatialDensityFeatures.Count);
                threads.Clear();
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int skip = (int)o;
                            NpgsqlConnection connection = DB.Connection.OpenConnection;
                            foreach (Feature spatialDensityFeature in spatialDensityFeatures)
                                if (skip-- <= 0)
                                {
                                    Shapefile shapefile = new Shapefile(int.Parse(training ? spatialDensityFeature.TrainingResourceId : spatialDensityFeature.PredictionResourceId));
                                    Console.Out.WriteLine("Computing spatial density of \"" + shapefile.Name + "\".");

                                    string geometryRecordWhereClause = "WHERE " + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp OR (" + ShapefileGeometry.Columns.Time + ">=@geometry_start AND " + ShapefileGeometry.Columns.Time + "<=@geometry_end)";
                                    TimeSpan spatialDensityFeatureLag = new TimeSpan(spatialDensityFeature.GetIntegerParameterValue("Lag days"), 0, 0, 0);
                                    Parameter geometryStart = new Parameter("geometry_start", NpgsqlDbType.Timestamp, start - spatialDensityFeatureLag);
                                    Parameter geometryEnd = new Parameter("geometry_end", NpgsqlDbType.Timestamp, start - new TimeSpan(1));
                                    List<PostGIS.Point> kdeInputPoints = Geometry.GetPoints(connection, ShapefileGeometry.GetTableName(shapefile), ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, geometryRecordWhereClause, -1, geometryStart.NpgsqlParameter, geometryEnd.NpgsqlParameter).SelectMany(pointList => pointList).Select(p => new PostGIS.Point(p.X, p.Y, area.SRID)).ToList();
                                    int sampleSize = spatialDensityFeature.GetIntegerParameterValue("Sample size");
                                    List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, sampleSize, false, -1, -1, densityEvalPoints, true);
                                    if (densityEstimates.Count == densityEvalPoints.Count)
                                        lock (featureIdDensityEstimates) { featureIdDensityEstimates.Add(spatialDensityFeature.Id, densityEstimates); }

                                    skip = Configuration.ProcessorCount - 1;
                                }

                            DB.Connection.Return(connection);
                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                foreach (string featureId in featureIdDensityEstimates.Keys)
                {
                    List<float> densityEstimates = featureIdDensityEstimates[featureId];
                    NumericFeature densityFeature = _idNumericFeature[featureId];
                    for (int i = 0; i < densityEstimates.Count; ++i)
                        featureVectors[i].Add(densityFeature, densityEstimates[i]);
                }
            }
            #endregion

            #region incident density features
            List<Feature> kdeFeatures = Features.Where(f => f.EnumValue.Equals(FeatureType.IncidentKernelDensityEstimate)).ToList();
            if (kdeFeatures.Count > 0)
            {
                List<PostGIS.Point> densityEvalPoints = featureVectors.Select(v => (v.DerivedFrom as Point).Location).ToList();
                Dictionary<string, List<float>> featureIdDensityEstimates = new Dictionary<string, List<float>>(kdeFeatures.Count);
                threads.Clear();
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int skip = (int)o;
                            foreach (Feature kdeFeature in kdeFeatures)
                                if (skip-- <= 0)
                                {
                                    string incident = training ? kdeFeature.TrainingResourceId : kdeFeature.PredictionResourceId;

                                    Console.Out.WriteLine("Computing spatial density of \"" + incident + "\"");

                                    TimeSpan kdeFeatureLag = new TimeSpan(kdeFeature.GetIntegerParameterValue("Lag days"), 0, 0, 0);
                                    IEnumerable<PostGIS.Point> kdeInputPoints = Incident.Get(start - kdeFeatureLag, start - new TimeSpan(1), area, incident).Select(inc => inc.Location);
                                    int sampleSize = kdeFeature.GetIntegerParameterValue("Sample size");
                                    List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, sampleSize, false, 0, 0, densityEvalPoints, true);
                                    if (densityEstimates.Count == densityEvalPoints.Count)
                                        lock (featureIdDensityEstimates) { featureIdDensityEstimates.Add(kdeFeature.Id, densityEstimates); }

                                    skip = Configuration.ProcessorCount - 1;
                                }
                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                foreach (string featureId in featureIdDensityEstimates.Keys)
                {
                    List<float> densityEstimates = featureIdDensityEstimates[featureId];
                    NumericFeature densityFeature = _idNumericFeature[featureId];
                    for (int i = 0; i < densityEstimates.Count; ++i)
                        featureVectors[i].Add(densityFeature, densityEstimates[i]);
                }
            }
            #endregion

            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(FeatureBasedDCM));
            if (externalFeatureExtractor != null)
            {
                Console.Out.WriteLine("Running external feature extractor for " + typeof(FeatureBasedDCM));
                foreach (FeatureVectorList externalFeatureVectors in externalFeatureExtractor.ExtractFeatures(prediction, featureVectors, training, start, end))
                    yield return externalFeatureVectors;
            }
            else
                yield return featureVectors;
        }

        public void SelectFeatures(Prediction prediction, bool runPredictionAfterSelect)
        {
            _classifier.Initialize();

            Console.Out.WriteLine("Selecting features");
            Set<string> selectedFeatureIds = new Set<string>(_classifier.SelectFeatures(prediction).ToArray());
            Features = Features.Where(f => selectedFeatureIds.Contains(f.Id)).ToList();

            if (runPredictionAfterSelect)
            {
                Console.Out.WriteLine("Re-running prediction");
                PointPrediction.DeleteTable(prediction.Id);
                Point.DeleteTable(prediction.Id);
                prediction.ReleaseAllLazyLoadedData();
                Run(prediction, true, false, true);
                prediction.MostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
            }
        }

        protected override void Run(Prediction prediction)
        {
            Run(prediction, true, _classifier.RunFeatureSelection, true);
        }

        private int Run(Prediction prediction, bool train, bool runFeatureSelection, bool predict)
        {
            if (Features.Count == 0)
                throw new Exception("Must select one or more features.");

            // all features must reference a shapefile that is valid for the prediction area's SRID -- might not be the case because we allow remapping
            Set<int> shapefilesInPredictionSRID = new Set<int>(Shapefile.GetForSRID(prediction.PredictionArea.SRID).Select(s => s.Id).ToArray());
            string badFeatures = Features.Where(f => (f.EnumValue.Equals(FeatureType.MinimumDistanceToGeometry) || f.EnumValue.Equals(FeatureType.GeometryDensity)) &&
                                                     !shapefilesInPredictionSRID.Contains(int.Parse(f.PredictionResourceId))).Select(f => f.ToString()).Concatenate(",");
            if (badFeatures.Length > 0)
                throw new Exception("Features \"" + badFeatures + "\" are not valid for the prediction area (" + prediction.PredictionArea.Name + "). These features must be remapped for prediction (or perhaps they were remapped incorrectly).");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            try
            {
                _classifier.Initialize();

                #region training
                if (train)
                {
                    Console.Out.WriteLine("Creating training grid");

                    Point.CreateTable(prediction.Id, prediction.Model.TrainingArea.SRID);
                    InsertPointsIntoPrediction(cmd.Connection, prediction, true, true);

                    #region feature selection
                    if (runFeatureSelection)
                    {
                        Console.Out.WriteLine("Running feature selection");

                        foreach (FeatureVectorList vectors in ExtractFeatureVectors(prediction, true, TrainingStart, TrainingEnd))
                            _classifier.Consume(vectors);

                        _classifier.Train();

                        SelectFeatures(prediction, false);
                    }
                    #endregion

                    foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, true, TrainingStart, TrainingEnd))
                        _classifier.Consume(featureVectors);

                    Console.Out.WriteLine("Training model");

                    _classifier.Train();

                    Point.DeleteTable(prediction.Id);
                    prediction.ReleaseAllLazyLoadedData();
                }
                #endregion

                #region prediction
                if (predict)
                {
                    Console.Out.WriteLine("Creating prediction grid");

                    Point.CreateTable(prediction.Id, prediction.PredictionArea.SRID);
                    InsertPointsIntoPrediction(cmd.Connection, prediction, false, true);

                    PointPrediction.CreateTable(prediction.Id);
                    using (FileStream pointPredictionLogFile = new FileStream(prediction.PointPredictionLogPath, FileMode.Create, FileAccess.Write))
                    using (GZipStream pointPredictionLogGzip = new GZipStream(pointPredictionLogFile, CompressionMode.Compress))
                    using (StreamWriter pointPredictionLog = new StreamWriter(pointPredictionLogGzip))
                    {
                        foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, false, prediction.PredictionStartTime, prediction.PredictionEndTime))
                        {
                            Console.Out.WriteLine("Making predictions");

                            _classifier.Classify(featureVectors);

                            #region log feature values
                            foreach (FeatureVector vector in featureVectors.OrderBy(v => (v.DerivedFrom as Point).Id))  // sort by point ID so prediction log is sorted
                            {
                                Point p = vector.DerivedFrom as Point;
                                if (p == null)
                                    throw new NullReferenceException("Expected Point in vector.DerivedFrom");

                                pointPredictionLog.Write(GetPointIdForLog(p.Id, p.Time) + " <p><ls>");
                                foreach (string label in vector.DerivedFrom.PredictionConfidenceScores.SortKeysByValues(true))
                                    if (label == PointPrediction.NullLabel || prediction.Model.IncidentTypes.Contains(label))
                                        pointPredictionLog.Write("<l c=\"" + Math.Round(vector.DerivedFrom.PredictionConfidenceScores[label], 3) + "\"><![CDATA[" + label + "]]></l>");
                                    else
                                        throw new Exception("Invalid prediction label on point:  " + label);

                                pointPredictionLog.Write("</ls><fvs>");
                                foreach (LAIR.MachineLearning.Feature feature in vector)
                                {
                                    object value;
                                    if (feature is NumericFeature)
                                        value = Math.Round(Convert.ToSingle(vector[feature]), 3);
                                    else
                                        value = vector[feature].ToString();

                                    pointPredictionLog.Write("<fv id=\"" + feature.Name + "\">" + value + "</fv>");
                                }

                                pointPredictionLog.WriteLine("</fvs></p>");
                            }
                            #endregion

                            PointPrediction.Insert(GetPointPredictionValues(featureVectors), prediction.Id, true);
                        }

                        pointPredictionLog.Close();
                    }

                    Smooth(prediction);
                }
                #endregion

                return prediction.Id;
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);
            }
        }

        public override string GetPointIdForLog(int id, DateTime time)
        {
            return id.ToString();
        }

        /// <summary>
        /// Reads the point log for this prediction. The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.
        /// </summary>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        /// <param name="pointIds">Point IDs to read log for, or null for all points.</param>
        /// <returns></returns>
        public override Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, Set<string> pointIds = null)
        {
            Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> log = new Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>>();

            using (FileStream pointPredictionLogFile = new FileStream(pointPredictionLogPath, FileMode.Open, FileAccess.Read))
            using (GZipStream pointPredictionLogGzip = new GZipStream(pointPredictionLogFile, CompressionMode.Decompress))
            using (StreamReader pointPredictionLog = new StreamReader(pointPredictionLogGzip))
            {
                string line;
                while ((line = pointPredictionLog.ReadLine()) != null)
                {
                    string pointId = line.Substring(0, line.IndexOf(' '));

                    if (pointIds == null || pointIds.Contains(pointId))
                    {
                        XmlParser pointP = new XmlParser(line.Substring(line.IndexOf(' ') + 1));

                        List<Tuple<string, double>> labelConfidences = new List<Tuple<string, double>>();
                        XmlParser labelsP = new XmlParser(pointP.OuterXML("ls"));
                        string labelXML;
                        while ((labelXML = labelsP.OuterXML("l")) != null)
                        {
                            XmlParser labelP = new XmlParser(labelXML);
                            double confidence = double.Parse(labelP.AttributeValue("l", "c"));
                            string label = labelP.ElementText("l");
                            labelConfidences.Add(new Tuple<string, double>(label, confidence));
                        }

                        List<Tuple<string, double>> featureValues = new List<Tuple<string, double>>();
                        XmlParser featureValuesP = new XmlParser(pointP.OuterXML("fvs"));
                        string featureValueXML;
                        while ((featureValueXML = featureValuesP.OuterXML("fv")) != null)
                        {
                            XmlParser featureValueP = new XmlParser(featureValueXML);
                            featureValues.Add(new Tuple<string, double>(featureValueP.AttributeValue("fv", "id"), double.Parse(featureValueP.ElementText("fv"))));
                        }

                        log.Add(pointId, new Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>(labelConfidences, featureValues));

                        if (pointIds != null)
                        {
                            pointIds.Remove(pointId);
                            if (pointIds.Count == 0)
                                break;
                        }
                    }
                }

                pointPredictionLog.Close();
            }

            return log;
        }

        /// <summary>
        /// Writes the point log for this prediction.
        /// </summary>
        /// <param name="pointIdLabelsFeatureValues">The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.</param>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        public override void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath)
        {
            using (StreamWriter pointPredictionLog = new StreamWriter(new GZipStream(new FileStream(pointPredictionLogPath, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                foreach (string pointId in pointIdLabelsFeatureValues.Keys.OrderBy(k => k))
                {
                    pointPredictionLog.Write(pointId + " <p><ls>");
                    foreach (Tuple<string, double> labelConfidence in pointIdLabelsFeatureValues[pointId].Item1)
                        pointPredictionLog.Write("<l c=\"" + labelConfidence.Item2 + "\"><![CDATA[" + labelConfidence.Item1 + "]]></l>");

                    pointPredictionLog.Write("</ls><fvs>");
                    foreach (Tuple<string, double> featureIdValue in pointIdLabelsFeatureValues[pointId].Item2)
                        pointPredictionLog.Write("<fv id=\"" + featureIdValue.Item1 + "\">" + featureIdValue.Item2 + "</fv>");

                    pointPredictionLog.WriteLine("</fvs></p>");
                }

                pointPredictionLog.Close();
            }
        }

        public override string GetDetails(Prediction prediction)
        {
            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(FeatureBasedDCM));
            string details = _classifier.GetDetails(prediction, externalFeatureExtractor == null ? null : externalFeatureExtractor.GetDetails(prediction));

            prediction.ModelDetails = details;

            return prediction.ModelDetails;
        }

        public override DiscreteChoiceModel Copy()
        {
            return new FeatureBasedDCM(Name, PointSpacing, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, _featureDistanceThreshold, _trainingSampleSize, _predictionSampleSize, _classifier.Copy(), _features);
        }

        public override string ToString()
        {
            return "Distance DCM:  " + Name;
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            int featuresToDisplay = 10; // can have hundreds of features, which makes the tooltip excrutiatingly slow
            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(FeatureBasedDCM));
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Classifier:  " + _classifier.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Features:  " + Features.Where((f, i) => i < featuresToDisplay).Select(f => f.ToString()).Concatenate(", ") + (Features.Count > featuresToDisplay ? " ... (" + (Features.Count - featuresToDisplay) + " not shown)" : "") + Environment.NewLine +
                   indent + "External feature extractor (" + typeof(FeatureBasedDCM) + "):  " + (externalFeatureExtractor == null ? "None" : externalFeatureExtractor.GetDetails(indentLevel + 1)) + Environment.NewLine +
                   indent + "Feature distance threshold:  " + _featureDistanceThreshold + Environment.NewLine +
                   indent + "Training sample size:  " + _trainingSampleSize + Environment.NewLine +
                   indent + "Prediction sample size:  " + _predictionSampleSize;
        }
    }
}
