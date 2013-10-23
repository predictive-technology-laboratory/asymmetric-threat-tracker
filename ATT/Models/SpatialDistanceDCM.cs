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
using PTL.ATT.Incidents;
using LAIR.Collections.Generic;
using System.Drawing;
using PTL.ATT.Evaluation;
using LAIR.MachineLearning;
using System.Threading;
using PTL.ATT.ShapeFiles;
using System.IO;
using PTL.ATT.Smoothers;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using PTL.ATT.Exceptions;

namespace PTL.ATT.Models
{
    public class SpatialDistanceDCM : DiscreteChoiceModel, IFeatureBasedDCM
    {
        public enum SpatialDistanceFeature
        {
            /// <summary>
            /// Shortest distance to entities in a shapefile
            /// </summary>
            DistanceShapeFile,

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
            public const string ClassifyNonZeroVectorsUniformly = "classify_nonzero_uniformly";
            [Reflector.Insert, Reflector.Select(true)]
            public const string FeatureDistanceThreshold = "feature_distance_threshold";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return DiscreteChoiceModel.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinDiscreteChoiceModel { get { return DiscreteChoiceModel.Table + " JOIN " + Table + " ON " + DiscreteChoiceModel.Table + "." + DiscreteChoiceModel.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Classifier + " BYTEA," +
                   Columns.ClassifyNonZeroVectorsUniformly + " BOOLEAN," +
                   Columns.FeatureDistanceThreshold + " INT," +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE CASCADE);";
        }

        public override IEnumerable<Feature> AvailableFeatures
        {
            get
            {
                foreach (FeatureShapeFile featureShapefile in FeatureShapeFile.GetAvailable())
                {
                    if (featureShapefile.Type == FeatureShapeFile.ShapefileType.Distance)
                        yield return new Feature(typeof(SpatialDistanceFeature), SpatialDistanceFeature.DistanceShapeFile, featureShapefile.Id.ToString(), featureShapefile.Name);
                    else
                        throw new Exception("Unimplemented shapefile type:  " + featureShapefile.Type);
                }

                foreach (SpatialDistanceFeature f in Enum.GetValues(typeof(SpatialDistanceFeature)))
                    if (f == SpatialDistanceFeature.IncidentKernelDensityEstimate)
                        foreach (string incidentType in IncidentTypes)
                            yield return new Feature(typeof(SpatialDistanceFeature), f, incidentType, "KDE \"" + incidentType + "\"");
                    else if (f != SpatialDistanceFeature.DistanceShapeFile)
                        yield return new Feature(typeof(SpatialDistanceFeature), f, null, f.ToString());

                if (_externalFeatureExtractor != null)
                    foreach (Feature f in _externalFeatureExtractor.AvailableFeatures)
                        yield return f;
            }
        }

        public static int Create(NpgsqlConnection connection,
                                 string name,
                                 int pointSpacing,
                                 int featureDistanceThreshold,
                                 bool classifyNonZeroVectorsUniformly,
                                 Type type,
                                 Area trainingArea,
                                 DateTime trainingStart,
                                 DateTime trainingEnd,
                                 int trainingSampleSize,
                                 int predictionSampleSize,
                                 IEnumerable<string> incidentTypes,
                                 PTL.ATT.Classifiers.Classifier classifier,
                                 IEnumerable<Smoother> smoothers)
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

            int dcmId = DiscreteChoiceModel.Create(cmd.Connection, name, pointSpacing, type, trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, smoothers);

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            classifier.ModelId = dcmId;
            bf.Serialize(ms, classifier);

            ConnectionPool.AddParameters(cmd, new Parameter("classifier", NpgsqlDbType.Bytea, ms.ToArray()));

            cmd.CommandText = "INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (@classifier," +
                                                                                          classifyNonZeroVectorsUniformly + "," +
                                                                                          featureDistanceThreshold + "," +
                                                                                          dcmId + ")";
            cmd.ExecuteNonQuery();

            if (returnConnection)
            {
                cmd.CommandText = "COMMIT";
                cmd.ExecuteNonQuery();
                DB.Connection.Return(cmd.Connection);
            }

            return dcmId;
        }

