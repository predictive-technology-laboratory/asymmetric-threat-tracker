#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using System.IO;
using LAIR.ResourceAPIs.R;

namespace PTL.ATT.Smoothers
{
    public class MarsSmoother : Smoother
    {
        private int _numberOfKnots;
        private int _consideredParentTerms;
        private int _interactionDegree;

        public int NumberOfKnots
        {
            get { return _numberOfKnots; }
            set { _numberOfKnots = value; }
        }

        public int ConsideredParentTerms
        {
            get { return _consideredParentTerms; }
            set { _consideredParentTerms = value; }
        }

        public int InteractionDegree
        {
            get { return _interactionDegree; }
            set { _interactionDegree = value; }
        }

        public MarsSmoother()
        {
            _numberOfKnots = -1;
            _consideredParentTerms = 20;
            _interactionDegree = 1;
        }

        public override void Apply(Prediction prediction)
        {
            List<PointPrediction> pointPredictions = prediction.PointPredictions.ToList();

            if (pointPredictions.Count > 0)
            {
                Dictionary<int, Point> idPoint = new Dictionary<int, Point>();
                foreach (Point p in prediction.Points)
                    idPoint.Add(p.Id, p);

                string inputPointsPath = Path.GetTempFileName();
                string evalPointsPath = Path.GetTempFileName();
                string outputPath = Path.GetTempFileName();

                StreamWriter evalPointsFile = new StreamWriter(evalPointsPath);
                foreach (PostGIS.Point p in pointPredictions.Select(p => idPoint[p.PointId].Location))
                    evalPointsFile.WriteLine(p.X + "," + p.Y);
                evalPointsFile.Close();

                foreach (string incident in pointPredictions[0].IncidentScore.Keys.ToArray())
                    if (incident != PointPrediction.NullLabel)
                    {
                        StreamWriter inputPointsFile = new StreamWriter(inputPointsPath);
                        inputPointsFile.WriteLine("threat,x,y");
                        foreach (PointPrediction pointPrediction in pointPredictions)
                        {
                            PostGIS.Point location = idPoint[pointPrediction.PointId].Location;
                            inputPointsFile.WriteLine(pointPrediction.IncidentScore[incident] + "," + location.X + "," + location.Y);
                        }
                        inputPointsFile.Close();

                        R.Execute(@"
library(earth)
input.points = read.csv(""" + inputPointsPath.Replace(@"\", @"\\") + @""",header=TRUE)
model = earth(threat ~ ., data = input.points, " + (_numberOfKnots == -1 ? "" : "nk = " + _numberOfKnots + ", ") + "fast.k = " + _consideredParentTerms + ", degree = " + _interactionDegree + @")

eval.points = read.csv(""" + evalPointsPath.Replace(@"\", @"\\") + @""",header=FALSE)
prediction = predict(model, eval.points, type=""response"")
prediction = (prediction - min(prediction)) / (max(prediction) - min(prediction))

write.table(prediction,file=""" + outputPath.Replace(@"\", @"\\") + @""",row.names=FALSE,col.names=FALSE)", false);

                        int pointNum = 0;
                        foreach (string line in File.ReadLines(outputPath))
                            pointPredictions[pointNum++].IncidentScore[incident] = double.Parse(line);
                    }

                File.Delete(inputPointsPath);
                File.Delete(evalPointsPath);
                File.Delete(outputPath);

                foreach (PointPrediction pointPrediction in pointPredictions)
                    pointPrediction.TotalThreat = pointPrediction.IncidentScore.Keys.Sum(incident => incident == PointPrediction.NullLabel ? 0 : pointPrediction.IncidentScore[incident]);

                PointPrediction.UpdateThreatScores(pointPredictions, prediction.Id);

                prediction.Smoothing = GetSmoothingDetails();
            }
        }

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "knots=" + _numberOfKnots + ", parent terms=" + _consideredParentTerms + ", interaction degree=" + _interactionDegree;
        }
    }
}
