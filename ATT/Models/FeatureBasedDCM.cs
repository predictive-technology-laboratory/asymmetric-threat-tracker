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
            IncidentDensity,

            /// <summary>
            /// Shortest distance to geometries in a shapefile
            /// </summary>
            MinimumDistanceToGeometry,

            /// <summary>
            /// Attribute for a geometry
            /// </summary>
            GeometryAttribute
        }

        public enum SpatialDistanceParameter
        {
            LagOffset,
            LagDuration
        }

        public enum SpatialDensityParameter
        {
            LagOffset,
            LagDuration,
            SampleSize,
            DefaultValue
        }

        public enum GeometryAttributeParameter
        {
            AttributeColumn,
            AttributeType,
            DefaultValue
        }

        public enum IncidentDensityParameter
        {
            LagOffset,
            LagDuration,
            LagCount,
            SampleSize,
            DefaultValue
        }

        public static IEnumerable<Feature> GetAvailableFeatures(Area area)
        {
            FeatureParameterCollection parameters;

            // shapefile-based features
            foreach (Shapefile shapefile in Shapefile.GetForSRID(area.Shapefile.SRID).OrderBy(s => s.Name))
                if (shapefile.Type == Shapefile.ShapefileType.Feature)
                {
                    // spatial distance
                    parameters = new FeatureParameterCollection();
                    parameters.Set(SpatialDistanceParameter.LagOffset, "31.00:00:00", "Offset prior to training/prediction window. Format:  DAYS.HH:MM:SS");
                    parameters.Set(SpatialDistanceParameter.LagDuration, "30.23:59:59", "Duration of lag window. Format:  DAYS.HH:MM:SS");
                    yield return new Feature(typeof(FeatureType), FeatureType.MinimumDistanceToGeometry, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (distance)", parameters);

                    // spatial density
                    parameters = new FeatureParameterCollection();
                    parameters.Set(SpatialDensityParameter.LagOffset, "31.00:00:00", "Offset prior to training/prediction window. Format:  DAYS.HH:MM:SS");
                    parameters.Set(SpatialDensityParameter.LagDuration, "30.23:59:59", "Duration of lag window. Format:  DAYS.HH:MM:SS");
                    parameters.Set(SpatialDensityParameter.SampleSize, "500", "Sample size for spatial density estimate.");
                    parameters.Set(SpatialDensityParameter.DefaultValue, "0", "Value to use when density is not computable (e.g., too few spatial objects).");
                    yield return new Feature(typeof(FeatureType), FeatureType.GeometryDensity, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (density)", parameters);

                    // geometry attribute
                    parameters = new FeatureParameterCollection();
                    parameters.Set(GeometryAttributeParameter.AttributeColumn, "", "Name of column within geometry from which to draw value.");
                    parameters.Set(GeometryAttributeParameter.AttributeType, "", "Type of attribute:  Nominal or Numeric");
                    parameters.Set(GeometryAttributeParameter.DefaultValue, "0", "Value to use when geometry does not overlap a model point.");
                    yield return new Feature(typeof(FeatureType), FeatureType.GeometryAttribute, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (attribute)", parameters);
                }

            // incident density features
            foreach (string incidentType in Incident.GetUniqueTypes(DateTime.MinValue, DateTime.MaxValue, area).OrderBy(i => i))
            {
                parameters = new FeatureParameterCollection();
                parameters.Set(IncidentDensityParameter.LagOffset, "31.00:00:00", "Offset prior to training/prediction window. Format:  DAYS.HH:MM:SS");
                parameters.Set(IncidentDensityParameter.LagDuration, "30.23:59:59", "Duration of lag window. Format:  DAYS.HH:MM:SS");
                parameters.Set(IncidentDensityParameter.LagCount, "1", "Number of lags of the given offset and duration to use, with offsets being additive.");
                parameters.Set(IncidentDensityParameter.SampleSize, "500", "Sample size for incident density estimate.");
                parameters.Set(IncidentDensityParameter.DefaultValue, "0", "Value to use when density is not computable (e.g., too few incidents).");
                yield return new Feature(typeof(FeatureType), FeatureType.IncidentDensity, incidentType, incidentType, "\"" + incidentType + "\" density", parameters);
            }

            // external features
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(FeatureBasedDCM));
            if (externalFeatureExtractor != null)
                foreach (Feature f in externalFeatureExtractor.GetAvailableFeatures(area))
                    yield return f;
        }

        internal static List<Tuple<string, Parameter>> GetPointPredictionValues(FeatureVectorList featureVectors)
        {
            List<Tuple<string, Parameter>> pointPredictionValues = new List<Tuple<string, Parameter>>(featureVectors.Count);
            int pointNum = 0; // must use this instead of point IDs because point IDs get repeated for the timeslice model
            foreach (FeatureVector featureVector in featureVectors)
            {
                Point point = featureVector.DerivedFrom as Point;
                string timeParameterName = "@time_" + pointNum++;
                IEnumerable<KeyValuePair<string, double>> incidentScore = point.PredictionConfidenceScores.Where(kvp => kvp.Key != PointPrediction.NullLabel).Select(kvp => new KeyValuePair<string, double>(kvp.Key, kvp.Value));
                pointPredictionValues.Add(new Tuple<string, Parameter>(PointPrediction.GetValue(point.Id, timeParameterName, incidentScore, incidentScore.Sum(kvp => kvp.Value)), new Parameter(timeParameterName, NpgsqlDbType.Timestamp, point.Time)));
            }

            return pointPredictionValues;
        }

        public static IFeatureExtractor InitializeExternalFeatureExtractor(Type modelType)
        {
            IFeatureExtractor externalFeatureExtractor;
            if (Configuration.TryGetFeatureExtractor(modelType, out externalFeatureExtractor))
                externalFeatureExtractor.Initialize(modelType, Configuration.GetFeatureExtractorConfigOptions(modelType));

            return externalFeatureExtractor;
        }

        private PTL.ATT.Classifiers.Classifier _classifier;
        private int _trainingPointSpacing;
        private int _featureDistanceThreshold;
        private int _negativePointStandoff;
        private List<Feature> _features;
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

        public int TrainingPointSpacing
        {
            get { return _trainingPointSpacing; }
            set
            {
                _trainingPointSpacing = value;
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

        public int NegativePointStandoff
        {
            get { return _negativePointStandoff; }
            set
            {
                _negativePointStandoff = value;
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
                               IEnumerable<string> incidentTypes,
                               Area trainingArea,
                               DateTime trainingStart,
                               DateTime trainingEnd,
                               IEnumerable<Smoother> smoothers,
                               int trainingPointSpacing,
                               int featureDistanceThreshold,
                               int negativePointStandoff,
                               PTL.ATT.Classifiers.Classifier classifier,
                               IEnumerable<Feature> features)
            : base(name, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers)
        {
            _trainingPointSpacing = trainingPointSpacing;
            _featureDistanceThreshold = featureDistanceThreshold;
            _negativePointStandoff = negativePointStandoff;
            _classifier = classifier;
            _classifier.Model = this;

            Construct(); // initializes feature lookups

            if (features != null)
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

            // insert positive points
            List<Tuple<PostGIS.Point, string, DateTime>> incidentPointTuples = new List<Tuple<PostGIS.Point, string, DateTime>>();
            foreach (Incident i in Incident.Get(prediction.Model.TrainingStart, prediction.Model.TrainingEnd, area, prediction.Model.IncidentTypes.ToArray()))
                incidentPointTuples.Add(new Tuple<PostGIS.Point, string, DateTime>(new PostGIS.Point(i.Location.X, i.Location.Y, area.Shapefile.SRID), training ? i.Type : PointPrediction.NullLabel, training ? i.Time : DateTime.MinValue)); // training points are labeled and have a time associated with them

            if (training && incidentPointTuples.Count == 0)
                Console.Out.WriteLine("WARNING:  Zero positive incident points retrieved for \"" + prediction.Model.IncidentTypes.Concatenate(", ") + "\" during the training period \"" + prediction.Model.TrainingStart.ToShortDateString() + " " + prediction.Model.TrainingStart.ToShortTimeString() + " -- " + prediction.Model.TrainingEnd.ToShortDateString() + " " + prediction.Model.TrainingEnd.ToShortTimeString() + "\"");

            Point.Insert(connection, incidentPointTuples, prediction, area, vacuum);  // all incidents are constrained to be in the area upon import, so we don't need to filter them before inserting

            // insert negative points
            int negativePointSpacing = training ? _trainingPointSpacing : prediction.PredictionPointSpacing;
            List<Tuple<PostGIS.Point, string, DateTime>> nullPointTuples = new List<Tuple<PostGIS.Point, string, DateTime>>();
            double areaMinX = area.BoundingBox.MinX;
            double areaMaxX = area.BoundingBox.MaxX;
            double areaMinY = area.BoundingBox.MinY;
            double areaMaxY = area.BoundingBox.MaxY;
            for (double x = areaMinX + negativePointSpacing / 2d; x <= areaMaxX; x += negativePointSpacing) // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + negativePointSpacing / 2d; y <= areaMaxY; y += negativePointSpacing)
                {
                    PostGIS.Point point = new PostGIS.Point(x, y, area.Shapefile.SRID);
                    nullPointTuples.Add(new Tuple<PostGIS.Point, string, DateTime>(point, PointPrediction.NullLabel, DateTime.MinValue)); // null points are never labeled and never have time
                }

            // filter out any negative point whose bounding box does not intersect the area
            nullPointTuples = area.Intersects(nullPointTuples.Select(t => t.Item1), negativePointSpacing / 2f).Select(i => nullPointTuples[i]).ToList();

            // filter out any negative point that is too close to a positive point -- only when training since we need all null points during prediction for a continuous surface
            if (training)
                nullPointTuples = nullPointTuples.Where(nullPointTuple => !incidentPointTuples.Any(incidentPointTuple => incidentPointTuple.Item1.DistanceTo(nullPointTuple.Item1) < _negativePointStandoff)).ToList();

            Point.Insert(connection, nullPointTuples, prediction, area, vacuum);
        }

        protected virtual int GetNumFeaturesExtractedFor(Prediction prediction)
        {
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(FeatureBasedDCM));
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
            // this can be called concurrently (e.g., via the time slice model with one thread per slice), so lock on prediction to get the point objects and their vectors
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
                    Thread t = new Thread(new ParameterizedThreadStart(o =>
                        {
                            int core = (int)o;
                            NpgsqlConnection threadConnection = DB.Connection.OpenConnection;
                            string pointTableName = Point.GetTableName(prediction);
                            foreach (Feature spatialDistanceFeature in spatialDistanceFeatures)
                            {
                                Shapefile shapefile = new Shapefile(int.Parse(training ? spatialDistanceFeature.TrainingResourceId : spatialDistanceFeature.PredictionResourceId));

                                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT points." + Point.Columns.Id + " as points_" + Point.Columns.Id + "," +
                                                                             "CASE WHEN COUNT(" + shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ")=0 THEN " + distanceWhenBeyondThreshold + " " +
                                                                             "ELSE min(st_distance(st_closestpoint(" + shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ",points." + Point.Columns.Location + "),points." + Point.Columns.Location + ")) " +
                                                                             "END as feature_value " +

                                                                             "FROM (SELECT *,st_expand(" + pointTableName + "." + Point.Columns.Location + "," + FeatureDistanceThreshold + ") as bounding_box " +
                                                                                   "FROM " + pointTableName + " " +
                                                                                   "WHERE " + pointTableName + "." + Point.Columns.Id + " % " + Configuration.ProcessorCount + " = " + core + " AND " +
                                                                                              "(" +
                                                                                                  pointTableName + "." + Point.Columns.Time + "='-infinity'::timestamp OR " +
                                                                                                  "(" +
                                                                                                      pointTableName + "." + Point.Columns.Time + ">=@point_start AND " +
                                                                                                      pointTableName + "." + Point.Columns.Time + "<=@point_end" +
                                                                                                  ")" +
                                                                                              ")" +
                                                                                   ") points " +

                                                                             "LEFT JOIN " + shapefile.GeometryTable + " " +

                                                                             "ON points.bounding_box && " + shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + " AND " +
                                                                                 "(" +
                                                                                    shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp OR " +
                                                                                    "(" +
                                                                                        shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Time + ">=@geometry_start AND " +
                                                                                        shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Time + "<=@geometry_end" +
                                                                                    ")" +
                                                                                 ")" +

                                                                             "GROUP BY points." + Point.Columns.Id, null, threadConnection);

                                DateTime spatialDistanceFeatureStart = start - spatialDistanceFeature.Parameters.GetTimeSpanValue(SpatialDistanceParameter.LagOffset);
                                DateTime spatialDistanceFeatureEnd = spatialDistanceFeatureStart + spatialDistanceFeature.Parameters.GetTimeSpanValue(SpatialDistanceParameter.LagDuration);

                                if (spatialDistanceFeatureEnd >= start)
                                    Console.Out.WriteLine("WARNING:  Spatial distance sample overlaps extraction period.");

                                if (spatialDistanceFeatureEnd < spatialDistanceFeatureStart)
                                    Console.Out.WriteLine("WARNING:  Spatial distance sample end precedes sample start.");

                                ConnectionPool.AddParameters(cmd, new Parameter("point_start", NpgsqlDbType.Timestamp, start),
                                                                  new Parameter("point_end", NpgsqlDbType.Timestamp, end),
                                                                  new Parameter("geometry_start", NpgsqlDbType.Timestamp, spatialDistanceFeatureStart),
                                                                  new Parameter("geometry_end", NpgsqlDbType.Timestamp, spatialDistanceFeatureEnd));

                                NpgsqlDataReader reader = cmd.ExecuteReader();
                                NumericFeature distanceFeature = _idNumericFeature[spatialDistanceFeature.Id];
                                while (reader.Read())
                                {
                                    FeatureVector vector;
                                    if (!pointIdFeatureVector.TryGetValue(Convert.ToInt32(reader["points_" + Point.Columns.Id]), out vector))  // above, we select all points that fall between point_start and point_end. the latter can be one tick short of the next minute, and npgsql rounds up causing points to appear in the reader that we didn't add to the pointIdFeatureVector collection.
                                        continue;

                                    double value = Convert.ToDouble(reader["feature_value"]);

                                    // value > threshold shouldn't happen here, since we exluced such objects from consideration above; however, the calculations aren't perfect in postgis, so we check again and reset appropriately
                                    if (value > distanceWhenBeyondThreshold)
                                        value = distanceWhenBeyondThreshold;

                                    vector.Add(distanceFeature, value, false); // don't update range due to concurrent access to the feature
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
                    Thread t = new Thread(new ParameterizedThreadStart(core =>
                        {
                            NpgsqlCommand command = DB.Connection.NewCommand(null);
                            for (int j = (int)core; j < spatialDensityFeatures.Count; j += Configuration.ProcessorCount)
                            {
                                Feature spatialDensityFeature = spatialDensityFeatures[j];

                                DateTime spatialDensityFeatureStart = start - spatialDensityFeature.Parameters.GetTimeSpanValue(SpatialDensityParameter.LagOffset);
                                DateTime spatialDensityFeatureEnd = spatialDensityFeatureStart + spatialDensityFeature.Parameters.GetTimeSpanValue(SpatialDensityParameter.LagDuration);

                                if (spatialDensityFeatureEnd >= start)
                                    Console.Out.WriteLine("WARNING:  Spatial density sample overlaps extraction period.");

                                if (spatialDensityFeatureEnd < spatialDensityFeatureStart)
                                    Console.Out.WriteLine("WARNING:  Spatial density sample end precedes sample start.");

                                Shapefile shapefile = new Shapefile(int.Parse(training ? spatialDensityFeature.TrainingResourceId : spatialDensityFeature.PredictionResourceId));
                                string geometryRecordWhereClause = "WHERE " + ShapefileGeometry.Columns.Time + "='-infinity'::timestamp OR (" + ShapefileGeometry.Columns.Time + ">=@geometry_start AND " + ShapefileGeometry.Columns.Time + "<=@geometry_end)";
                                Parameter geometryStart = new Parameter("geometry_start", NpgsqlDbType.Timestamp, spatialDensityFeatureStart);
                                Parameter geometryEnd = new Parameter("geometry_end", NpgsqlDbType.Timestamp, spatialDensityFeatureEnd);
                                List<PostGIS.Point> kdeInputPoints = Geometry.GetPoints(command, shapefile.GeometryTable, ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, geometryRecordWhereClause, -1, geometryStart.NpgsqlParameter, geometryEnd.NpgsqlParameter).SelectMany(pointList => pointList).Select(p => new PostGIS.Point(p.X, p.Y, area.Shapefile.SRID)).ToList();

                                Console.Out.WriteLine("Computing spatial density of \"" + shapefile.Name + "\".");
                                int sampleSize = spatialDensityFeature.Parameters.GetIntegerValue(SpatialDensityParameter.SampleSize);
                                List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, sampleSize, false, -1, -1, densityEvalPoints, false);

                                // the density might not be computable if too few points are provided -- use default value for all evaluation points in such cases
                                if (densityEstimates.Count != densityEvalPoints.Count)
                                {
                                    float defaultValue = spatialDensityFeature.Parameters.GetFloatValue(SpatialDensityParameter.DefaultValue);
                                    Console.Out.WriteLine("WARNING:  Using default value \"" + defaultValue + "\" for feature " + spatialDensityFeature);
                                    densityEstimates = Enumerable.Repeat(defaultValue, densityEvalPoints.Count).ToList();
                                }

                                lock (featureIdDensityEstimates) { featureIdDensityEstimates.Add(spatialDensityFeature.Id, densityEstimates); }
                            }

                            DB.Connection.Return(command.Connection);
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
                        featureVectors[i].Add(densityFeature, densityEstimates[i], false);  // don't update range due to concurrent access to the feature
                }
            }
            #endregion

            #region geometry attribute features
            List<Feature> geometryAttributeFeatures = Features.Where(f => f.EnumValue.Equals(FeatureType.GeometryAttribute)).ToList();
            if (geometryAttributeFeatures.Count > 0)
            {
                Console.Out.WriteLine("Extracting geometry attribute features.");
                threads.Clear();
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(o =>
                        {
                            int core = (int)o;
                            NpgsqlConnection threadConnection = DB.Connection.OpenConnection;
                            string pointTableName = Point.GetTableName(prediction);
                            foreach (Feature geometryAttributeFeature in geometryAttributeFeatures)
                            {
                                Shapefile shapefile = new Shapefile(int.Parse(training ? geometryAttributeFeature.TrainingResourceId : geometryAttributeFeature.PredictionResourceId));
                                string attributeColumn = geometryAttributeFeature.Parameters.GetStringValue(GeometryAttributeParameter.AttributeColumn);
                                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + pointTableName + "." + Point.Columns.Id + " as point_id," + shapefile.GeometryTable + "." + attributeColumn + " as geometry_attribute " +
                                                                             "FROM " + pointTableName + " " +
                                                                             "LEFT JOIN " + shapefile.GeometryTable + " " + // the geometry might not overlap the point, in which case we'll use the default feature value below
                                                                             "ON st_intersects(" + pointTableName + "." + Point.Columns.Location + "," + shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ") " +
                                                                             "WHERE " + pointTableName + "." + Point.Columns.Id + " % " + Configuration.ProcessorCount + " = " + core + " AND " +
                                                                                        "(" +
                                                                                          pointTableName + "." + Point.Columns.Time + "='-infinity'::timestamp OR " +
                                                                                          "(" +
                                                                                            pointTableName + "." + Point.Columns.Time + ">=@point_start AND " +
                                                                                            pointTableName + "." + Point.Columns.Time + "<=@point_end" +
                                                                                          ")" +
                                                                                        ") " +
                                                                             "ORDER BY " + pointTableName + "." + Point.Columns.Id, null, threadConnection);

                                ConnectionPool.AddParameters(cmd, new Parameter("point_start", NpgsqlDbType.Timestamp, start),
                                                                  new Parameter("point_end", NpgsqlDbType.Timestamp, end));

                                LAIR.MachineLearning.Feature attributeFeature;
                                string attributeType = geometryAttributeFeature.Parameters.GetStringValue(GeometryAttributeParameter.AttributeType);
                                if (attributeType == "Numeric")
                                    attributeFeature = _idNumericFeature[geometryAttributeFeature.Id] as LAIR.MachineLearning.Feature;
                                else if (attributeType == "Nominal")
                                    attributeFeature = _idNominalFeature[geometryAttributeFeature.Id] as LAIR.MachineLearning.Feature;
                                else
                                    throw new NotImplementedException("Unrecognized geometry attribute feature type:  " + attributeType);

                                List<object> values = new List<object>();
                                int currPointId = -1;
                                int pointId = -1;

                                Action addFeatureToVector = new Action(() =>
                                    {
                                        if (values.Count > 0)
                                        {
                                            FeatureVector vector = pointIdFeatureVector[currPointId];
                                            if (attributeFeature is NumericFeature)
                                                vector.Add(attributeFeature, values.Select(v => Convert.ToSingle(v)).Average(), false);  // don't update range due to concurrent access to the feature
                                            else if (values.Count == 1)
                                                vector.Add(attributeFeature, Convert.ToString(values[0]), false);  // don't update range due to concurrent access to the feature
                                            else
                                                throw new Exception("Nominal geometry attribute \"" + attributeColumn + "\" of shapefile \"" + shapefile.GeometryTable + "\" has multiple non-numeric values at point \"" + (vector.DerivedFrom as Point).Location + "\".");
                                        }

                                        values.Clear();
                                        currPointId = pointId;
                                    });

                                NpgsqlDataReader reader = cmd.ExecuteReader();
                                string defaultValue = geometryAttributeFeature.Parameters.GetStringValue(GeometryAttributeParameter.DefaultValue);
                                while (reader.Read())
                                {
                                    pointId = Convert.ToInt32(reader["point_id"]);
                                    if (pointId != currPointId)
                                        addFeatureToVector();

                                    object value = reader["geometry_attribute"];
                                    if (value is DBNull)  // we did a left join above, so the value might be null meaning the geometry did not overlap the point
                                        value = defaultValue;

                                    values.Add(value);
                                }
                                reader.Close();

                                addFeatureToVector();
                            }

                            DB.Connection.Return(threadConnection);
                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();
            }
            #endregion

            #region incident density features
            List<Feature> kdeFeatures = Features.Where(f => f.EnumValue.Equals(FeatureType.IncidentDensity)).ToList();
            if (kdeFeatures.Count > 0)
            {
                List<PostGIS.Point> densityEvalPoints = featureVectors.Select(v => (v.DerivedFrom as Point).Location).ToList();
                Dictionary<string, List<float>> featureIdDensityEstimates = new Dictionary<string, List<float>>(kdeFeatures.Count);
                threads.Clear();
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(core =>
                        {
                            for (int j = (int)core; j < kdeFeatures.Count; j += Configuration.ProcessorCount)
                            {
                                Feature kdeFeature = kdeFeatures[j];

                                List<PostGIS.Point> kdeInputPoints = new List<PostGIS.Point>();
                                string incident = training ? kdeFeature.TrainingResourceId : kdeFeature.PredictionResourceId;
                                int lagCount = kdeFeature.Parameters.GetIntegerValue(IncidentDensityParameter.LagCount);
                                TimeSpan lagOffset = kdeFeature.Parameters.GetTimeSpanValue(IncidentDensityParameter.LagOffset);
                                TimeSpan lagDuration = kdeFeature.Parameters.GetTimeSpanValue(IncidentDensityParameter.LagDuration);
                                for (int k = 1; k <= lagCount; ++k)
                                {
                                    DateTime incidentSampleStart = start - new TimeSpan(k * lagOffset.Ticks);
                                    DateTime incidentSampleEnd = incidentSampleStart + lagDuration;

                                    if (incidentSampleEnd >= start)
                                        Console.Out.WriteLine("WARNING:  Incident density sample overlaps extraction period.");

                                    if (incidentSampleEnd < incidentSampleStart)
                                        Console.Out.WriteLine("WARNING:  Incident density sample end precedes sample start.");

                                    kdeInputPoints.AddRange(Incident.Get(incidentSampleStart, incidentSampleEnd, area, incident).Select(inc => inc.Location));
                                }

                                Console.Out.WriteLine("Computing spatial density of \"" + incident + "\" with " + lagCount + " lag(s) at offset " + lagOffset + ", each with duration " + lagDuration);
                                int sampleSize = kdeFeature.Parameters.GetIntegerValue(IncidentDensityParameter.SampleSize);
                                List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, sampleSize, false, 0, 0, densityEvalPoints, false);

                                // the density might not be computable if too few points are provided -- use default density for all evaluation points in such cases
                                if (densityEstimates.Count != densityEvalPoints.Count)
                                {
                                    float defaultValue = kdeFeature.Parameters.GetFloatValue(IncidentDensityParameter.DefaultValue);
                                    Console.Out.WriteLine("WARNING:  Using default value \"" + defaultValue + "\" for feature " + kdeFeature);
                                    densityEstimates = Enumerable.Repeat(defaultValue, densityEvalPoints.Count).ToList();
                                }

                                lock (featureIdDensityEstimates) { featureIdDensityEstimates.Add(kdeFeature.Id, densityEstimates); }
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
                        featureVectors[i].Add(densityFeature, densityEstimates[i], false);  // don't update range due to concurrent access to the feature (e.g., via time slice model calling into this method)
                }
            }
            #endregion

            // update all feature ranges. this wasn't done above due to potential concurrent access, either within this method or from calls into this method. each feature needs to be locked here due to potential concurrent calls into this method (e.g., time slice model)
            foreach (FeatureVector vector in featureVectors)
                foreach (LAIR.MachineLearning.Feature f in vector)
                    lock (f)
                        f.UpdateRange(vector[f]);

            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(FeatureBasedDCM));
            if (externalFeatureExtractor == null)
                yield return featureVectors;
            else
                foreach (FeatureVectorList externalFeatureVectors in externalFeatureExtractor.ExtractFeatures(prediction, featureVectors, training, start, end, true))
                    yield return externalFeatureVectors;
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
                PointPrediction.DeleteTable(prediction);
                Point.DeleteTable(prediction);
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
            Set<int> shapefilesInPredictionSRID = new Set<int>(Shapefile.GetForSRID(prediction.PredictionArea.Shapefile.SRID).Select(s => s.Id).ToArray());
            string badFeatures = Features.Where(f => (f.EnumValue.Equals(FeatureType.MinimumDistanceToGeometry) || f.EnumValue.Equals(FeatureType.GeometryDensity)) &&
                                                     !shapefilesInPredictionSRID.Contains(int.Parse(f.PredictionResourceId))).Select(f => f.ToString()).Concatenate(",");
            if (badFeatures.Length > 0)
                throw new Exception("Features \"" + badFeatures + "\" are not valid for the prediction area (" + prediction.PredictionArea.Name + "). These features must be remapped for prediction (or perhaps they were remapped incorrectly).");

            if (prediction.PredictionArea.Id != TrainingArea.Id && Features.Count(f => f.EnumValue.Equals(FeatureBasedDCM.FeatureType.GeometryAttribute)) > 0)
                throw new Exception("Cannot use geometry attributes in feature-remapped predictions.");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            try
            {
                _classifier.Initialize();

                #region training
                if (train)
                {
                    Console.Out.WriteLine("Creating training grid");

                    Point.CreateTable(prediction, prediction.Model.TrainingArea.Shapefile.SRID);
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

                    Point.DeleteTable(prediction);
                    prediction.ReleaseAllLazyLoadedData();
                }
                #endregion

                #region prediction
                if (predict)
                {
                    Console.Out.WriteLine("Creating prediction grid");

                    Point.CreateTable(prediction, prediction.PredictionArea.Shapefile.SRID);
                    InsertPointsIntoPrediction(cmd.Connection, prediction, false, true);

                    PointPrediction.CreateTable(prediction);
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
                                        pointPredictionLog.Write("<l c=\"" + vector.DerivedFrom.PredictionConfidenceScores[label] + "\"><![CDATA[" + label + "]]></l>");
                                    else
                                        throw new Exception("Invalid prediction label on point:  " + label);

                                pointPredictionLog.Write("</ls><fvs>");
                                foreach (LAIR.MachineLearning.Feature feature in vector)
                                {
                                    object value;
                                    if (feature is NumericFeature)
                                        value = Convert.ToSingle(vector[feature]);
                                    else
                                        value = vector[feature].ToString();

                                    pointPredictionLog.Write("<fv id=\"" + feature.Name + "\">" + value + "</fv>");
                                }

                                pointPredictionLog.WriteLine("</fvs></p>");
                            }
                            #endregion

                            PointPrediction.Insert(GetPointPredictionValues(featureVectors), prediction, false);
                        }

                        pointPredictionLog.Close();
                    }

                    PointPrediction.VacuumTable(prediction);
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
        /// list contains the labels and their confidence scores and the second list contains the feature IDs and their values.
        /// </summary>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        /// <param name="pointIds">Point IDs to read log for, or null for all points.</param>
        /// <returns></returns>
        public override Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, string>>>> ReadPointPredictionLog(string pointPredictionLogPath, Set<string> pointIds = null)
        {
            Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, string>>>> log = new Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, string>>>>();

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

                        List<Tuple<string, string>> featureValues = new List<Tuple<string, string>>();
                        XmlParser featureValuesP = new XmlParser(pointP.OuterXML("fvs"));
                        string featureValueXML;
                        while ((featureValueXML = featureValuesP.OuterXML("fv")) != null)
                        {
                            XmlParser featureValueP = new XmlParser(featureValueXML);
                            featureValues.Add(new Tuple<string, string>(featureValueP.AttributeValue("fv", "id"), featureValueP.ElementText("fv")));
                        }

                        log.Add(pointId, new Tuple<List<Tuple<string, double>>, List<Tuple<string, string>>>(labelConfidences, featureValues));

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
        /// list contains the labels and their confidence scores and the second list contains the feature IDs and their values.</param>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        public override void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, string>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath)
        {
            using (StreamWriter pointPredictionLog = new StreamWriter(new GZipStream(new FileStream(pointPredictionLogPath, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                foreach (string pointId in pointIdLabelsFeatureValues.Keys.OrderBy(k => k))
                {
                    pointPredictionLog.Write(pointId + " <p><ls>");
                    foreach (Tuple<string, double> labelConfidence in pointIdLabelsFeatureValues[pointId].Item1)
                        pointPredictionLog.Write("<l c=\"" + labelConfidence.Item2 + "\"><![CDATA[" + labelConfidence.Item1 + "]]></l>");

                    pointPredictionLog.Write("</ls><fvs>");
                    foreach (Tuple<string, string> featureIdValue in pointIdLabelsFeatureValues[pointId].Item2)
                        pointPredictionLog.Write("<fv id=\"" + featureIdValue.Item1 + "\">" + featureIdValue.Item2 + "</fv>");

                    pointPredictionLog.WriteLine("</fvs></p>");
                }

                pointPredictionLog.Close();
            }
        }

        public override string GetDetails(Prediction prediction)
        {
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(FeatureBasedDCM));
            string details = _classifier.GetDetails(prediction, externalFeatureExtractor == null ? null : externalFeatureExtractor.GetDetails(prediction));

            prediction.ModelDetails = details;

            return prediction.ModelDetails;
        }

        public override DiscreteChoiceModel Copy()
        {
            return new FeatureBasedDCM(Name, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, _trainingPointSpacing, _featureDistanceThreshold, _negativePointStandoff, _classifier.Copy(), _features);
        }

        public override string ToString()
        {
            return "Feature DCM:  " + Name;
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            int featuresToDisplay = 10; // can have hundreds of features, which makes the tooltip excrutiatingly slow
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(FeatureBasedDCM));
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Classifier:  " + _classifier.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Features:  " + Features.Where((f, i) => i < featuresToDisplay).Select(f => f.ToString()).Concatenate(", ") + (Features.Count > featuresToDisplay ? " ... (" + (Features.Count - featuresToDisplay) + " not shown)" : "") + Environment.NewLine +
                   indent + "External feature extractor (" + typeof(FeatureBasedDCM) + "):  " + (externalFeatureExtractor == null ? "None" : externalFeatureExtractor.GetDetails(indentLevel + 1)) + Environment.NewLine +
                   indent + "Training point spacing:  " + _trainingPointSpacing + Environment.NewLine +
                   indent + "Feature distance threshold:  " + _featureDistanceThreshold + Environment.NewLine +
                   indent + "Negative point standoff:  " + _negativePointStandoff;
        }
    }
}
