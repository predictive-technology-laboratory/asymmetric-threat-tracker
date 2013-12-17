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
using LAIR.MachineLearning;
using LAIR.MachineLearning.FeatureNameTransformation;
using System.IO;
using LAIR.ResourceAPIs.R;
using LAIR.MachineLearning.ClassifierWrappers;
using PostGIS = LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.Classifiers
{
    [Serializable]
    public class RandomForest : Classifier
    {
        static RandomForest()
        {
            R.InstallPackages(R.CheckForMissingPackages(new string[] { "randomForest" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }

        [NonSerialized]
        private Prediction _prediction;
        private int _numTrees;

        public int NumTrees
        {
            get { return _numTrees; }
            set { _numTrees = value; }
        }

        public RandomForest()
            : this(false, -1, 500)
        {
        }

        public RandomForest(bool runFeatureSelection, int modelId, int numTrees)
            : base(runFeatureSelection, modelId)
        {
            _numTrees = numTrees;
        }

        public override void Initialize(Prediction prediction)
        {
            base.Initialize(prediction);

            StreamWriter trainingFile = new StreamWriter(Path.Combine(prediction.ModelDirectory, "TrainRaw.csv"), true);
            trainingFile.Write("Class");
            foreach (Feature f in prediction.SelectedFeatures.OrderBy(i => i.Id))
                trainingFile.Write("," + f.Id);
            trainingFile.WriteLine();
            trainingFile.Close();

            _prediction = prediction;
        }

        public override void Consume(FeatureVectorList featureVectors, Prediction prediction)
        {
            base.Consume(featureVectors, prediction);

            if (featureVectors != null && featureVectors.Count > 0)
            {
                StreamWriter trainingFile = new StreamWriter(Path.Combine(prediction.ModelDirectory, "TrainRaw.csv"), true);
                
                foreach (FeatureVector vector in featureVectors)
                {
                    trainingFile.Write(vector.DerivedFrom.TrueClass);
                    foreach (LAIR.MachineLearning.Feature f in vector.OrderBy(i => int.Parse(i.Name)))
                        trainingFile.Write("," + vector[f]);
                    trainingFile.WriteLine();
                }

                trainingFile.Close();
            }
        }
        
        protected override void BuildModel()
        {
            StringBuilder rCmd = new StringBuilder(@"
trainRaw=read.csv(""" + Path.Combine(_prediction.ModelDirectory, @"TrainRaw.csv").Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
trainNorm=trainRaw
cols=NCOL(trainRaw)
mxmn <- array(0, dim=c(2,cols-1))
options(scipen=999)
for(i in 2:cols) {
  cmax=max(trainRaw[,i])
  cmin=min(trainRaw[,i])
  mxmn[1,i-1]=cmax
  mxmn[2,i-1]=cmin
  trainNorm[,i]=(trainRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
}
write.table(data.frame(mxmn), file=""" + Path.Combine(_prediction.ModelDirectory, @"MaxMin.csv").Replace("\\", "/") + @""", row.names=FALSE, col.names=FALSE, sep=',')" + @"
write.csv(trainNorm, file=""" + Path.Combine(_prediction.ModelDirectory, @"TrainNorm.csv").Replace("\\", "/") + @""")" + @"
library(randomForest)
rf=randomForest(Class ~., data=trainNorm, ntree=" + _numTrees + ", importance=TRUE, seed=99)" + @"
save(rf, file=""" + Path.Combine(_prediction.ModelDirectory, @"rf.RData").Replace("\\", "/") + @""")" + @"
");
            R.Execute(rCmd.ToString(), false);
        }

        public override IEnumerable<int> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection has not been implemented for RandomForest classifiers.");
        }

        public override void Classify(FeatureVectorList featureVectors, Prediction prediction)
        {
            if (featureVectors != null && featureVectors.Count > 0)
            {
                StreamWriter predictionsFile = new StreamWriter(Path.Combine(prediction.ModelDirectory, "PredRaw.csv"));

                predictionsFile.Write("Class");
                foreach (Feature f in prediction.SelectedFeatures.OrderBy(i => i.Id))
                    predictionsFile.Write("," + f.Id);
                predictionsFile.WriteLine();

                foreach (FeatureVector vector in featureVectors)
                {
                    predictionsFile.Write(vector.DerivedFrom.TrueClass);
                    foreach (LAIR.MachineLearning.Feature f in vector.OrderBy(i => int.Parse(i.Name)))
                        predictionsFile.Write("," + vector[f]);
                    predictionsFile.WriteLine();
                }
                predictionsFile.Close();

                StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + Path.Combine(_prediction.ModelDirectory, @"PredRaw.csv").Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
mxmn=read.csv(""" + Path.Combine(_prediction.ModelDirectory, @"MaxMin.csv").Replace("\\", "/") + @""", header = FALSE, sep = ',')" + @"
predNorm=predRaw
for(i in 2:NCOL(predRaw)) {
  cmax=mxmn[1,i-1]
  cmin=mxmn[2,i-1]
  predNorm[,i] = (predRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
}
write.csv(predNorm, file=""" + Path.Combine(_prediction.ModelDirectory, @"PredNorm.csv").Replace("\\", "/") + @""")" + @"
library(randomForest)
load(file=""" + Path.Combine(_prediction.ModelDirectory, @"rf.RData").Replace("\\", "/") + @""")" + @"
rf.pred=predict(rf, predNorm, norm.votes=TRUE, type='prob')
dfp<-data.frame(rf.pred)
names(dfp)[names(dfp)=='NULL.'] <- 'NULL'
write.table(dfp, file=""" + Path.Combine(_prediction.ModelDirectory, @"Predictions.csv").Replace("\\", "/") + @""", row.names=FALSE, sep=',')" + @"
");
                R.Execute(rCmd.ToString(), false);

                StreamReader dataReader = new StreamReader(Path.Combine(prediction.ModelDirectory, "Predictions.csv"));

                string[] colnames = dataReader.ReadLine().Split(',');
                int row = 0;
                string line;

                while ((line = dataReader.ReadLine()) != null)
                {
                    string[] lines = line.Split(',');

                    for (int i = 0; i < colnames.Length; i++)
                    {
                        string label = colnames[i].Replace("\"", @"");
                        label = label.Replace(".", " ");
                        float prob = float.Parse(lines[i]);
                        featureVectors[row].DerivedFrom.PredictionConfidenceScores.Add(label, prob);
                    }
                    row++;
                }
                predictionsFile.Close();

                if (row != featureVectors.Count)
                    throw new Exception("Number of predictions doesn't match number of input vectors");
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<int, string> attFeatureIdInformation)
        {
            return "No details available for AdaBoost predictions.";
        }

        public override Classifier Copy()
        {
            return new RandomForest(RunFeatureSelection, ModelId, _numTrees);
        }

        internal override void ChangeFeatureIds(Prediction prediction, Dictionary<int, int> oldNewFeatureId)
        {
            throw new NotImplementedException();
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Number of trees:  " + _numTrees;
        }
    }
}
