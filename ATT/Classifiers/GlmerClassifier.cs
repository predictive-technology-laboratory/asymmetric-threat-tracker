using LAIR.MachineLearning;
using LAIR.MachineLearning.FeatureNameTransformation;
using LAIR.MachineLearning.FeatureSpaceRepresentation;
using LAIR.ResourceAPIs.R;
using PTL.ATT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Classifiers
{
    [Serializable]
   public class GlmerClassifier : Classifier
    {
        static GlmerClassifier()
        {
          R.InstallPackages(R.CheckForMissingPackages(new string[] { "lme4" }), Configuration.RCranMirror, Configuration.RPackageInstallDirectory);
        }
        private string _zipcodeFeatureName;

        public string ZipcodeFeatureName
        {
            get { return _zipcodeFeatureName; }
            set { _zipcodeFeatureName = value; }
        }
        

        private string RawTrainPath { get { return Path.Combine(Model.ModelDirectory, "TrainRaw.csv"); } }

        private string ColumnMaxMinPath { get { return Path.Combine(Model.ModelDirectory, "MaxMin.csv"); } }

        private string GlmerModelPath { get { return Path.Combine(Model.ModelDirectory, "glmer.RData"); } }

        private string RawPredictionInstancesPath { get { return Path.Combine(Model.ModelDirectory, "PredRaw.csv"); } }

        private string PredictionsPath { get { return Path.Combine(Model.ModelDirectory, "Predictions.csv"); } }

        public GlmerClassifier()
            : this(false, null )
        {
        }
        public GlmerClassifier(bool runFeatureSelection, FeatureBasedDCM model )
            : base(runFeatureSelection, model)
        {
           
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
            string otherFeatures = "";
          
            string zipCodeFeature = "";
            foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
            {
                if (f.Description == ZipcodeFeatureName)
                    zipCodeFeature = f.Id;
                else
                    otherFeatures += "X" + f.Id + "+";
            }
            string incidentType = Model.IncidentTypes.First();

            StringBuilder rCmd = new StringBuilder(@"
trainRaw=read.csv(""" + RawTrainPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
trainNorm=trainRaw
trainNorm$X" + zipCodeFeature + @" <- as.factor(trainNorm$X" + zipCodeFeature + @")
trainNorm[is.na(trainNorm)]=0
trainNorm$Class<-as.character(trainNorm$Class)
trainNorm$Class[which (trainNorm$Class=='" + incidentType + @"')]<-'1'
trainNorm$Class[which (trainNorm$Class=='NULL')]<-'0'
trainNorm$Class<-as.factor(trainNorm$Class)
library(lme4)
gl=glmer(Class ~  " + otherFeatures + @"  (1   | X" + zipCodeFeature + @"), data=trainNorm, family = binomial)" + @"
save(gl, file=""" + GlmerModelPath.Replace("\\", "/") + @""")" + @"
");
            string output="", error="";
             R.Execute(rCmd.ToString(), false, out output, out error);

            try
            {
                if (!File.Exists(GlmerModelPath))
                    throw new Exception("Hierarchical model was not created at \"" + GlmerModelPath + "\".");
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Hierarchical failed to build model. Output and error messages follow:" + Environment.NewLine +
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
            throw new NotImplementedException("Feature selection has not been implemented for Hierarchical classifiers.");
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
                string zipCodeFeature = "";
                foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                    if (f.Description == ZipcodeFeatureName)
                        zipCodeFeature = f.Id;
                string incidentType = Model.IncidentTypes.First();

                StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
predNorm=predRaw
predNorm$X" + zipCodeFeature + @" <- as.factor(predNorm$X" + zipCodeFeature + @")
predNorm[is.na(predNorm)]=0
library(lme4)
load(file=""" + GlmerModelPath.Replace("\\", "/") + @""")" + @"
gl.pred=predict(gl, predNorm,   type='response')
dfp<-data.frame(gl.pred)
dfp[,'NULL']<-(1-dfp$gl.pred)
names(dfp)[names(dfp)=='gl.pred'] <- '"+incidentType+@"'
write.table(dfp, file=""" + PredictionsPath.Replace("\\", "/") + @""", row.names=FALSE, sep=',')" + @"
");
                string output="", error="";
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
                    throw new Exception("ERROR:  Hierarchical failed to classify points. Output and error messages follow:" + Environment.NewLine +
                                        "\tException message:  " + ex.Message + Environment.NewLine +
                                        "\tR output:  " + output + Environment.NewLine +
                                        "\tR orror:  " + error);
                }
                finally
                {
                    try { File.Delete(ColumnMaxMinPath); }
                    catch { }
                    try { File.Delete(GlmerModelPath); }
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
            return "No details available for Hierarchical predictions.";
        }

        public override Classifier Copy()
        {
            return new GlmerClassifier(RunFeatureSelection, Model );
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
                   indent + "Zipcode feature name:  "+ZipcodeFeatureName  ;
        }
    }
}
