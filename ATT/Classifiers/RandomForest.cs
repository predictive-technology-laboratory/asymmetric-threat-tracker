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
using LAIR.MachineLearning;
using LAIR.MachineLearning.FeatureNameTransformation;
using System.IO;
using LAIR.ResourceAPIs.R;
using LAIR.MachineLearning.ClassifierWrappers;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using PTL.ATT.Models;

namespace PTL.ATT.Classifiers
{
    [Serializable]
    public class RandomForest : Classifier
    {
        static RandomForest()
        {
            R.InstallPackages(R.CheckForMissingPackages(new string[] { "randomForest" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }

        private int _numTrees;

        public int NumTrees
        {
            get { return _numTrees; }
            set { _numTrees = value; }
        }

        private string RawTrainPath { get { return Path.Combine(Model.ModelDirectory, "TrainRaw.csv"); } }

        private string ColumnMaxMinPath { get { return Path.Combine(Model.ModelDirectory, "MaxMin.csv"); } }

        private string RandomForestModelPath { get { return Path.Combine(Model.ModelDirectory, "rf.RData"); } }

        private string RawPredictionInstancesPath { get { return Path.Combine(Model.ModelDirectory, "PredRaw.csv"); } }

        private string PredictionsPath { get { return Path.Combine(Model.ModelDirectory, "Predictions.csv"); } }

        public RandomForest()
            : this(false, null, 500)
        {
        }

        public RandomForest(bool runFeatureSelection, FeatureBasedDCM model, int numTrees)
            : base(runFeatureSelection, model)
        {
            _numTrees = numTrees;
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
                            if (vector.TryGetValue(f.Id, out value))
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
trainRaw=read.csv(""" + RawTrainPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
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
write.table(data.frame(mxmn), file=""" + ColumnMaxMinPath.Replace("\\", "/") + @""", row.names=FALSE, col.names=FALSE, sep=',')" + @"
library(randomForest)
rf=randomForest(Class ~., data=trainNorm, ntree=" + _numTrees + ", importance=TRUE, seed=99)" + @"
save(rf, file=""" + RandomForestModelPath.Replace("\\", "/") + @""")" + @"
");
            string output, error;
            R.Execute(rCmd.ToString(), false, out output, out error);

            try
            {
                if (!File.Exists(RandomForestModelPath))
                    throw new Exception("RandomForest model was not created at \"" + RandomForestModelPath + "\".");
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  RandomForest failed to build model. Output and error messages follow:" + Environment.NewLine +
                                    "\tException message:  " + ex.Message + Environment.NewLine +
                                    "\tR output:  " + output + Environment.NewLine +
                                    "\tR orror:  " + error);
            }
            finally
            {
                try { File.Delete(RawTrainPath); }
                catch { }
            }
        }

        public override IEnumerable<string> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection has not been implemented for RandomForest classifiers.");
        }

        public override void Classify(FeatureVectorList featureVectors)
        {
            base.Classify(featureVectors);

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
                            if (vector.TryGetValue(f.Id, out value))
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
library(randomForest)
load(file=""" + RandomForestModelPath.Replace("\\", "/") + @""")" + @"
rf.pred=predict(rf, predNorm, norm.votes=TRUE, type='prob')
dfp<-data.frame(rf.pred)
names(dfp)[names(dfp)=='NULL.'] <- 'NULL'
write.table(dfp, file=""" + PredictionsPath.Replace("\\", "/") + @""", row.names=FALSE, sep=',')" + @"
");
                string output, error;
                R.Execute(rCmd.ToString(), false, out output, out error);

                try
                {
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
                catch (Exception ex)
                {
                    throw new Exception("ERROR:  RandomForest failed to classify points. Output and error messages follow:" + Environment.NewLine +
                                        "\tException message:  " + ex.Message + Environment.NewLine +
                                        "\tR output:  " + output + Environment.NewLine +
                                        "\tR orror:  " + error);
                }
                finally
                {
                    try { File.Delete(ColumnMaxMinPath); }
                    catch { }
                    try { File.Delete(RandomForestModelPath); }
                    catch { }
                    try { File.Delete(RawPredictionInstancesPath); }
                    catch { }
                    try { File.Delete(PredictionsPath); }
                    catch { }
                }
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation)
        {
            return "No details available for RandomForest predictions.";
        }

        public override Classifier Copy()
        {
            return new RandomForest(RunFeatureSelection, Model, _numTrees);
        }

        internal override void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId)
        {
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
