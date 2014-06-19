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
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using LAIR.MachineLearning;
using LAIR.Extensions;
using System.Drawing;
using PTL.ATT.Evaluation;
using System.Threading;
using System.IO;
using System.Collections;
using PTL.ATT.Smoothers;

namespace PTL.ATT.Models
{
    [Serializable]
    public class TimeSliceDCM : FeatureBasedDCM
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

        public new static IEnumerable<Feature> GetAvailableFeatures(Area area)
        {
            foreach (Feature f in FeatureBasedDCM.GetAvailableFeatures(area))
                yield return f;

            foreach (TimeSliceFeature f in Enum.GetValues(typeof(TimeSliceFeature)))
                yield return new Feature(typeof(TimeSliceFeature), f, null, null, f.ToString(), null);

            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(null, typeof(TimeSliceDCM));
            if (externalFeatureExtractor != null)
                foreach (Feature f in externalFeatureExtractor.GetAvailableFeatures(area))
                    yield return f;
        }

        private int _timeSliceHours;
        private int _periodTimeSlices;
        private long _timeSliceTicks;

        public int TimeSliceHours
        {
            get { return _timeSliceHours; }
            set
            {
                _timeSliceHours = value;
                _timeSliceTicks = new TimeSpan(_timeSliceHours, 0, 0).Ticks;
                Update();
            }
        }

        public int PeriodTimeSlices
        {
            get { return _periodTimeSlices; }
            set
            {
                _periodTimeSlices = value;
                Update();
            }
        }

        public long TimeSliceTicks
        {
            get { return _timeSliceTicks; }
        }

        public TimeSliceDCM() : base() { }

        public TimeSliceDCM(string name,
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
                            IEnumerable<Feature> features,
                            int timeSliceHours,
                            int periodTimeSlices)
            : base(name, pointSpacing, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers, featureDistanceThreshold, trainingSampleSize, predictionSampleSize, classifier, features)
        {
            _timeSliceHours = timeSliceHours;
            _periodTimeSlices = periodTimeSlices;
            _timeSliceTicks = new TimeSpan(_timeSliceHours, 0, 0).Ticks;

            Update();
        }

