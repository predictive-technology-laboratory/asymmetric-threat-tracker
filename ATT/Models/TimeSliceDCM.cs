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
            foreach (Feature feature in Features.Where(f => f.EnumType == typeof(TimeSliceFeature)))
                featureId.Add((TimeSliceFeature)feature.EnumValue, feature.Id);

            List<TimeSliceFeature> threeHourIntervals = new List<TimeSliceFeature>(new TimeSliceFeature[] { TimeSliceFeature.LateNight, TimeSliceFeature.EarlyMorning, TimeSliceFeature.Morning, TimeSliceFeature.MidMorning, TimeSliceFeature.Afternoon, TimeSliceFeature.MidAfternoon, TimeSliceFeature.Evening, TimeSliceFeature.Night });

            int processorCount = Configuration.ProcessorCount;
            Configuration.ProcessorCount = 1; // all sub-threads (e.g., those in FeatureBasedDCM) should use 1 core, since we're multi-threading here
            Set<Thread> threads = new Set<Thread>(processorCount);
            long firstSlice = (long)((training ? prediction.Model.TrainingStart.Ticks : prediction.PredictionStartTime.Ticks) / _timeSliceTicks);
            long lastSlice = (long)((training ? prediction.Model.TrainingEnd.Ticks : prediction.PredictionEndTime.Ticks) / _timeSliceTicks);
            long ticksPerHour = new TimeSpan(1, 0, 0).Ticks;
            List<FeatureVectorList> completeFeatureVectorLists = new List<FeatureVectorList>();
            List<FeatureVectorList> incompleteFeatureVectorLists = new List<FeatureVectorList>();
            AutoResetEvent emitCompleteFeatureVectorLists = new AutoResetEvent(false);
            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
            for (int i = 0; i < processorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(core =>
                    {
                        for (long slice = firstSlice + (int)core; slice <= lastSlice; slice += processorCount)
                        {
                            Console.Out.WriteLine("Processing slice " + (slice - firstSlice + 1) + " of " + (lastSlice - firstSlice + 1));

                            DateTime sliceStart = new DateTime(slice * _timeSliceTicks);
                            DateTime sliceEnd = sliceStart.Add(new TimeSpan(_timeSliceTicks - 1));
                            DateTime sliceMid = new DateTime((sliceStart.Ticks + sliceEnd.Ticks) / 2L);

                            #region get interval features that are true for all points in the current slice
                            Dictionary<NumericFeature, int> threeHourIntervalFeatureValue = new Dictionary<NumericFeature, int>();
                            int startingThreeHourInterval = sliceStart.Hour / 3;                                                  // which 3-hour interval does the current slice start in?
                            int threeHourIntervalsTouched = (int)(((sliceEnd.Ticks - sliceStart.Ticks) / ticksPerHour) / 3) + 1;  // how many 3-hour intervals does the current slice touch?
                            int endingThreeHourInterval = startingThreeHourInterval + threeHourIntervalsTouched - 1;              // which 3-hour interval does the current slice end in?
                            for (int k = 0; k < threeHourIntervals.Count; ++k)
                            {
                                TimeSliceFeature threeHourInterval = threeHourIntervals[k];
                                string id;
                                if (featureId.TryGetValue(threeHourInterval, out id))  // if the current model uses the current 3-hour interval as a feature
                                {
                                    bool covered = false;
                                    for (int interval = startingThreeHourInterval; !covered && interval <= endingThreeHourInterval; ++interval)
                                        if (interval % 8 == k)
                                            covered = true;

                                    threeHourIntervalFeatureValue.Add(IdNumericFeature[id], covered ? 1 : 0);
                                }
                            }
                            #endregion

                            #region extract feature vectors
                            foreach (FeatureVectorList featureVectors in base.ExtractFeatureVectors(prediction, training, sliceStart, sliceEnd))
                            {
                                if (!featureVectors.Complete)
                                    throw new Exception("Incomplete feature vectors received from base class extractor");

                                Console.Out.WriteLine("Extracting " + featureId.Count + " time slice features for " + featureVectors.Count + " points.");

                                foreach (FeatureVector featureVector in featureVectors)
                                {
                                    Point point = featureVector.DerivedFrom as Point;

                                    if (point.Time == DateTime.MinValue)
                                        point.Time = sliceMid;
                                    else if ((long)(point.Time.Ticks / _timeSliceTicks) != slice)
                                        throw new Exception("Point should not be in slice:  " + point);

                                    foreach (LAIR.MachineLearning.NumericFeature threeHourIntervalFeature in threeHourIntervalFeatureValue.Keys)
                                        featureVector.Add(threeHourIntervalFeature, threeHourIntervalFeatureValue[threeHourIntervalFeature]);

                                    double percentThroughPeriod = (slice % _periodTimeSlices) / (double)(_periodTimeSlices - 1);
                                    double radians = 2 * Math.PI * percentThroughPeriod;

                                    foreach (TimeSliceFeature feature in featureId.Keys)
                                        if (feature == TimeSliceFeature.CosinePeriodPosition)
                                            featureVector.Add(IdNumericFeature[featureId[feature]], Math.Cos(radians));
                                        else if (feature == TimeSliceFeature.SinePeriodPosition)
                                            featureVector.Add(IdNumericFeature[featureId[feature]], Math.Sin(radians));
                                }

                                if (externalFeatureExtractor == null)
                                    lock (completeFeatureVectorLists)
                                    {
                                        completeFeatureVectorLists.Add(featureVectors);
                                        emitCompleteFeatureVectorLists.Set();
                                    }
                                else
                                    foreach (FeatureVectorList externalFeatureVectors in externalFeatureExtractor.ExtractFeatures(prediction, featureVectors, training, sliceStart, sliceEnd, false))
                                        if (externalFeatureVectors.Complete)
                                            lock (completeFeatureVectorLists)
                                            {
                                                completeFeatureVectorLists.Add(externalFeatureVectors);
                                                emitCompleteFeatureVectorLists.Set();
                                            }
                                        else
                                            lock (incompleteFeatureVectorLists)
                                                incompleteFeatureVectorLists.Add(externalFeatureVectors);
                            }
                            #endregion
                        }

                        lock (threads)
                            threads.Remove(Thread.CurrentThread);

                        emitCompleteFeatureVectorLists.Set();
                    }));

                lock (threads) { threads.Add(t); }
                t.Start(i);
            }

            while (emitCompleteFeatureVectorLists.WaitOne())
            {
                lock (completeFeatureVectorLists)
                {
                    foreach (FeatureVectorList completeFeatureVectors in completeFeatureVectorLists)
                        yield return completeFeatureVectors;

                    completeFeatureVectorLists.Clear();
                }

                lock (threads)
                    if (threads.Count == 0)
                        break;
            }

            // emit any remaining completed vectors, which might have arrived just before the last thread was removed (breaking out of the loop above)
            foreach (FeatureVectorList completeFeatureVectors in completeFeatureVectorLists)
                yield return completeFeatureVectors;
            completeFeatureVectorLists.Clear();

            Configuration.ProcessorCount = processorCount;  // reset system-wide processor count since we're done with threads here

            foreach (FeatureVectorList incompleteFeatureVectors in incompleteFeatureVectorLists)
                foreach (FeatureVectorList externalFeatureVectors in externalFeatureExtractor.ExtractFeatures(prediction, incompleteFeatureVectors, training, start, end, true))
                    yield return externalFeatureVectors;
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
