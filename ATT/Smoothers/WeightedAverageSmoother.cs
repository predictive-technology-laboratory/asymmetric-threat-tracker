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
using LAIR.Extensions;
using System.Threading;
using LAIR.Collections.Generic;

namespace PTL.ATT.Smoothers
{
    [Serializable]
    public class WeightedAverageSmoother : Smoother
    {
        private double _minimum;
        private double _maximum;

        public double Minimum
        {
            get { return _minimum; }
            set { _minimum = value; }
        }

        public double Maximum
        {
            get { return _maximum; }
            set { _maximum = value; }
        }

        public WeightedAverageSmoother()
        {
            _minimum = 0;
            _maximum = 500;
        }

        public override void Apply(Prediction prediction)
        {
            List<PointPrediction> pointPredictions = prediction.PointPredictions.ToList();

            if (pointPredictions.Count > 0)
            {
                Dictionary<int, Point> idPoint = new Dictionary<int, Point>();
                foreach (Point p in prediction.Points)
                    idPoint.Add(p.Id, p);

                List<Tuple<PointPrediction, Dictionary<string, double>>> pointPredictionIncidentScore = new List<Tuple<PointPrediction, Dictionary<string, double>>>(pointPredictions.Count);
                Set<Thread> threads = new Set<Thread>(Configuration.ProcessorCount);
                for (int i = 0; i < Configuration.ProcessorCount; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                        {
                            int core = (int)o;
                            List<Tuple<PointPrediction, Dictionary<string, double>>> threadPointPredictionIncidentScore = new List<Tuple<PointPrediction, Dictionary<string, double>>>((int)((pointPredictions.Count / (float)Configuration.ProcessorCount) + 1));
                            for (int j = 0; j + core < pointPredictions.Count; j += Configuration.ProcessorCount)
                                {
                                    PointPrediction pointPrediction = pointPredictions[j + core];
                                    Dictionary<PointPrediction, double> neighborInvDist = new Dictionary<PointPrediction, double>();
                                    foreach (PointPrediction neighbor in pointPredictions)
                                    {
                                        double distance = idPoint[pointPrediction.PointId].Location.DistanceTo(idPoint[neighbor.PointId].Location);
                                        if (pointPrediction == neighbor || (distance >= _minimum && distance <= _maximum))
                                            neighborInvDist.Add(neighbor, _maximum - distance);
                                    }

                                    double totalInvDistance = neighborInvDist.Values.Sum();

                                    Dictionary<string, double> incidentScore = new Dictionary<string, double>(pointPrediction.IncidentScore.Count);
                                    foreach (string incident in pointPrediction.IncidentScore.Keys)
                                        incidentScore.Add(incident, neighborInvDist.Keys.Sum(neighbor => (neighborInvDist[neighbor] / totalInvDistance) * neighbor.IncidentScore[incident]));

                                    threadPointPredictionIncidentScore.Add(new Tuple<PointPrediction, Dictionary<string, double>>(pointPrediction, incidentScore));

                                }

                            lock (pointPredictionIncidentScore) { pointPredictionIncidentScore.AddRange(threadPointPredictionIncidentScore); }

                        }));

                    t.Start(i);
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                    t.Join();

                foreach (Tuple<PointPrediction, Dictionary<string, double>> pointPredictionScores in pointPredictionIncidentScore)
                {
                    pointPredictionScores.Item1.IncidentScore = pointPredictionScores.Item2;
                    pointPredictionScores.Item1.TotalThreat = pointPredictionScores.Item2.Values.Sum();
                }

                PointPrediction.UpdateThreatScores(pointPredictions, prediction);

                prediction.SmoothingDetails = GetSmoothingDetails();
            }
        }

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "minimum distance=" + _minimum + ", maximum distance=" + _maximum;
        }
    }
}
