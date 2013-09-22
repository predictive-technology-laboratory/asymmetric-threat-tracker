#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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

                        List<float> density = KernelDensityDCM.GetDensityEstimate(kdeInputPoints, _sampleSize, false, 0, 0, kdeEvalPoints, _normalize, false);
                        for (int i = 0; i < density.Count; ++i)
                            pointPredictions[i].IncidentScore[incident] = density[i];
                    }

                foreach (PointPrediction pointPrediction in pointPredictions)
                    pointPrediction.TotalThreat = pointPrediction.IncidentScore.Keys.Sum(incident => incident == PointPrediction.NullLabel ? 0 : pointPrediction.IncidentScore[incident]);

                PointPrediction.UpdateThreatScores(pointPredictions, prediction.Id);
            }

            prediction.Smoothing = GetSmoothingDetails();
        }        

        public void Update(int sampleSize)
        {
            _sampleSize = sampleSize;
        }

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "normalize=" + _normalize + ", sample size=" + _sampleSize;
        }
    }
}
