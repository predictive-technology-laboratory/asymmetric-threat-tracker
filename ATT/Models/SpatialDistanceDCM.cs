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
using PTL.ATT.Exceptions;
using LAIR.XML;
using LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.Models
{
    public class SpatialDistanceDCM : DiscreteChoiceModel, IFeatureBasedDCM
    {
        public enum SpatialDistanceFeature
        {
            /// <summary>
            /// Shortest distance to geometries in a shapefile
            /// </summary>
            MinimumDistanceToGeometry,

            /// <summary>
            /// Density of geometries in a shapefile
            /// </summary>
            GeometryDensity,

            /// <summary>
            /// Value of incident density derived from the training incidents
            /// </summary>
            IncidentKernelDensityEstimate
        }

        public new const string Table = "spatial_distance_dcm";

        public new class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Classifier = "classifier";
            [Reflector.Insert, Reflector.Select(true)]
            public const string FeatureDistanceThreshold = "feature_distance_threshold";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionSampleSize = "prediction_sample_size";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingSampleSize = "training_sample_size";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return DiscreteChoiceModel.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinDiscreteChoiceModel { get { return DiscreteChoiceModel.Table + " JOIN " + Table + " ON " + DiscreteChoiceModel.Table + "." + DiscreteChoiceModel.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Classifier + " BYTEA," +
                   Columns.FeatureDistanceThreshold + " INT," +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE CASCADE," +
                   Columns.PredictionSampleSize + " INT," +
                   Columns.TrainingSampleSize + " INT);";
        }

        public static int Create(NpgsqlConnection connection,
                                 string name,
                                 int pointSpacing,
                                 int featureDistanceThreshold,
                                 Type type,
                                 Area trainingArea,
                                 DateTime trainingStart,
                                 DateTime trainingEnd,
                                 int trainingSampleSize,
                                 int predictionSampleSize,
                                 IEnumerable<string> incidentTypes,
                                 PTL.ATT.Classifiers.Classifier classifier,
                                 IEnumerable<Smoother> smoothers,
                                 List<Feature> features)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(null, connection);

            bool returnConnection = false;
            if (cmd.Connection == null)
            {
                cmd.Connection = DB.Connection.OpenConnection;
                cmd.CommandText = "BEGIN";
                cmd.ExecuteNonQuery();
                returnConnection = true;
            }

            if (type == null)
                type = typeof(SpatialDistanceDCM);

            int dcmId = DiscreteChoiceModel.Create(cmd.Connection, name, pointSpacing, type, trainingArea, trainingStart, trainingEnd, incidentTypes, smoothers);

            MemoryStream ms = new MemoryStream();

            if (classifier != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                classifier.ModelId = dcmId;
                bf.Serialize(ms, classifier);
            }

            ConnectionPool.AddParameters(cmd, new Parameter("classifier", NpgsqlDbType.Bytea, ms.ToArray()));

            cmd.CommandText = "INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (@classifier," +
                                                                                          featureDistanceThreshold + "," +
                                                                                          dcmId + "," +
                                                                                          predictionSampleSize + "," +
                                                                                          trainingSampleSize + ")";
            cmd.ExecuteNonQuery();

            if (returnConnection)
            {
                cmd.CommandText = "COMMIT";
                cmd.ExecuteNonQuery();
                DB.Connection.Return(cmd.Connection);

                SpatialDistanceDCM spatialDistanceDCM = SpatialDistanceDCM.Instantiate(dcmId) as SpatialDistanceDCM;
                spatialDistanceDCM.Features = features;
            }

            return dcmId;
        }

        public static IEnumerable<Feature> GetAvailableFeatures(Area area)
        {
            foreach (SpatialDistanceFeature f in Enum.GetValues(typeof(SpatialDistanceFeature)))
                if (f == SpatialDistanceFeature.MinimumDistanceToGeometry)
                {
                    foreach (Shapefile shapefile in Shapefile.GetForSRID(area.SRID).OrderBy(s => s.Name))
                        if (shapefile.Type == Shapefile.ShapefileType.Feature)
                            yield return new Feature(typeof(SpatialDistanceFeature), SpatialDistanceFeature.MinimumDistanceToGeometry, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (DISTANCE)");
                }
                else if (f == SpatialDistanceFeature.GeometryDensity)
                {
                    foreach (Shapefile shapefile in Shapefile.GetForSRID(area.SRID).OrderBy(s => s.Name))
                        if (shapefile.Type == Shapefile.ShapefileType.Feature)
                            yield return new Feature(typeof(SpatialDistanceFeature), SpatialDistanceFeature.GeometryDensity, shapefile.Id.ToString(), shapefile.Id.ToString(), shapefile.Name + " (DENSITY)");
                }
                else if (f == SpatialDistanceFeature.IncidentKernelDensityEstimate)
                    foreach (string incidentType in Incident.GetUniqueTypes(DateTime.MinValue, DateTime.MaxValue, area).OrderBy(i => i))
                        yield return new Feature(typeof(SpatialDistanceFeature), f, incidentType, incidentType, "KDE \"" + incidentType + "\"");
                else
                    yield return new Feature(typeof(SpatialDistanceFeature), f, null, null, f.ToString());

            FeatureExtractor externalFeatureExtractor;
            if (Configuration.TryGetFeatureExtractor(typeof(SpatialDistanceDCM), out externalFeatureExtractor))
            {
                externalFeatureExtractor.Initialize(null, Configuration.GetFeatureExtractorConfigOptions(typeof(SpatialDistanceDCM)));
                foreach (Feature f in externalFeatureExtractor.GetAvailableFeatures(area))
                    yield return f;
            }
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

        private PTL.ATT.Classifiers.Classifier _classifier;
        private int _featureDistanceThreshold;
        private FeatureExtractor _externalFeatureExtractor;
        private List<Feature> _features;
        private int _trainingSampleSize;
        private int _predictionSampleSize;

        public PTL.ATT.Classifiers.Classifier Classifier
        {
            get { return _classifier; }
        }

        public FeatureExtractor ExternalFeatureExtractor
        {
            get { return _externalFeatureExtractor; }
        }

        public int FeatureDistanceThreshold
        {
            get { return _featureDistanceThreshold; }
        }

        public ICollection<Feature> Features
        {
            get
            {
                if (_features == null)
                {
                    _features = new List<Feature>();

                    NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                    try
                    {
                        cmd.CommandText = "SELECT " + Feature.Columns.Select + " FROM " + Feature.Table + " WHERE " + Feature.Columns.ModelId + "=" + Id + " ORDER BY " + Feature.Columns.Id;
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            _features.Add(new Feature(reader));

                        reader.Close();
                    }
                    finally { DB.Connection.Return(cmd.Connection); }
                }

                return _features;
            }
            set
            {
                _features = null;

                NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                try
                {
                    cmd.CommandText = "DELETE FROM " + Feature.Table + " WHERE " + Feature.Columns.ModelId + "=" + Id;
                    cmd.ExecuteNonQuery();

                    if (value != null)
                        foreach (Feature feature in value.OrderBy(f => f.Id))
                            Feature.Create(cmd.Connection, feature.Description, feature.EnumType, feature.EnumValue, this, feature.TrainingResourceId, feature.PredictionResourceId, false);

                    Feature.VacuumTable();
                }
                finally { DB.Connection.Return(cmd.Connection); }
            }
        }

        public int TrainingSampleSize
        {
            get { return _trainingSampleSize; }
        }

        public int PredictionSampleSize
        {
            get { return _predictionSampleSize; }
        }

        protected SpatialDistanceDCM() { }

        internal SpatialDistanceDCM(int id)
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

            _featureDistanceThreshold = Convert.ToInt32(reader[Table + "_" + Columns.FeatureDistanceThreshold]);
            _trainingSampleSize = Convert.ToInt32(reader[Table + "_" + Columns.TrainingSampleSize]);
            _predictionSampleSize = Convert.ToInt32(reader[Table + "_" + Columns.PredictionSampleSize]);

            if (Configuration.TryGetFeatureExtractor(GetType(), out _externalFeatureExtractor))
                _externalFeatureExtractor.Initialize(this, Configuration.GetFeatureExtractorConfigOptions(GetType()));

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(reader[Table + "_" + Columns.Classifier] as byte[]);
            ms.Position = 0;
            _classifier = bf.Deserialize(ms) as PTL.ATT.Classifiers.Classifier;
        }

        public void Update(string name, int pointSpacing, int featureDistanceThreshold, Area trainingArea, DateTime trainingStart, DateTime trainingEnd, int trainingSampleSize, int predictionSampleSize, IEnumerable<string> incidentTypes, PTL.ATT.Classifiers.Classifier classifier, IEnumerable<Smoother> smoothers, List<Feature> features)
        {
            base.Update(name, pointSpacing, trainingArea, trainingStart, trainingEnd, incidentTypes, smoothers);

            _featureDistanceThreshold = featureDistanceThreshold;
            _classifier = classifier;
            _trainingSampleSize = trainingSampleSize;
            _predictionSampleSize = predictionSampleSize;

            Features = features;

            MemoryStream ms = new MemoryStream();

            if (_classifier != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                classifier.ModelId = Id;
                bf.Serialize(ms, _classifier);
            }

            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " +
                                          Columns.Classifier + "=@classifier," +
                                          Columns.FeatureDistanceThreshold + "=" + _featureDistanceThreshold + "," +
                                          Columns.TrainingSampleSize + "=" + _trainingSampleSize + "," +
                                          Columns.PredictionSampleSize + "=" + _predictionSampleSize + " " +
                                          "WHERE " + Columns.Id + "=" + Id, new Parameter("classifier", NpgsqlDbType.Bytea, ms.ToArray()));
        }

        protected virtual void InsertPointsIntoPrediction(NpgsqlConnection connection, Prediction prediction, bool training, bool vacuum)
        {
            Area area = training ? prediction.Model.TrainingArea : prediction.PredictionArea;

            List<Tuple<PostGIS.Point, string, DateTime>> incidentPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            Set<Tuple<double, double>> incidentLocations = new Set<Tuple<double, double>>(false);
            foreach (Incident i in Incident.Get(prediction.Model.TrainingStart, prediction.Model.TrainingEnd, area, prediction.Model.IncidentTypes.ToArray()))
            {
                incidentPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(new PostGIS.Point(i.Location.X, i.Location.Y, area.SRID), training ? i.Type : PointPrediction.NullLabel, training ? i.Time : DateTime.MinValue));
                incidentLocations.Add(new Tuple<double, double>(i.Location.X, i.Location.Y));
            }

            if (training && incidentPoints.Count == 0)
                throw new ZeroPositivePointsException("Zero positive incident points retrieved for \"" + prediction.Model.IncidentTypes.Concatenate(", ") + "\" during the training period \"" + prediction.Model.TrainingStart.ToShortDateString() + " " + prediction.Model.TrainingStart.ToShortTimeString() + " -- " + prediction.Model.TrainingEnd.ToShortDateString() + " " + prediction.Model.TrainingEnd.ToShortTimeString() + "\"");

            List<Tuple<PostGIS.Point, string, DateTime>> nullPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            double areaMinX = area.BoundingBox.MinX;
            double areaMaxX = area.BoundingBox.MaxX;
            double areaMinY = area.BoundingBox.MinY;
            double areaMaxY = area.BoundingBox.MaxY;
            for (double x = areaMinX + prediction.Model.PointSpacing / 2d; x <= areaMaxX; x += prediction.Model.PointSpacing)  // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + prediction.Model.PointSpacing / 2d; y <= areaMaxY; y += prediction.Model.PointSpacing)
                {
                    Tuple<double, double> location = new Tuple<double, double>(x, y);
                    PostGIS.Point point = new PostGIS.Point(x, y, area.SRID);
                    if (!incidentLocations.Contains(location))
                        nullPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(point, PointPrediction.NullLabel, DateTime.MinValue));
                }

            List<int> nullPointIds = Point.Insert(connection, nullPoints, prediction.Id, area, true, vacuum);

            Set<int> incidentPointIndexesInArea = new Set<int>(area.Contains(incidentPoints.Select(p => p.Item1).ToList()).ToArray());
            incidentPoints = incidentPoints.Where((p, i) => incidentPointIndexesInArea.Contains(i)).ToList();
            int maxSampleSize = training ? _trainingSampleSize : _predictionSampleSize;
            int numIncidentsToRemove = (nullPointIds.Count + incidentPoints.Count) - maxSampleSize;
            string sample = training ? "training" : "prediction";
            if (numIncidentsToRemove > 0)
                if (incidentPoints.Count > 0)
                {
                    numIncidentsToRemove = Math.Min(incidentPoints.Count, numIncidentsToRemove);

                    Console.Out.WriteLine("WARNING:  the " + sample + " sample size is too small. We are forced to remove " + numIncidentsToRemove + " random incidents in order to meet the required sample size. In order to use all incidents, increase the sample size to " + (nullPointIds.Count + incidentPoints.Count));

                    incidentPoints.Randomize(new Random(1240894));
                    incidentPoints.RemoveRange(0, numIncidentsToRemove);
                }
                else
                    Console.Out.WriteLine("WARNING:  we are using " + nullPointIds.Count + " points, but the maximum " + sample + " sample size is " + maxSampleSize + ". Be aware that this could be going beyond the memory limits of your machine.");

            if (training && incidentPoints.Count == 0)
                throw new ZeroPositivePointsException("Zero positive incident points inserted into point sample for \"" + prediction.Model.IncidentTypes.Concatenate(", ") + "\"." + (numIncidentsToRemove > 0 ? " This is due to sample size restrictions. Either increase the training sample size to " + (maxSampleSize + numIncidentsToRemove) + " to include all incidents or increase the point spacing to reduce the number of null points." : ""));

            Point.Insert(connection, incidentPoints, prediction.Id, area, false, vacuum);
        }

        protected virtual IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training)
        {
            int numFeatures = Features.Count(f => f.EnumType == typeof(SpatialDistanceFeature)) + (_externalFeatureExtractor == null ? 0 : _externalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(SpatialDistanceDCM)));

            Dictionary<int, NumericFeature> idFeature = new Dictionary<int, NumericFeature>();
            foreach (Feature f in Features)
                idFeature.Add(f.Id, new NumericFeature(f.Id.ToString()));

            Area area = training ? prediction.Model.TrainingArea : prediction.PredictionArea;
            Set<Thread> threads = new Set<Thread>();

            #region spatial distance features
            Console.Out.WriteLine("Extracting spatial distance feature values");

            List<Dictionary<int, FeatureVector>> corePointIdFeatureVector = new List<Dictionary<int, FeatureVector>>(Configuration.ProcessorCount);
            float featureValueWhenBeyondThreshold = (float)Math.Sqrt(2.0 * Math.Pow(FeatureDistanceThreshold, 2));
            for (int i = 0; i < Configuration.ProcessorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        int core = (int)o;
                        string pointFeatureTable = "temp_" + core;
                        NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT p." + Point.Columns.Id + " as point_id," +
                                                                            "p." + Point.Columns.IncidentType + " as point_incident_type," +
                                                                            "p." + Point.Columns.Location + " as point_location," +
                                                                            "p." + Point.Columns.Time + " as point_time," +
                                                                            "st_expand(p." + Point.Columns.Location + "," + FeatureDistanceThreshold + ") as point_bounding_box," +  // get a bounding box around the point to limit the minimum distance calculation time. any distance beyond the threshold will receive a fixed distance value.
                                                                            "CASE WHEN f." + Feature.Columns.Id + " IS NULL THEN -1 ELSE f." + Feature.Columns.Id + " END as feature_id," +  // can't have a null feature ID, since we're using this column as our primary key. the null can happen since we're doing a left-join on the features, and there's no requirement to define features for a prediction. in such cases, -1 for feature ID will be fine since shapefile ID will be null, thus not matching anything in the left-join below.
                                                                            "f." + (training ? Feature.Columns.TrainingResourceId : Feature.Columns.PredictionResourceId) + "::integer as shapefile_id " +

                                                                     "INTO " + pointFeatureTable + " " +

                                                                     // cross points with features
                                                                     "FROM " + Point.GetTableName(prediction.Id) + " p LEFT JOIN " + Feature.Table + " f ON f." + Feature.Columns.ModelId + "=" + Id + " AND " +                                                // cross all points with all features for the current prediction (left-join in case there are no features to extract here and we just want the points for further feature extraction)
                                                                                                                                                           "f." + Feature.Columns.EnumType + "='" + typeof(SpatialDistanceFeature) + "' AND " +                 // distance features
                                                                                                                                                           "f." + Feature.Columns.EnumValue + "='" + SpatialDistanceFeature.MinimumDistanceToGeometry + "' " +  // as opposed to raster shapefiles
                                                                     // only process points associated with the current core                                                                                         
                                                                     "WHERE p." + Point.Columns.Core + "=" + core + ";" +

                                                                     "ALTER TABLE " + pointFeatureTable + " ADD PRIMARY KEY (point_id,feature_id);" + // this is required in order to write a clean grouping statement below. if we don't have this, then we need to also group by the selected columns below.

                                                                     // create some indexes
                                                                     "CREATE INDEX ON " + pointFeatureTable + " (point_id);" +
                                                                     "CREATE INDEX ON " + pointFeatureTable + " USING GIST (point_bounding_box);" +
                                                                     "CREATE INDEX ON " + pointFeatureTable + " (feature_id);" +
                                                                     "CREATE INDEX ON " + pointFeatureTable + " (shapefile_id);");

                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "VACUUM ANALYZE " + pointFeatureTable;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "SELECT " + pointFeatureTable + ".point_id as " + pointFeatureTable + "_" + Point.Columns.Id + "," +
                                                      pointFeatureTable + ".point_incident_type as " + pointFeatureTable + "_" + Point.Columns.IncidentType + "," +
                                                      "st_x(" + pointFeatureTable + ".point_location) as " + Point.Columns.X(pointFeatureTable) + "," +
                                                      "st_y(" + pointFeatureTable + ".point_location) as " + Point.Columns.Y(pointFeatureTable) + "," +
                                                      "st_srid(" + pointFeatureTable + ".point_location) as " + Point.Columns.SRID(pointFeatureTable) + "," +
                                                      pointFeatureTable + ".point_time as " + pointFeatureTable + "_" + Point.Columns.Time + "," +
                                                      pointFeatureTable + ".feature_id," +

                                                      // the feature value for a point is the minimum distance from the point to an object associated with the feature
                                                     "CASE WHEN COUNT(sfg." + ShapefileGeometry.Columns.Geometry + ")=0 THEN " + featureValueWhenBeyondThreshold + " " + // with a bounding box of FeatureDistanceThreshold around each point, the maximum distance between a point and some feature shapefile geometry would be sqrt(2*FeatureDistanceThreshold^2). That is, the feature shapefile geometry would be positioned in one of the corners of the bounding box. 
                                                     "ELSE min(st_distance(st_closestpoint(sfg." + ShapefileGeometry.Columns.Geometry + "," + pointFeatureTable + ".point_location)," + pointFeatureTable + ".point_location)) " +
                                                     "END as feature_value " +

                                          "FROM " + pointFeatureTable + " LEFT JOIN " + ShapefileGeometry.GetTableName(area.SRID) + " sfg ON " + pointFeatureTable + ".shapefile_id=sfg." + ShapefileGeometry.Columns.ShapefileId + " AND sfg." + ShapefileGeometry.Columns.Geometry + " && " + pointFeatureTable + ".point_bounding_box " + // only calculate distance for spatial objects that are within the point's bounding box

                                          // group all distance calculations by point and feature -- we're going to then find the minimum value
                                          "GROUP BY " + pointFeatureTable + ".point_id," +
                                                        pointFeatureTable + ".feature_id;";

                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        Dictionary<int, FeatureVector> pointIdFeatureVector = new Dictionary<int, FeatureVector>();
                        while (reader.Read())
                        {
                            int pointId = Convert.ToInt32(reader[pointFeatureTable + "_" + Point.Columns.Id]);

                            FeatureVector vector;
                            if (!pointIdFeatureVector.TryGetValue(pointId, out vector))
                            {
                                Point p = new Point(reader, pointFeatureTable);
                                p.TrueClass = p.IncidentType;
                                vector = new FeatureVector(p, numFeatures);
                                pointIdFeatureVector.Add(pointId, vector);
                            }

                            int featureId = Convert.ToInt32(reader["feature_id"]);
                            if (featureId != -1) // we can get -1 back if no objects for a feature were within the point's bounding box or if no features were defined (we did a left-join with the feature)
                            {
                                double value = Convert.ToDouble(reader["feature_value"]);

                                // value > threshold shouldn't happen here, since we exluced such objects from consideration above; however, the calculations aren't perfect in postgis, so we check again and reset appropriately
                                if (value > featureValueWhenBeyondThreshold)
                                    value = featureValueWhenBeyondThreshold;

                                vector.Add(idFeature[featureId], value, false);
                            }
                        }
                        reader.Close();

                        lock (corePointIdFeatureVector) { corePointIdFeatureVector.Add(pointIdFeatureVector); }

                        cmd.CommandText = "DROP TABLE " + pointFeatureTable;
                        cmd.ExecuteNonQuery();

                        DB.Connection.Return(cmd.Connection);
                    }));

                t.Start(i);
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();

            FeatureVectorList mergedVectors = new FeatureVectorList(corePointIdFeatureVector.SelectMany(l => l.Values), corePointIdFeatureVector.Sum(l => l.Count));
            foreach (FeatureVector vector in mergedVectors)
                foreach (LAIR.MachineLearning.Feature f in vector)
                    f.UpdateRange(vector[f]);
            #endregion

            #region spatial density features
            List<Feature> spatialDensityFeatures = Features.Where(f => f.EnumValue.Equals(SpatialDistanceFeature.GeometryDensity)).ToList();
            if (spatialDensityFeatures.Count > 0)
            {
                threads.Clear();
                List<PostGIS.Point> densityEvalPoints = mergedVectors.Select(v => (v.DerivedFrom as Point).Location).ToList();
                Dictionary<int, List<float>> featureIdDensityEstimates = new Dictionary<int, List<float>>(spatialDensityFeatures.Count);
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int skip = (int)o;
                            NpgsqlConnection connection = DB.Connection.OpenConnection;
                            foreach (Feature spatialDensityFeature in spatialDensityFeatures)
                                if (skip-- == 0)
                                {
                                    Shapefile shapefile = new Shapefile(int.Parse(training ? spatialDensityFeature.TrainingResourceId : spatialDensityFeature.PredictionResourceId));
                                    Console.Out.WriteLine("Computing spatial density of \"" + shapefile.Name + "\".");

                                    Dictionary<string, string> constraints = new Dictionary<string, string>();
                                    constraints.Add(ShapefileGeometry.Columns.ShapefileId, shapefile.Id.ToString());
                                    List<PostGIS.Point> kdeInputPoints = Geometry.GetPoints(connection, ShapefileGeometry.GetTableName(area.SRID), ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, constraints, -1).SelectMany(pointList => pointList).Select(p => new PostGIS.Point(p.X, p.Y, area.SRID)).ToList();
                                    List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, 1000, false, -1, -1, densityEvalPoints, true);
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

                foreach (int featureId in featureIdDensityEstimates.Keys)
                {
                    List<float> densityEstimates = featureIdDensityEstimates[featureId];
                    for (int i = 0; i < densityEstimates.Count; ++i)
                        mergedVectors[i].Add(idFeature[featureId], densityEstimates[i]);
                }
            }
            #endregion

            #region kde
            List<Feature> kdeFeatures = Features.Where(f => f.EnumValue.Equals(SpatialDistanceFeature.IncidentKernelDensityEstimate)).ToList();
            if (kdeFeatures.Count > 0)
            {
                threads.Clear();
                List<PostGIS.Point> densityEvalPoints = mergedVectors.Select(v => (v.DerivedFrom as Point).Location).ToList();
                Dictionary<int, List<float>> featureIdDensityEstimates = new Dictionary<int, List<float>>(kdeFeatures.Count);
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

                                    IEnumerable<PostGIS.Point> kdeInputPoints = Incident.Get(TrainingStart, TrainingEnd, area, incident).Select(inc => inc.Location);
                                    List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, 500, false, 0, 0, densityEvalPoints, true);
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

                foreach (int featureId in featureIdDensityEstimates.Keys)
                {
                    List<float> densityEstimates = featureIdDensityEstimates[featureId];
                    for (int i = 0; i < densityEstimates.Count; ++i)
                        mergedVectors[i].Add(idFeature[featureId], densityEstimates[i]);
                }
            }
            #endregion

            if (_externalFeatureExtractor != null)
            {
                Console.Out.WriteLine("Running external feature extractor for " + typeof(SpatialDistanceDCM));

                foreach (FeatureVectorList featureVectors in _externalFeatureExtractor.ExtractFeatures(typeof(SpatialDistanceDCM), prediction, mergedVectors, training))
                    yield return featureVectors;
            }
            else
                yield return mergedVectors;
        }

        public void SelectFeatures(Prediction prediction, bool runPredictionAfterSelect)
        {
            _classifier.Initialize();

            Console.Out.WriteLine("Selecting features");
            Set<int> selectedFeatureIds = new Set<int>(_classifier.SelectFeatures(prediction).ToArray());
            Features = Features.Where(f => selectedFeatureIds.Contains(f.Id)).ToList();

            if (runPredictionAfterSelect)
            {
                Console.Out.WriteLine("Re-running prediction");
                PointPrediction.DeleteTable(prediction.Id);
                Point.DeleteTable(prediction.Id);
                prediction.ReleaseLazyLoadedData();
                Run(prediction, true, false, true);
                prediction.MostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
                prediction.UpdateEvaluation();
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
            string badFeatures = Features.Where(f => (f.EnumValue.Equals(SpatialDistanceFeature.MinimumDistanceToGeometry) || f.EnumValue.Equals(SpatialDistanceFeature.GeometryDensity)) && 
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

                        foreach (FeatureVectorList vectors in ExtractFeatureVectors(prediction, true))
                            _classifier.Consume(vectors);

                        _classifier.Train();

                        SelectFeatures(prediction, false);
                    }
                    #endregion

                    foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, true))
                        _classifier.Consume(featureVectors);

                    Console.Out.WriteLine("Training model");

                    _classifier.Train();

                    Point.DeleteTable(prediction.Id);
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
                        foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, false))
                        {
                            Console.Out.WriteLine("Making predictions");

                            _classifier.Classify(featureVectors);

                            foreach (FeatureVector vector in featureVectors.OrderBy(v => (v.DerivedFrom as Point).Id))  // sort by point ID so prediction log is sorted
                            {
                                #region log feature values
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
                                #endregion
                            }

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
        public override Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, Set<string> pointIds = null)
        {
            Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> log = new Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>();

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

                        List<Tuple<int, double>> featureValues = new List<Tuple<int, double>>();
                        XmlParser featureValuesP = new XmlParser(pointP.OuterXML("fvs"));
                        string featureValueXML;
                        while ((featureValueXML = featureValuesP.OuterXML("fv")) != null)
                        {
                            XmlParser featureValueP = new XmlParser(featureValueXML);
                            featureValues.Add(new Tuple<int, double>(int.Parse(featureValueP.AttributeValue("fv", "id")), double.Parse(featureValueP.ElementText("fv"))));
                        }

                        log.Add(pointId, new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(labelConfidences, featureValues));

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
        public override void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath)
        {
            using (StreamWriter pointPredictionLog = new StreamWriter(new GZipStream(new FileStream(pointPredictionLogPath, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                foreach (string pointId in pointIdLabelsFeatureValues.Keys.OrderBy(k => k))
                {
                    pointPredictionLog.Write(pointId + " <p><ls>");
                    foreach (Tuple<string, double> labelConfidence in pointIdLabelsFeatureValues[pointId].Item1)
                        pointPredictionLog.Write("<l c=\"" + labelConfidence.Item2 + "\"><![CDATA[" + labelConfidence.Item1 + "]]></l>");

                    pointPredictionLog.Write("</ls><fvs>");
                    foreach (Tuple<int, double> featureIdValue in pointIdLabelsFeatureValues[pointId].Item2)
                        pointPredictionLog.Write("<fv id=\"" + featureIdValue.Item1 + "\">" + featureIdValue.Item2 + "</fv>");

                    pointPredictionLog.WriteLine("</fvs></p>");
                }

                pointPredictionLog.Close();
            }
        }

        public override string GetDetails(Prediction prediction)
        {
            string details = _classifier.GetDetails(prediction, _externalFeatureExtractor == null ? null : _externalFeatureExtractor.GetDetails(prediction));

            prediction.ModelDetails = details;

            return prediction.ModelDetails;
        }

        public override int Copy()
        {
            return Create(null, Name, PointSpacing, _featureDistanceThreshold, GetType(), TrainingArea, TrainingStart, TrainingEnd, _trainingSampleSize, _predictionSampleSize, IncidentTypes, Classifier.Copy(), Smoothers, Features.ToList());
        }

        public override void UpdateFeatureIdsFrom(DiscreteChoiceModel original)
        {
            SpatialDistanceDCM originalSpatialDistanceDCM = original as SpatialDistanceDCM;
            Dictionary<int, int> oldNewFeatureId = new Dictionary<int, int>();
            foreach (Tuple<int, int> oldNew in originalSpatialDistanceDCM.Features.Zip(Features, new Func<Feature, Feature, Tuple<int, int>>((f1, f2) => new Tuple<int, int>(f1.Id, f2.Id))))
                oldNewFeatureId.Add(oldNew.Item1, oldNew.Item2);

            foreach (Prediction prediction in Prediction.GetForModel(this))
            {
                Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> originalLog = ReadPointPredictionLog(prediction.PointPredictionLogPath);
                Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> updatedLog = new Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>(originalLog.Count);
                foreach (string originalPointId in originalLog.Keys)
                {
                    List<Tuple<int, double>> updatedFeatureValues = new List<Tuple<int, double>>(originalLog[originalPointId].Item2.Count);
                    foreach (Tuple<int, double> originalFeatureValue in originalLog[originalPointId].Item2)
                        updatedFeatureValues.Add(new Tuple<int, double>(oldNewFeatureId[originalFeatureValue.Item1], originalFeatureValue.Item2));

                    updatedLog.Add(originalPointId, new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(originalLog[originalPointId].Item1, updatedFeatureValues));
                }

                WritePointPredictionLog(updatedLog, prediction.PointPredictionLogPath);
            }

            _classifier.ChangeFeatureIds(oldNewFeatureId);
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

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Classifier:  " + _classifier.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Features:  " + Features.Where((f, i) => i < featuresToDisplay).Select(f => f.ToString()).Concatenate(",") + (Features.Count > featuresToDisplay ? " ... (" + (Features.Count - featuresToDisplay) + " not shown)" : "") + Environment.NewLine +
                   indent + "External feature extractor:  " + (_externalFeatureExtractor == null ? "None" : _externalFeatureExtractor.GetDetails(indentLevel + 1)) + Environment.NewLine +
                   indent + "Feature distance threshold:  " + _featureDistanceThreshold + Environment.NewLine +
                   indent + "Training sample size:  " + _trainingSampleSize + Environment.NewLine +
                   indent + "Prediction sample size:  " + _predictionSampleSize;
        }
    }
}
