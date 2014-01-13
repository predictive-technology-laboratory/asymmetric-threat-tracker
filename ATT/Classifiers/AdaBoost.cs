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
    public class AdaBoost : Classifier
    {
        static AdaBoost()
        {
            if (Configuration.RCranMirror != null)
                R.InstallPackages(R.CheckForMissingPackages(new string[] { "ada" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }

        private int _iterations;

        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

        private string RawTrainPath { get { return Path.Combine(Model.ModelDirectory, "TrainRaw.csv"); } }

        private string ColumnMaxMinPath { get { return Path.Combine(Model.ModelDirectory, "MaxMin.csv"); } }

        private string ClassPath { get { return Path.Combine(Model.ModelDirectory, "class.RData"); } }

        private string AdaModelPath { get { return Path.Combine(Model.ModelDirectory, "ada.RData"); } }

        private string RawPredictionInstancesPath { get { return Path.Combine(Model.ModelDirectory, "PredRaw.csv"); } }

        private string PredictionsPath { get { return Path.Combine(Model.ModelDirectory, "Predictions.csv"); } }

        public AdaBoost()
            : this(false, -1, 50)
        {
        }

        public AdaBoost(bool runFeatureSelection, int modelId, int iterations)
            : base(runFeatureSelection, modelId)
        {
            _iterations = iterations;
        }

        public override void Initialize()
        {
            using (StreamWriter trainingFile = new StreamWriter(RawTrainPath, true))
            {
                trainingFile.Write("Class");
                foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                    trainingFile.Write("," + f.Id);
                trainingFile.WriteLine();
                trainingFile.Close();
            }
        }

        public override void Consume(FeatureVectorList featureVectors)
        {
            base.Consume(featureVectors);

            if (featureVectors != null && featureVectors.Count > 0)
            {
                using (StreamWriter trainingFile = new StreamWriter(RawTrainPath, true))
                {
                    foreach (FeatureVector vector in featureVectors)
                    {
                        trainingFile.Write(vector.DerivedFrom.TrueClass);
                        foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                        {
                            object value;
                            if (vector.TryGetValue(f.Id.ToString(), out value))
                                trainingFile.Write("," + value);
                            else
                                trainingFile.Write(",0");
                        }
                        trainingFile.WriteLine();
                    }
                    trainingFile.Close();
                }
            }
        }

        protected override void BuildModel()
        { 
            StringBuilder rCmd = new StringBuilder(@"
trainRaw=read.csv(""" + RawTrainPath.Replace(@"\", "/") + @""", header = TRUE, sep = ',')" + @"
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
trainNorm[is.na(trainNorm)]=0
write.table(data.frame(mxmn), file=""" + ColumnMaxMinPath.Replace(@"\", "/") + @""", row.names=FALSE, col.names=FALSE, sep=',')" + @"
library(ada)
set.seed(99)
cls <- sort(unique(trainNorm$Class))
cList <- vector('list', length(cls))
save(cls, file=""" + ClassPath.Replace(@"\", "/") + @""")" + @"
binForm = ""Class ~.,data=trainNorm,iter=" + _iterations + ",loss='logistic',nu=1,type='discrete'" + @"""
multForm = ""Class ~.,data=cList[[i]],iter=" + _iterations + ",loss='logistic',nu=1,type='discrete'" + @"""
if(length(cls)==2) {
  adb <- eval(parse(text=paste('ada(', binForm, ')', sep='')))
  save(adb, file=""" + AdaModelPath.Replace(@"\", "/") + @""")" + @"
} else {
  for(i in 1:length(cls)) {
    cList[[i]] <- trainNorm
    for(j in 1:length(cls)) {
      if(i!=j) {
        levels(cList[[i]]$Class)[levels(cList[[i]]$Class)==cls[j]] <- 'REST'
      }
    }
    adb <- eval(parse(text=paste('ada(', multForm, ')', sep='')))
    save(adb, file=paste('" + Path.Combine(Model.ModelDirectory, @"ada', i, '.RData', sep='')").Replace("\\", "/") + @")" + @"
  }
}
");
            R.Execute(rCmd.ToString(), false);

            File.Delete(RawTrainPath);
        }

        public override IEnumerable<int> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection has not been implemented for AdaBoost");
        }

        public override void Classify(FeatureVectorList featureVectors)
        {
            if (featureVectors != null && featureVectors.Count > 0)
            {
                using (StreamWriter predictionsFile = new StreamWriter(RawPredictionInstancesPath))
                {
                    predictionsFile.Write("Class");
                    foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                        predictionsFile.Write("," + f.Id);
                    predictionsFile.WriteLine();

                    foreach (FeatureVector vector in featureVectors)
                    {
                        predictionsFile.Write(vector.DerivedFrom.TrueClass);
                        foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                        {
                            object value;
                            if (vector.TryGetValue(f.Id.ToString(), out value))
                                predictionsFile.Write("," + value);
                            else
                                predictionsFile.Write(",0");
                        }
                        predictionsFile.WriteLine();
                    }
                    predictionsFile.Close();
                }

                StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
mxmn=read.csv(""" + ColumnMaxMinPath.Replace("\\", "/") + @""", header = FALSE, sep = ',')" + @"
predNorm=predRaw
for(i in 2:NCOL(predRaw)) {
  cmax=mxmn[1,i-1]
  cmin=mxmn[2,i-1]
  predNorm[,i] = (predRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
}
predNorm[is.na(predNorm)]=0
library(ada)
set.seed(99)
load(file=""" + ClassPath.Replace("\\", "/") + @""")" + @"
if(length(cls)==2) {
  load(file=""" + AdaModelPath.Replace("\\", "/") + @""")" + @"
  adb.pred<-predict(adb, newdata=predNorm, type='prob')
  mult<-data.frame(adb.pred)
  names(mult)<-sort(c(toString(cls[1]), toString(cls[2])))
} else {
  mult<-data.frame(matrix(0, ncol=1, nrow=NROW(predRaw)))
  names(mult)<-c('INIT_DF')
  for(i in 1:length(cls)) {
    load(file=paste('" + Path.Combine(Model.ModelDirectory, @"ada', i, '.RData', sep='')").Replace("\\", "/") + @")" + @"
    adb.pred<-predict(adb, newdata=predNorm, type='prob')
    abp<-data.frame(adb.pred)
    names(abp)<-sort(c(toString(cls[i]), 'REST'))
    abp<-subset(abp, select=-c(REST))
    mult<-cbind(mult, abp)
  }
  mult<-subset(mult, select=-c(INIT_DF))
  mult<-1/(1+exp(-1*mult))
  sums<-data.frame(rowSums(mult))
  for(j in 1:length(cls)) {
    mult[j]<-mult[j]/sums
  }
}
write.table(mult, file=""" + PredictionsPath.Replace("\\", "/") + @""", row.names=FALSE, sep=',')" + @"
");
                R.Execute(rCmd.ToString(), false);

                using (StreamReader predictionsFile = new StreamReader(PredictionsPath))
                {
                    string[] colnames = predictionsFile.ReadLine().Split(',');
                    int row = 0;
                    string line;

                    while ((line = predictionsFile.ReadLine()) != null)
                    {
                        string[] lines = line.Split(',');

                        for (int i = 0; i < colnames.Length; i++)
                        {
                            string label = colnames[i].Replace("\"", @""); ;
                            float prob = float.Parse(lines[i]);
                            featureVectors[row].DerivedFrom.PredictionConfidenceScores.Add(label, prob);
                        }
                        row++;
                    }

                    predictionsFile.Close();

                    if (row != featureVectors.Count)
                        throw new Exception("Number of predictions doesn't match number of input vectors");
                }

                File.Delete(ColumnMaxMinPath);
                File.Delete(ClassPath);
                File.Delete(AdaModelPath);
                File.Delete(RawPredictionInstancesPath);
                File.Delete(PredictionsPath);
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<int, string> attFeatureIdInformation)
        {
            return "No details available for AdaBoost predictions.";
        }

        public override Classifier Copy()
        {
            return new AdaBoost(RunFeatureSelection, ModelId, _iterations);
        }

        internal override void ChangeFeatureIds(Dictionary<int, int> oldNewFeatureId)
        {
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Number of iterations:  " + _iterations;
        }
    }
}
