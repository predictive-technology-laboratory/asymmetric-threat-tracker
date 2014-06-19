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

using PostGIS = LAIR.ResourceAPIs.PostGIS;
using PTL.ATT.Models;

namespace PTL.ATT.Smoothers
{
    [Serializable]
    public class KdeSmoother : Smoother
    {
        private int _sampleSize;
        private bool _normalize;

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; }
        }

        public bool Normalize
        {
            get { return _normalize; }
            set { _normalize = value; }
        }

        public KdeSmoother()
        {
            _sampleSize = 500;
            _normalize = true;
        }

        public override void Apply(Prediction prediction)
        {
            List<PointPrediction> pointPredictions = prediction.PointPredictions.ToList();

            if (pointPredictions.Count > 0)
            {
                Dictionary<int, Point> idPoint = new Dictionary<int, Point>();
                foreach (Point p in prediction.Points)
                    idPoint.Add(p.Id, p);

                IEnumerable<PostGIS.Point> kdeEvalPoints = pointPredictions.Select(p => idPoint[p.PointId].Location);

                List<PostGIS.Point> kdeInputPoints = new List<PostGIS.Point>();
                foreach (string incident in pointPredictions[0].IncidentScore.Keys.ToArray())
                    if (incident != PointPrediction.NullLabel)
                    {
                        double minScore = pointPredictions.Min(p => p.IncidentScore[incident]);
                        kdeInputPoints.Clear();
                        foreach (PointPrediction pointPrediction in pointPredictions)
                        {
                            PostGIS.Point pointPredictionLocation = idPoint[pointPrediction.PointId].Location;
                            double replicates = pointPrediction.IncidentScore[incident] / minScore;
                            for (int i = 0; i < replicates; ++i)
                                kdeInputPoints.Add(pointPredictionLocation);
                        }

                        List<float> density = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, _sampleSize, false, 0, 0, kdeEvalPoints, _normalize);
                        for (int i = 0; i < density.Count; ++i)
                            pointPredictions[i].IncidentScore[incident] = density[i];
                    }

                foreach (PointPrediction pointPrediction in pointPredictions)
                    pointPrediction.TotalThreat = pointPrediction.IncidentScore.Keys.Sum(incident => incident == PointPrediction.NullLabel ? 0 : pointPrediction.IncidentScore[incident]);

                PointPrediction.UpdateThreatScores(pointPredictions, prediction.Id);
            }

            prediction.SmoothingDetails = GetSmoothingDetails();
        }        

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "normalize=" + _normalize + ", sample size=" + _sampleSize;
        }
    }
}