        protected override IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training, DateTime start, DateTime end)
        {
            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(TimeSliceDCM));

            #region create feature lookups
            Dictionary<TimeSliceFeature, string> featureId = new Dictionary<TimeSliceFeature, string>();
            Dictionary<TimeSliceFeature, NumericFeature> featureNumeric = new Dictionary<TimeSliceFeature, NumericFeature>();
            Dictionary<TimeSliceFeature, NominalFeature> featureNominal = new Dictionary<TimeSliceFeature, NominalFeature>();
            foreach (Feature f in Features.Where(f => f.EnumType == typeof(TimeSliceFeature)))
            {
                TimeSliceFeature feature = (TimeSliceFeature)f.EnumValue;

                featureId.Add(feature, f.Id);
                featureNumeric.Add(feature, new NumericFeature(f.Id));
                featureNominal.Add(feature, new NominalFeature(f.Id));
            }
            #endregion

            long firstSlice = (long)((training ? prediction.Model.TrainingStart.Ticks : prediction.PredictionStartTime.Ticks) / _timeSliceTicks);
            long lastSlice = (long)((training ? prediction.Model.TrainingEnd.Ticks : prediction.PredictionEndTime.Ticks) / _timeSliceTicks);
            int numSlices = (int)(lastSlice - firstSlice + 1);
            long ticksPerHour = new TimeSpan(1, 0, 0).Ticks;
            int numTimeSliceFeatures = featureId.Count + (externalFeatureExtractor == null ? 0 : externalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(TimeSliceDCM)));
            int slicesPerCore = (numSlices / Configuration.ProcessorCount) + 1;
            List<TimeSliceFeature> dayIntervalFeatures = new List<TimeSliceFeature>(new TimeSliceFeature[] { TimeSliceFeature.LateNight, TimeSliceFeature.EarlyMorning, TimeSliceFeature.Morning, TimeSliceFeature.MidMorning, TimeSliceFeature.Afternoon, TimeSliceFeature.MidAfternoon, TimeSliceFeature.Evening, TimeSliceFeature.Night });

            List<FeatureVectorList> coreFeatureVectors = new List<FeatureVectorList>(Configuration.ProcessorCount);
            Set<Thread> threads = new Set<Thread>();
            for (int core = 0; core < Configuration.ProcessorCount; ++core)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        Tuple<long, long> startSliceEndSlice = o as Tuple<long, long>;

                        FeatureVectorList featureVectors = new FeatureVectorList(slicesPerCore * prediction.Points.Count);
                        for (long slice = startSliceEndSlice.Item1; slice <= startSliceEndSlice.Item2; ++slice)
                        {
                            DateTime sliceStart = new DateTime(slice * _timeSliceTicks);
                            DateTime sliceEnd = sliceStart.Add(new TimeSpan(_timeSliceTicks - 1));
                            DateTime sliceMid = new DateTime((sliceStart.Ticks + sliceEnd.Ticks) / 2L);

                            #region get interval features
                            List<LAIR.MachineLearning.Feature> intervalFeatures = new List<LAIR.MachineLearning.Feature>();
                            int dayIntervalStart = sliceStart.Hour / 3;
                            int dayIntervalEnd = (int)(((sliceEnd.Ticks - sliceStart.Ticks) / ticksPerHour) / 3);
                            Set<int> coveredIntervals = new Set<int>(false);
                            for (int i = dayIntervalStart; i <= dayIntervalEnd; ++i)
                            {
                                int interval = i % 8;
                                if (coveredIntervals.Add(interval))
                                {
                                    string id;
                                    if (featureId.TryGetValue(dayIntervalFeatures[interval], out id))
                                        intervalFeatures.Add(new NumericFeature(id));
                                }
                                else
                                    break;
                            }
                            #endregion

                            #region extract feature vectors
                            foreach (FeatureVectorList vectors in base.ExtractFeatureVectors(prediction, training, sliceStart, sliceEnd))
                            {
                                Console.Out.WriteLine("Extracting " + featureId.Count + " time slice features for " + vectors.Count + " points.");

                                foreach (FeatureVector vector in vectors)
                                {
                                    Point point = vector.DerivedFrom as Point;

                                    Point timeSlicePoint;
                                    if (point.Time == DateTime.MinValue)
                                        timeSlicePoint = new Point(point.Id, point.IncidentType, point.Location, sliceMid);
                                    else if ((long)(point.Time.Ticks / _timeSliceTicks) == slice)
                                        timeSlicePoint = new Point(point.Id, point.IncidentType, point.Location, point.Time);
                                    else
                                        throw new Exception("Point should not be in slice:  " + point);

                                    FeatureVector timeSliceVector = new FeatureVector(timeSlicePoint, vector.Count + numTimeSliceFeatures);
                                    timeSliceVector.DerivedFrom.TrueClass = point.TrueClass;
                                    timeSliceVector.Add(vector);

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
                            #endregion
                        }

                        lock (coreFeatureVectors)
                        {
                            coreFeatureVectors.Add(featureVectors);
                        }
                    }));

                long startSlice = firstSlice + core * slicesPerCore;
                long endSlice = Math.Min(startSlice + slicesPerCore - 1, lastSlice);
                t.Start(new Tuple<long, long>(startSlice, endSlice));
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();

            FeatureVectorList timeSliceVectors = new FeatureVectorList(coreFeatureVectors.SelectMany(l => l), coreFeatureVectors.Sum(l => l.Count));

            coreFeatureVectors = null;

            if (externalFeatureExtractor != null)
            {
                Console.Out.WriteLine("Running external feature extractor for " + typeof(TimeSliceDCM));
                foreach (FeatureVectorList externalVectors in externalFeatureExtractor.ExtractFeatures(prediction, timeSliceVectors, training, start, end))
                    yield return externalVectors;
            }
            else
                yield return timeSliceVectors;
        }

        public override string GetPointIdForLog(int id, DateTime time)
        {
            long slice = time.Ticks / _timeSliceTicks;
            return slice + "-" + id;
        }

        public override DiscreteChoiceModel Copy()
        {
            return new TimeSliceDCM(Name, PointSpacing, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, FeatureDistanceThreshold, TrainingSampleSize, PredictionSampleSize, Classifier, Features, _timeSliceHours, _periodTimeSlices);
        }

        public override string ToString()
        {
            return "Time slice DCM:  " + Name;
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            FeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(this, typeof(TimeSliceDCM));
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Time slice hours:  " + _timeSliceHours + Environment.NewLine +
                   indent + "Time slices per period:  " + _periodTimeSlices + Environment.NewLine +
                   indent + "External feature extractor (" + typeof(TimeSliceDCM) + "):  " + (externalFeatureExtractor == null ? "None" : externalFeatureExtractor.GetDetails(indentLevel + 1));
        }
    }
}
