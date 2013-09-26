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
                            int skip = (int)o;
                            List<Tuple<PointPrediction, Dictionary<string, double>>> threadPointPredictionIncidentScore = new List<Tuple<PointPrediction, Dictionary<string, double>>>((int)((pointPredictions.Count / (float)Configuration.ProcessorCount) + 1));
                            foreach (PointPrediction pointPrediction in pointPredictions)
                                if (skip-- <= 0)
                                {
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

                                    skip = Configuration.ProcessorCount - 1;
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

                PointPrediction.UpdateThreatScores(pointPredictions, prediction.Id);

                prediction.Smoothing = GetSmoothingDetails();
            }
        }

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "minimum distance=" + _minimum + ", maximum distance=" + _maximum;
        }
    }
}
