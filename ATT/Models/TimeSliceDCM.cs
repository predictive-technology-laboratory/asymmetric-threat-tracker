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

            FeatureExtractor externalFeatureExtractor;
            if (Configuration.TryGetFeatureExtractor(typeof(TimeSliceDCM), out externalFeatureExtractor))
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

        protected TimeSliceDCM(string name,
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

        protected override IEnumerable<FeatureVectorList> ExtractFeatureVectors(Prediction prediction, bool training)
        {
            foreach (FeatureVectorList vectors in base.ExtractFeatureVectors(prediction, training))
            {
                Dictionary<TimeSliceFeature, string> featureId = new Dictionary<TimeSliceFeature, string>();
                Dictionary<TimeSliceFeature, NumericFeature> featureNumeric = new Dictionary<TimeSliceFeature, NumericFeature>();
                Dictionary<TimeSliceFeature, NominalFeature> featureNominal = new Dictionary<TimeSliceFeature, NominalFeature>();
                foreach (Feature f in Features)
                    if (f.EnumType == typeof(TimeSliceFeature))
                    {
                        TimeSliceFeature feature = (TimeSliceFeature)f.EnumValue;

                        featureId.Add(feature, f.Id);
                        featureNumeric.Add(feature, new NumericFeature(f.Id));
                        featureNominal.Add(feature, new NominalFeature(f.Id));
                    }

                long sliceTicks = TimeSliceTicks;
                long firstSlice = (long)((training ? prediction.Model.TrainingStart.Ticks : prediction.PredictionStartTime.Ticks) / sliceTicks);
                long lastSlice = (long)((training ? prediction.Model.TrainingEnd.Ticks : prediction.PredictionEndTime.Ticks) / sliceTicks);
                int numSlices = (int)(lastSlice - firstSlice + 1);
                long ticksPerHour = new TimeSpan(1, 0, 0).Ticks;
                int numFeatures = Features.Count(f => f.EnumType == typeof(TimeSliceFeature)) + (ExternalFeatureExtractor == null ? 0 : ExternalFeatureExtractor.GetNumFeaturesExtractedFor(prediction, typeof(TimeSliceDCM)));
                List<TimeSliceFeature> dayIntervals = new List<TimeSliceFeature>(new TimeSliceFeature[] { TimeSliceFeature.LateNight, TimeSliceFeature.EarlyMorning, TimeSliceFeature.Morning, TimeSliceFeature.MidMorning, TimeSliceFeature.Afternoon, TimeSliceFeature.MidAfternoon, TimeSliceFeature.Evening, TimeSliceFeature.Night });

                Console.Out.WriteLine("Extracting " + featureId.Count + " time slice features for " + vectors.Count + " spatial points across " + numSlices + " time slices");

                List<FeatureVectorList> coreFeatureVectors = new List<FeatureVectorList>(Configuration.ProcessorCount);
                int slicesPerCore = (numSlices / Configuration.ProcessorCount) + 1;
                Set<Thread> threads = new Set<Thread>();
                for (int core = 0; core < Configuration.ProcessorCount; ++core)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            Tuple<long, long> startEnd = o as Tuple<long, long>;

                            FeatureVectorList featureVectors = new FeatureVectorList(slicesPerCore * vectors.Count);
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
                                        string id;
                                        if (featureId.TryGetValue(dayIntervals[interval], out id))
                                            intervalFeatures.Add(new NumericFeature(id));
                                    }
                                    else
                                        break;
                                }
                                #endregion

                                foreach (FeatureVector vector in vectors)
                                {
                                    Point spatialPoint = vector.DerivedFrom as Point;

                                    Point timeSlicePoint;
                                    if (spatialPoint.Time == DateTime.MinValue)
                                        timeSlicePoint = new Point(spatialPoint.Id, spatialPoint.IncidentType, spatialPoint.Location, sliceMid);
                                    else if ((long)(spatialPoint.Time.Ticks / sliceTicks) == slice)
                                        timeSlicePoint = new Point(spatialPoint.Id, spatialPoint.IncidentType, spatialPoint.Location, spatialPoint.Time);
                                    else
                                        continue;

                                    FeatureVector timeSliceVector = new FeatureVector(timeSlicePoint, numFeatures);
                                    timeSliceVector.DerivedFrom.TrueClass = spatialPoint.TrueClass;
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

                    foreach (FeatureVectorList externalVectors in ExternalFeatureExtractor.ExtractFeatures(typeof(TimeSliceDCM), prediction, timeSliceVectors, training))
                        yield return externalVectors;
                }
                else
                    yield return timeSliceVectors;
            }
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
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   "Time slice hours:  " + _timeSliceHours + Environment.NewLine +
                   "Time slices per period:  " + _periodTimeSlices;
        }
    }
}
