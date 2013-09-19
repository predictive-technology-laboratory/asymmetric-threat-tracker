using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using LAIR.MachineLearning;
using PTL.ATT.Incidents;
using LAIR.Extensions;
using System.Drawing;
using PTL.ATT.Evaluation;
using System.Threading;
using System.IO;
using System.Collections;
using PTL.ATT.Smoothers;

namespace PTL.ATT.Models
{
    public class TimeSliceDCM : SpatialDistanceDCM
    {
        public enum TimeSliceFeature
        {
            /// <summary>
            /// Sine of period position
            /// </summary>
            SinePeriodPosition,

            /// <summary>
            /// Cosine of period position
            /// </summary>
            CosinePeriodPosition,

            /// <summary>
            /// 12am-3am
            /// </summary>
            LateNight,

            /// <summary>
            /// 3am-6am
            /// </summary>
            EarlyMorning,

            /// <summary>
            /// 6am-9am
            /// </summary>
            Morning,

            /// <summary>
            /// 9am-12pm
            /// </summary>
            MidMorning,

            /// <summary>
            /// 12pm-3pm
            /// </summary>
            Afternoon,

            /// <summary>
            /// 3pm-6pm
            /// </summary>
            MidAfternoon,

            /// <summary>
            /// 6pm-9pm
            /// </summary>
            Evening,

            /// <summary>
            /// 9pm-12am
            /// </summary>
            Night
        }

        public new const string Table = "time_slice_dcm";

        public new class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PeriodTimeSlices = "period_time_slices";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TimeSliceHours = "time_slice_hours";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return SpatialDistanceDCM.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinSpatialDistanceDCM { get { return SpatialDistanceDCM.Columns.JoinDiscreteChoiceModel + " JOIN " + Table + " ON " + SpatialDistanceDCM.Table + "." + SpatialDistanceDCM.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(SpatialDistanceDCM))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + SpatialDistanceDCM.Table + " ON DELETE CASCADE," +
                   Columns.PeriodTimeSlices + " INT," +
                   Columns.TimeSliceHours + " INT);";
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
                                 IEnumerable<Smoother> smoothers,
                                 int timeSliceHours,
                                 int periodTimeSlices)
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
                type = typeof(TimeSliceDCM);

            int spatialDistanceDcmId = SpatialDistanceDCM.Create(cmd.Connection, name, pointSpacing, featureDistanceThreshold, classifyNonZeroVectorsUniformly, type, trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, classifier, smoothers);

            cmd.CommandText = "INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + spatialDistanceDcmId + "," + periodTimeSlices + "," + timeSliceHours + ")";
            cmd.ExecuteNonQuery();

            if (returnConnection)
            {
                cmd.CommandText = "COMMIT";
                cmd.ExecuteNonQuery();
                DB.Connection.Return(cmd.Connection);
            }

