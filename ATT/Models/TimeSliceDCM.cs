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

            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
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
                            IEnumerable<string> incidentTypes,
                            Area trainingArea,
                            DateTime trainingStart,
                            DateTime trainingEnd,
                            IEnumerable<Smoother> smoothers,
                            int trainingPointSpacing,
                            int featureDistanceThreshold,
                            int negativePointStandoff,
                            PTL.ATT.Classifiers.Classifier classifier,
                            IEnumerable<Feature> features,
                            int timeSliceHours,
                            int periodTimeSlices)
            : base(name, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers, trainingPointSpacing, featureDistanceThreshold, negativePointStandoff, classifier, features)
        {
            _timeSliceHours = timeSliceHours;
            _periodTimeSlices = periodTimeSlices;
            _timeSliceTicks = new TimeSpan(_timeSliceHours, 0, 0).Ticks;

            Update();
        }

        protected override int GetNumFeaturesExtractedFor(Prediction prediction)
        {
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
            return base.GetNumFeaturesExtractedFor(prediction) + Features.Where(f => f.EnumType == typeof(TimeSliceFeature)).Count() + (externalFeatureExtractor == null ? 0 : externalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(TimeSliceDCM)));
        }

        protected override IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training, DateTime start, DateTime end)
        {
            Dictionary<TimeSliceFeature, string> featureId = new Dictionary<TimeSliceFeature, string>();
            foreach (Feature f in Features.Where(f => f.EnumType == typeof(TimeSliceFeature)))
            {
                TimeSliceFeature feature = (TimeSliceFeature)f.EnumValue;
                featureId.Add(feature, f.Id);
            }

            long firstSlice = (long)((training ? prediction.Model.TrainingStart.Ticks : prediction.PredictionStartTime.Ticks) / _timeSliceTicks);
            long lastSlice = (long)((training ? prediction.Model.TrainingEnd.Ticks : prediction.PredictionEndTime.Ticks) / _timeSliceTicks);
            int numSlices = (int)(lastSlice - firstSlice + 1);
            long ticksPerHour = new TimeSpan(1, 0, 0).Ticks;
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
                            Console.Out.WriteLine("Processing slice " + (slice - firstSlice + 1) + " of " + numSlices);

                            DateTime sliceStart = new DateTime(slice * _timeSliceTicks);
                            DateTime sliceEnd = sliceStart.Add(new TimeSpan(_timeSliceTicks - 1));
                            DateTime sliceMid = new DateTime((sliceStart.Ticks + sliceEnd.Ticks) / 2L);

                            #region get interval features that are true for all points in the current slice
                            List<NominalFeature> intervalFeatures = new List<NominalFeature>();
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
                                        intervalFeatures.Add(IdNominalFeature[id]);
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

                                    if (point.Time == DateTime.MinValue)
                                        point.Time = sliceMid;
                                    else if ((long)(point.Time.Ticks / _timeSliceTicks) != slice)
                                        throw new Exception("Point should not be in slice:  " + point);

                                    foreach (LAIR.MachineLearning.Feature feature in intervalFeatures)
                                        vector.Add(feature, true);

                                    foreach (TimeSliceFeature feature in featureId.Keys)
                                    {
                                        double percentThroughPeriod = (slice % _periodTimeSlices) / (double)(_periodTimeSlices - 1);
                                        double radians = 2 * Math.PI * percentThroughPeriod;

                                        if (feature == TimeSliceFeature.CosinePeriodPosition)
                                            vector.Add(IdNumericFeature[featureId[feature]], Math.Cos(radians));
                                        else if (feature == TimeSliceFeature.SinePeriodPosition)
                                            vector.Add(IdNumericFeature[featureId[feature]], Math.Sin(radians));
                                    }

                                    featureVectors.Add(vector);
                                }
                            }
                            #endregion
                        }

                        lock (coreFeatureVectors) { coreFeatureVectors.Add(featureVectors); }
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

            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
            if (externalFeatureExtractor != null)
            {
                Console.Out.WriteLine("Running external feature extractor for " + externalFeatureExtractor.ModelType);
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
            return new TimeSliceDCM(Name, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, TrainingPointSpacing, FeatureDistanceThreshold, NegativePointStandoff, Classifier, Features, _timeSliceHours, _periodTimeSlices);
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

            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Time slice hours:  " + _timeSliceHours + Environment.NewLine +
                   indent + "Time slices per period:  " + _periodTimeSlices + Environment.NewLine +
                   indent + "External feature extractor (" + typeof(TimeSliceDCM) + "):  " + (externalFeatureExtractor == null ? "None" : externalFeatureExtractor.GetDetails(indentLevel + 1));
        }
    }
}