        internal static IEnumerable<Tuple<string, Parameter>> GetPointPredictionValues(FeatureVectorList featureVectors)
        {
            int pointNum = 0;
            foreach (FeatureVector featureVector in featureVectors)
            {
                Point point = featureVector.DerivedFrom as Point;

                string labels = "'{" + point.PredictionConfidenceScores.Keys.Where(l => l != PointPrediction.NullLabel).Select(l => "\"" + l + "\"").Concatenate(",") + "}'";
                string threats = "'{" + point.PredictionConfidenceScores.Keys.Where(l => l != PointPrediction.NullLabel).Select(l => point.PredictionConfidenceScores[l].ToString()).Concatenate(",") + "}'";
                double totalThreat = point.PredictionConfidenceScores.Keys.Where(l => l != PointPrediction.NullLabel).Sum(l => point.PredictionConfidenceScores[l]);
                string timeParameterName = "@time_" + pointNum++;
                Parameter timeParameter = new Parameter(timeParameterName, NpgsqlDbType.Timestamp, point.Time);

                yield return new Tuple<string, Parameter>("(" + labels + "," + point.Id + "," + threats + "," + timeParameterName + "," + totalThreat + ")", timeParameter);
            }
        }

        private PTL.ATT.Classifiers.Classifier _classifier;
        private int _featureDistanceThreshold;
        private bool _classifyNonZeroVectorsUniformly;
        private FeatureExtractor _externalFeatureExtractor;

        public PTL.ATT.Classifiers.Classifier Classifier
        {
            get { return _classifier; }
        }

        protected FeatureExtractor ExternalFeatureExtractor
        {
            get { return _externalFeatureExtractor; }
        }

        public int FeatureDistanceThreshold
        {
            get { return _featureDistanceThreshold; }
        }

        public bool ClassifyNonZeroVectorsUniformly
        {
            get { return _classifyNonZeroVectorsUniformly; }
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
            _classifyNonZeroVectorsUniformly = Convert.ToBoolean(reader[Table + "_" + Columns.ClassifyNonZeroVectorsUniformly]);

            Type featureExtractorType;
            if (Configuration.TryGetFeatureExtractorType(GetType(), out featureExtractorType))
            {
                _externalFeatureExtractor = Activator.CreateInstance(featureExtractorType) as FeatureExtractor;
                _externalFeatureExtractor.Initialize(this, Configuration.GetFeatureExtractorConfigOptions(GetType()));
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(reader[Table + "_" + Columns.Classifier] as byte[]);
            ms.Position = 0;
            _classifier = bf.Deserialize(ms) as PTL.ATT.Classifiers.Classifier;
        }

        public void Update(string name, int pointSpacing, int featureDistanceThreshold, bool classifyNonZeroVectorsUniformly, Area trainingArea, DateTime trainingStart, DateTime trainingEnd, int trainingSampleSize, int predictionSampleSize, IEnumerable<string> incidentTypes, PTL.ATT.Classifiers.Classifier classifier, IEnumerable<Smoother> smoothers)
        {
            base.Update(name, pointSpacing, trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, smoothers);

            _classifyNonZeroVectorsUniformly = classifyNonZeroVectorsUniformly;
            _featureDistanceThreshold = featureDistanceThreshold;
            _classifier = classifier;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            classifier.ModelId = Id;
            bf.Serialize(ms, classifier);

            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " +
                                          Columns.Classifier + "=@classifier," +
                                          Columns.ClassifyNonZeroVectorsUniformly + "=" + classifyNonZeroVectorsUniformly + "," +
                                          Columns.FeatureDistanceThreshold + "=" + featureDistanceThreshold + " " +
                                          "WHERE " + Columns.Id + "=" + Id, new Parameter("classifier", NpgsqlDbType.Bytea, ms.ToArray()));
        }

        protected virtual void InsertPointsIntoPrediction(NpgsqlConnection connection, Prediction prediction, bool training, bool vacuum)
        {
            Area area = training ? prediction.TrainingArea : prediction.PredictionArea;

            List<Tuple<PostGIS.Point, string, DateTime>> incidentPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            Set<Tuple<double, double>> incidentLocations = new Set<Tuple<double, double>>(false);
            foreach (Incident i in Incident.Get(prediction.TrainingStartTime, prediction.TrainingEndTime, prediction.IncidentTypes.ToArray()))
            {
                incidentPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(new PostGIS.Point(i.Location.X, i.Location.Y, Configuration.PostgisSRID), training ? i.Type : PointPrediction.NullLabel, training ? i.Time : DateTime.MinValue));
                incidentLocations.Add(new Tuple<double, double>(i.Location.X, i.Location.Y));
            }