            return spatialDistanceDcmId;
        }

        private int _timeSliceHours;
        private long _periodTimeSlices;

        public int TimeSliceHours
        {
            get { return _timeSliceHours; }
        }

        public long PeriodTimeSlices
        {
            get { return _periodTimeSlices; }
        }

        public override IEnumerable<Feature> AvailableFeatures
        {
            get
            {
                foreach (Feature f in base.AvailableFeatures)
                    yield return f;

                foreach (TimeSliceFeature f in Enum.GetValues(typeof(TimeSliceFeature)))
                    yield return new Feature(typeof(TimeSliceFeature), f, null, f.ToString());
            }
        }

        public long TimeSliceTicks
        {
            get { return new TimeSpan(_timeSliceHours, 0, 0).Ticks; }
        }

        internal TimeSliceDCM(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " +
                                                         "FROM " + Columns.JoinSpatialDistanceDCM + " " +
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

            _timeSliceHours = Convert.ToInt32(reader[Table + "_" + Columns.TimeSliceHours]);
            _periodTimeSlices = Convert.ToInt32(reader[Table + "_" + Columns.PeriodTimeSlices]);
        }

        public void Update(string name, int pointSpacing, int featureDistanceThreshold, bool classifyNonZeroVectorsUniformly, Area trainingArea, DateTime trainingStart, DateTime trainingEnd, int trainingSampleSize, int predictionSampleSize, IEnumerable<string> incidentTypes, PTL.ATT.Classifiers.Classifier classifier, IEnumerable<Smoother> smoothers, int timeSliceHours, int periodTimeSlices)
        {
            base.Update(name, pointSpacing, featureDistanceThreshold, classifyNonZeroVectorsUniformly, trainingArea, trainingStart, trainingEnd, trainingSampleSize, predictionSampleSize, incidentTypes, classifier, smoothers);

            _timeSliceHours = timeSliceHours;
            _periodTimeSlices = periodTimeSlices;

            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " +
                                          Columns.TimeSliceHours + "=" + timeSliceHours + "," +
                                          Columns.PeriodTimeSlices + "=" + periodTimeSlices + " " +
                                          "WHERE " + Columns.Id + "=" + Id);
        }

        protected override IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training, int idOfSpatiotemporallyIdenticalPrediction)
        {
            foreach (FeatureVectorList spatialVectors in base.ExtractFeatureVectors(prediction, training, idOfSpatiotemporallyIdenticalPrediction))
            {
                Dictionary<TimeSliceFeature, int> featureId = new Dictionary<TimeSliceFeature, int>();
                Dictionary<TimeSliceFeature, NumericFeature> featureNumeric = new Dictionary<TimeSliceFeature, NumericFeature>();
                Dictionary<TimeSliceFeature, NominalFeature> featureNominal = new Dictionary<TimeSliceFeature, NominalFeature>();
                foreach (Feature f in prediction.SelectedFeatures)
                    if (f.EnumType == typeof(TimeSliceFeature))
                    {
                        TimeSliceFeature feature = (TimeSliceFeature)f.EnumValue;

                        featureId.Add(feature, f.Id);
                        featureNumeric.Add(feature, new NumericFeature(f.Id.ToString()));
                        featureNominal.Add(feature, new NominalFeature(f.Id.ToString()));
                    }

                long sliceTicks = TimeSliceTicks;
                long firstSlice = (long)((training ? prediction.TrainingStartTime.Ticks : prediction.PredictionStartTime.Ticks) / sliceTicks);
                long lastSlice = (long)((training ? prediction.TrainingEndTime.Ticks : prediction.PredictionEndTime.Ticks) / sliceTicks);
                int numSlices = (int)(lastSlice - firstSlice + 1);
                long ticksPerHour = new TimeSpan(1, 0, 0).Ticks;
                int numFeatures = prediction.SelectedFeatures.Count(f => f.EnumType == typeof(TimeSliceFeature)) + (ExternalFeatureExtractor == null ? 0 : ExternalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(TimeSliceDCM)));
                List<TimeSliceFeature> dayIntervals = new List<TimeSliceFeature>(new TimeSliceFeature[] { TimeSliceFeature.LateNight, TimeSliceFeature.EarlyMorning, TimeSliceFeature.Morning, TimeSliceFeature.MidMorning, TimeSliceFeature.Afternoon, TimeSliceFeature.MidAfternoon, TimeSliceFeature.Evening, TimeSliceFeature.Night });

                Console.Out.WriteLine("Extracting " + featureId.Count + " time slice features for " + spatialVectors.Count + " spatial points across " + numSlices + " time slices");

                List<FeatureVectorList> coreFeatureVectors = new List<FeatureVectorList>(Configuration.ProcessorCount);
                int slicesPerCore = (numSlices / Configuration.ProcessorCount) + 1;
                Set<Thread> threads = new Set<Thread>();
                for (int core = 0; core < Configuration.ProcessorCount; ++core)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            Tuple<long, long> startEnd = o as Tuple<long, long>;

                            FeatureVectorList featureVectors = new FeatureVectorList(slicesPerCore * spatialVectors.Count);
                            for (long slice = startEnd.Item1; slice <= startEnd.Item2 && slice <= lastSlice; ++slice)
                            {
                                DateTime sliceStart = new DateTime(slice * sliceTicks);
                                DateTime sliceEnd = sliceStart.Add(new TimeSpan(sliceTicks - 1));
                                DateTime sliceMid = new DateTime((sliceStart.Ticks + sliceEnd.Ticks) / 2L);

                                #region interval features
                                List<LAIR.MachineLearning.Feature> intervalFeatures = new List<LAIR.MachineLearning.Feature>();
                                int dayIntervalStart = sliceStart.Hour / 3;
                                int dayIntervalEnd = (int)(((sliceEnd.Ticks - sliceStart.Ticks) / ticksPerHour) / 3);
                                Set<int> coveredIntervals = new Set<int>(false);
                                for (int i = dayIntervalStart; i <= dayIntervalEnd; ++i)
                                {
                                    int interval = i % 8;
                                    if (coveredIntervals.Add(interval))
                                    {
                                        int id;
                                        if (featureId.TryGetValue(dayIntervals[interval], out id))
                                            intervalFeatures.Add(new NumericFeature(id.ToString()));
                                    }
                                    else
                                        break;
                                }
                                #endregion

                                foreach (FeatureVector spatialVector in spatialVectors)
                                {
                                    Point spatialPoint = spatialVector.DerivedFrom as Point;

                                    Point timeSlicePoint;
                                    if (spatialPoint.Time == DateTime.MinValue)
                                        timeSlicePoint = new Point(spatialPoint.Id, spatialPoint.IncidentType, spatialPoint.Location, sliceMid);
                                    else if ((long)(spatialPoint.Time.Ticks / sliceTicks) == slice)
                                        timeSlicePoint = new Point(spatialPoint.Id, spatialPoint.IncidentType, spatialPoint.Location, spatialPoint.Time);
                                    else
                                        continue;

                                    FeatureVector timeSliceVector = new FeatureVector(timeSlicePoint, numFeatures);
                                    timeSliceVector.DerivedFrom.TrueClass = spatialPoint.TrueClass;
                                    timeSliceVector.Add(spatialVector);

                                    foreach (LAIR.MachineLearning.Feature feature in intervalFeatures)
                                        timeSliceVector.Add(feature, 1);

                                    foreach (TimeSliceFeature feature in featureId.Keys)
                                    {
                                        double percent = (slice % _periodTimeSlices) / (double)(_periodTimeSlices - 1);
                                        double degrees = 360 * percent;
                                        double radians = degrees * (Math.PI / 180d);

                                        if (feature == TimeSliceFeature.CosinePeriodPosition)
                                            timeSliceVector.Add(featureNumeric[feature], Math.Cos(radians));
                                        else if (feature == TimeSliceFeature.SinePeriodPosition)
                                            timeSliceVector.Add(featureNumeric[feature], Math.Sin(radians));
                                    }

                                    featureVectors.Add(timeSliceVector);
                                }
                            }

                            lock (coreFeatureVectors)
                            {
                                coreFeatureVectors.Add(featureVectors);
                            }
                        }));

                    long start = firstSlice + core * slicesPerCore;
                    long end = start + slicesPerCore - 1;
                    t.Start(new Tuple<long, long>(start, end));
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                FeatureVectorList timeSliceVectors = new FeatureVectorList(coreFeatureVectors.SelectMany(l => l), coreFeatureVectors.Sum(l => l.Count));

                coreFeatureVectors = null;

                if (ExternalFeatureExtractor != null)
                {
                    Console.Out.WriteLine("Running external feature extractor for " + typeof(TimeSliceDCM));

                    foreach (FeatureVectorList featureVectors in ExternalFeatureExtractor.ExtractFeatures(typeof(TimeSliceDCM), prediction, timeSliceVectors, training, idOfSpatiotemporallyIdenticalPrediction))
                        yield return featureVectors;
                }
                else
                    yield return timeSliceVectors;
            }
        }

        public override int Copy()
        {
            return Create(null, Name, PointSpacing, FeatureDistanceThreshold, ClassifyNonZeroVectorsUniformly, GetType(), TrainingArea, TrainingStart, TrainingEnd, TrainingSampleSize, PredictionSampleSize, IncidentTypes, Classifier.Copy(), Smoothers, (int)_timeSliceHours, (int)_periodTimeSlices);
        }

        public override string ToString()
        {
            return "Time slice DCM:  " + Name;
        }

        public override string GetDetails(int indentLevel)
        {
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   "Time slice hours:  " + _timeSliceHours + Environment.NewLine +
                   "Time slices per period:  " + _periodTimeSlices;
        }
    }
}
