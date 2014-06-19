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
using System.IO;
using LAIR.ResourceAPIs.R;

namespace PTL.ATT.Smoothers
{
    public class MarsSmoother : Smoother
    {
        static MarsSmoother()
        {
            if (Configuration.RCranMirror != null)
                R.InstallPackages(R.CheckForMissingPackages(new string[] { "earth" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }

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

                using (StreamWriter evalPointsFile = new StreamWriter(evalPointsPath))
                {
                    foreach (PostGIS.Point p in pointPredictions.Select(p => idPoint[p.PointId].Location))
                        evalPointsFile.WriteLine(p.X + "," + p.Y);
                    evalPointsFile.Close();
                }

                foreach (string incident in pointPredictions[0].IncidentScore.Keys.ToArray())
                    if (incident != PointPrediction.NullLabel)
                    {
                        using (StreamWriter inputPointsFile = new StreamWriter(inputPointsPath))
                        {
                            inputPointsFile.WriteLine("threat,x,y");
                            foreach (PointPrediction pointPrediction in pointPredictions)
                            {
                                PostGIS.Point location = idPoint[pointPrediction.PointId].Location;
                                inputPointsFile.WriteLine(pointPrediction.IncidentScore[incident] + "," + location.X + "," + location.Y);
                            }
                            inputPointsFile.Close();
                        }

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

                prediction.SmoothingDetails = GetSmoothingDetails();
            }
        }

        public override string GetSmoothingDetails()
        {
            return base.GetSmoothingDetails() + "knots=" + _numberOfKnots + ", parent terms=" + _consideredParentTerms + ", interaction degree=" + _interactionDegree;
        }
    }
}