            if (training && incidentPoints.Count == 0)
                throw new ZeroPositivePointsException("Zero positive incident points retrieved for \"" + prediction.IncidentTypes.Concatenate(", ") + "\" during the training period \"" + prediction.TrainingStartTime.ToShortDateString() + " " + prediction.TrainingStartTime.ToShortTimeString() + " -- " + prediction.TrainingEndTime.ToShortDateString() + " " + prediction.TrainingEndTime.ToShortTimeString() + "\"");

            List<Tuple<PostGIS.Point, string, DateTime>> nullPoints = new List<Tuple<PostGIS.Point, string, DateTime>>();
            double areaMinX = area.BoundingBox.MinX;
            double areaMaxX = area.BoundingBox.MaxX;
            double areaMinY = area.BoundingBox.MinY;
            double areaMaxY = area.BoundingBox.MaxY;
            for (double x = areaMinX + prediction.PointSpacing / 2d; x <= areaMaxX; x += prediction.PointSpacing)  // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + prediction.PointSpacing / 2d; y <= areaMaxY; y += prediction.PointSpacing)
                {
                    Tuple<double, double> location = new Tuple<double, double>(x, y);
                    PostGIS.Point point = new PostGIS.Point(x, y, Configuration.PostgisSRID);
                    if (!incidentLocations.Contains(location))
                        nullPoints.Add(new Tuple<PostGIS.Point, string, DateTime>(point, PointPrediction.NullLabel, DateTime.MinValue));
                }

            List<int> nullPointIds = Point.Insert(connection, nullPoints, prediction.Id, area, vacuum);

            Set<int> incidentPointIndexesInArea = new Set<int>(area.Contains(incidentPoints.Select(p => p.Item1).ToList()).ToArray());
            incidentPoints = incidentPoints.Where((p, i) => incidentPointIndexesInArea.Contains(i)).ToList();
            int maxSampleSize = training ? TrainingSampleSize : PredictionSampleSize;
            int numIncidentsToRemove = (nullPointIds.Count + incidentPoints.Count) - maxSampleSize;
            if (numIncidentsToRemove > 0)
            {
                numIncidentsToRemove = Math.Min(incidentPoints.Count, numIncidentsToRemove);

                Console.Out.WriteLine("WARNING:  removing " + numIncidentsToRemove + " random incident(s) to meet point count constraints. Increase the model's " + (training ? "training" : "prediction") + " sample size by this amount in order to use all available incident data.");

                incidentPoints.Randomize(new Random(1240894));
                incidentPoints.RemoveRange(0, numIncidentsToRemove);
            }

            if (training && incidentPoints.Count == 0)
                throw new ZeroPositivePointsException("Zero positive incident points inserted into point sample for \"" + prediction.IncidentTypes.Concatenate(", ") + "\"." + (numIncidentsToRemove > 0 ? " This is due to sample size restrictions. Either increase the training sample size to " + (maxSampleSize + numIncidentsToRemove) + " to include all incidents or increase the point spacing to reduce the number of null points." : ""));

            Point.Insert(connection, incidentPoints, prediction.Id, null, vacuum);
        }

        protected virtual IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training, int idOfSpatiotemporallyIdenticalPrediction)
        {
            int numFeatures = prediction.SelectedFeatures.Count(f => f.EnumType == typeof(SpatialDistanceFeature)) + (_externalFeatureExtractor == null ? 0 : _externalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(SpatialDistanceDCM)));

            Dictionary<int, NumericFeature> idFeature = new Dictionary<int, NumericFeature>();
            foreach (Feature f in prediction.SelectedFeatures)
                idFeature.Add(f.Id, new NumericFeature(f.Id.ToString()));

            #region spatial distance features
            Console.Out.WriteLine("Extracting spatial distance feature values");

            List<Dictionary<int, FeatureVector>> corePointIdFeatureVector = new List<Dictionary<int, FeatureVector>>(Configuration.ProcessorCount);
            Set<Thread> threads = new Set<Thread>();
            for (int i = 0; i < Configuration.ProcessorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        int core = (int)o;
                        string tableName = "temp_" + core;
                        NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT p." + Point.Columns.Id + " as point_id," +
                                                                            "p." + Point.Columns.Location + " as point_location," +
                                                                            "p." + Point.Columns.Time + " as point_time," +
                                                                            "st_expand(p." + Point.Columns.Location + "," + FeatureDistanceThreshold + ") as point_bounding_box," +
                                                                            "p." + Point.Columns.IncidentType + " as point_incident_type," +
                                                                            "f." + Feature.Columns.Id + " as feature_id," +
                                                                            "f." + Feature.Columns.ResourceId + "::integer as shapefile_id " +
                                                                     "INTO " + tableName + " " +
                                                                     "FROM " + Point.GetTable(prediction.Id, false) + " p LEFT JOIN " + Feature.Table + " f ON f." + Feature.Columns.PredictionId + "=" + prediction.Id + " AND " +
                                                                                                                                                              "f." + Feature.Columns.EnumType + "='" + typeof(SpatialDistanceFeature) + "' AND " +
                                                                                                                                                              "f." + Feature.Columns.EnumValue + "='" + SpatialDistanceFeature.DistanceShapeFile + "' " +
                                                                     "WHERE p." + Point.Columns.Core + "=" + core + ";" +

                                                                     "CREATE INDEX ON " + tableName + " (point_id);" +
                                                                     "CREATE INDEX ON " + tableName + " (point_time);" +
                                                                     "CREATE INDEX ON " + tableName + " USING GIST (point_location);" +
                                                                     "CREATE INDEX ON " + tableName + " USING GIST (point_bounding_box);" +
                                                                     "CREATE INDEX ON " + tableName + " (point_incident_type);" +
                                                                     "CREATE INDEX ON " + tableName + " (feature_id);" +
                                                                     "CREATE INDEX ON " + tableName + " (shapefile_id);");
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "VACUUM ANALYZE " + tableName;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "SELECT " + tableName + ".point_id as " + tableName + "_" + Point.Columns.Id + "," +
                                                      tableName + ".point_time as " + tableName + "_" + Point.Columns.Time + "," +
                                                      tableName + ".point_incident_type as " + tableName + "_" + Point.Columns.IncidentType + "," +
                                                      "st_x(" + tableName + ".point_location) as " + Point.Columns.X(tableName) + "," +
                                                      "st_y(" + tableName + ".point_location) as " + Point.Columns.Y(tableName) + "," +
                                                      "st_srid(" + tableName + ".point_location) as " + Point.Columns.SRID(tableName) + "," +
                                                      tableName + ".feature_id," +
                                                     "CASE WHEN COUNT(sfg." + ShapeFileGeometry.Columns.Geometry + ")=0 THEN sqrt(2.0 * " + FeatureDistanceThreshold + "^2) " + // with a bounding box of FeatureDistanceThreshold around each point, the maximum distance between a point and some feature shapefile geometry would be sqrt(2*FeatureDistanceThreshold^2). That is, the feature shapefile geometry would be positioned in one of the corners of the bounding box.
                                                     "ELSE min(st_distance(st_closestpoint(sfg." + ShapeFileGeometry.Columns.Geometry + "," + tableName + ".point_location)," + tableName + ".point_location)) " +
                                                     "END as feature_value " +

                                          "FROM " + tableName + " LEFT JOIN " + ShapeFileGeometry.Table + " sfg ON " + tableName + ".shapefile_id=sfg." + ShapeFileGeometry.Columns.ShapeFileId + " AND sfg." + ShapeFileGeometry.Columns.Geometry + " && " + tableName + ".point_bounding_box " +

                                          "GROUP BY " + tableName + ".point_id," + tableName + ".point_time," + tableName + ".point_incident_type," + tableName + ".point_location," + tableName + ".feature_id";

                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        Dictionary<int, FeatureVector> pointIdFeatureVector = new Dictionary<int, FeatureVector>();
                        while (reader.Read())
                        {
                            int pointId = Convert.ToInt32(reader[tableName + "_" + Point.Columns.Id]);

                            FeatureVector vector;
                            if (!pointIdFeatureVector.TryGetValue(pointId, out vector))
                            {
                                Point p = new Point(reader, tableName);
                                p.TrueClass = p.IncidentType;
                                vector = new FeatureVector(p, numFeatures);
                                pointIdFeatureVector.Add(pointId, vector);
                            }

                            object featureIdObject = reader["feature_id"];
                            if (!(featureIdObject is DBNull))
                            {
                                int featureId = Convert.ToInt32(featureIdObject);
                                double featureValue = Convert.ToDouble(reader["feature_value"]);
                                vector.Add(idFeature[featureId], featureValue, false);
                            }
                        }
                        reader.Close();

                        lock (corePointIdFeatureVector) { corePointIdFeatureVector.Add(pointIdFeatureVector); }

                        cmd.CommandText = "DROP TABLE " + tableName;
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

            #region kde
            List<Feature> kdeFeatures = prediction.SelectedFeatures.Where(f => f.EnumValue.Equals(SpatialDistanceFeature.IncidentKernelDensityEstimate)).ToList();
            if (kdeFeatures.Count > 0)
            {
                IEnumerable<PostGIS.Point> kdeEvalPoints = mergedVectors.Select(v => (v.DerivedFrom as Point).Location);
                Dictionary<Feature, List<float>> kdeFeatureDensityEstimates = new Dictionary<Feature, List<float>>(kdeFeatures.Count);
                threads = new Set<Thread>(Configuration.ProcessorCount);
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int skip = (int)o;
                            foreach (Feature kdeFeature in kdeFeatures)
                                if (skip-- <= 0)
                                {
                                    Console.Out.WriteLine("Computing spatial density of \"" + kdeFeature.ResourceId + "\"");

                                    IEnumerable<PostGIS.Point> kdeInputPoints = Incident.Get(TrainingStart, TrainingEnd, kdeFeature.ResourceId).Select(incident => incident.Location);
                                    List<float> densityEstimates = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, 500, false, 0, 0, kdeEvalPoints, true, true);
                                    lock (kdeFeatureDensityEstimates)
                                    {
                                        kdeFeatureDensityEstimates.Add(kdeFeature, densityEstimates);
                                    }

                                    skip = Configuration.ProcessorCount - 1;
                                }
                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                foreach (Feature kdeFeature in kdeFeatureDensityEstimates.Keys)
                {
                    int vecNum = 0;
                    NumericFeature numericFeature = new NumericFeature(kdeFeature.Id.ToString());
                    foreach (float densityEstimate in kdeFeatureDensityEstimates[kdeFeature])
                        mergedVectors[vecNum++].Add(numericFeature, densityEstimate);
                }
            }
            #endregion

            if (_externalFeatureExtractor != null)
            {
                Console.Out.WriteLine("Running external feature extractor for " + typeof(SpatialDistanceDCM));

                foreach (FeatureVectorList featureVectors in _externalFeatureExtractor.ExtractFeatures(typeof(SpatialDistanceDCM), prediction, mergedVectors, training, idOfSpatiotemporallyIdenticalPrediction))
                    yield return featureVectors;
            }
            else
                yield return mergedVectors;
        }

        public void SelectFeatures(Prediction prediction, bool runPredictionAfterSelect)
        {
            Set<int> selectedFeatureIds = new Set<int>(_classifier.SelectFeatures(prediction).ToArray());
            prediction.SelectedFeatures = prediction.SelectedFeatures.Where(f => selectedFeatureIds.Contains(f.Id));

            if (runPredictionAfterSelect)
            {
                prediction.DeletePoints();
                Run(prediction, -1, true, false, true);
                prediction.MostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
                prediction.UpdateEvaluation();
            }
        }

        public override int Run(Prediction prediction, int idOfSpatiotemporallyIdenticalPrediction)
        {
            return Run(prediction, idOfSpatiotemporallyIdenticalPrediction, true, _classifier.RunFeatureSelection, true);
        }

        public int Run(Prediction prediction, int idOfSpatiotemporallyIdenticalPrediction, bool train, bool runFeatureSelection, bool predict)
        {
            if (prediction.SelectedFeatures.Count() == 0)
                throw new Exception("Must select one or more features.");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            try
            {
                _classifier.Initialize(prediction);

                #region training
                if (train)
                {
                    Console.Out.WriteLine("Creating training grid");

                    InsertPointsIntoPrediction(cmd.Connection, prediction, true, true);

                    #region feature selection
                    if (runFeatureSelection)
                    {
                        Console.Out.WriteLine("Running feature selection");

                        foreach (FeatureVectorList vectors in ExtractFeatureVectors(prediction, true, idOfSpatiotemporallyIdenticalPrediction))
                            _classifier.Consume(vectors, prediction);

                        _classifier.Train(prediction);

                        SelectFeatures(prediction, false);

                        if (idOfSpatiotemporallyIdenticalPrediction == -1)
                            idOfSpatiotemporallyIdenticalPrediction = prediction.Id;  // we've already run feature extraction and so should be able to reuse data from here on out
                    }
                    #endregion

                    foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, true, idOfSpatiotemporallyIdenticalPrediction))
                        _classifier.Consume(featureVectors, prediction);

                    Console.Out.WriteLine("Training model");

                    _classifier.Train(prediction);
                }
                #endregion

                #region prediction
                if (predict)
                {
                    Console.Out.WriteLine("Creating prediction grid");

                    prediction.DeletePoints();
                    InsertPointsIntoPrediction(cmd.Connection, prediction, false, true);

                    StreamWriter pointPredictionLog = new StreamWriter(new GZipStream(new FileStream(prediction.PointPredictionLogPath, FileMode.Create, FileAccess.Write), CompressionMode.Compress));

                    foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, false, idOfSpatiotemporallyIdenticalPrediction))
                    {
                        Console.Out.WriteLine("Making predictions");

                        _classifier.Classify(featureVectors, prediction);

                        foreach (FeatureVector vector in featureVectors.OrderBy(v => (v.DerivedFrom as Point).Id))  // sort by point ID so prediction log is sorted
                        {
                            if (vector.Count == 0)
                            {
                                foreach (string label in vector.DerivedFrom.PredictionConfidenceScores.Keys.ToArray())
                                    if (label != PointPrediction.NullLabel)
                                        vector.DerivedFrom.PredictionConfidenceScores[label] = 0;

                                vector.DerivedFrom.PredictionConfidenceScores[PointPrediction.NullLabel] = 1;
                            }
                            else if (_classifyNonZeroVectorsUniformly)
                            {
                                foreach (string label in vector.DerivedFrom.PredictionConfidenceScores.Keys.ToArray())
                                    if (label == PointPrediction.NullLabel)
                                        vector.DerivedFrom.PredictionConfidenceScores[PointPrediction.NullLabel] = 0;
                                    else
                                        vector.DerivedFrom.PredictionConfidenceScores[label] = 1 / (float)(vector.DerivedFrom.PredictionConfidenceScores.Count - 1);
                            }

                            #region log feature values
                            pointPredictionLog.Write((vector.DerivedFrom as Point).Id + " <p><ls>");
                            foreach (string label in vector.DerivedFrom.PredictionConfidenceScores.SortKeysByValues(true))
                                if (label == PointPrediction.NullLabel || prediction.IncidentTypes.Contains(label))
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

                    Smooth(prediction);
                }
                #endregion

                LastRun = DateTime.Now;

                Console.Out.WriteLine(GetType().FullName + " prediction complete.");

                return prediction.Id;
            }
            catch (Exception ex)
            {
                try { prediction.Delete(); }
                catch (Exception) { }

                throw ex;
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);
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
            return Create(null, Name, PointSpacing, _featureDistanceThreshold, _classifyNonZeroVectorsUniformly, GetType(), TrainingArea, TrainingStart, TrainingEnd, TrainingSampleSize, PredictionSampleSize, IncidentTypes, _classifier.Copy(), Smoothers);
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

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Classifier:  " + _classifier.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "External feature extractor:  " + (_externalFeatureExtractor == null ? "None" : _externalFeatureExtractor.GetDetails(indentLevel + 1)) + Environment.NewLine +
                   indent + "Feature distance threshold:  " + _featureDistanceThreshold + Environment.NewLine +
                   indent + "Classify nonzero vectors uniformly:  " + _classifyNonZeroVectorsUniformly;
        }

        internal override void ChangeFeatureIds(Prediction prediction, Dictionary<int, int> oldNewFeatureId)
        {
            _classifier.ChangeFeatureIds(prediction, oldNewFeatureId);
        }
    }
}
