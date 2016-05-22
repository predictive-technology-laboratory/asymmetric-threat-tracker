
using LAIR.MachineLearning;
using LAIR.MachineLearning.FeatureNameTransformation;
using LAIR.MachineLearning.FeatureSpaceRepresentation;
using LAIR.ResourceAPIs.R;
using PTL.ATT.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Classifiers
{
    [Serializable]
   public class MultiTaskClassifier : Classifier
    {
        private string jarPath;
        static MultiTaskClassifier()
        {
          
        }
        private string _zipcodeFeatureName;

        public string ZipcodeFeatureName
        {
            get { return _zipcodeFeatureName; }
            set { _zipcodeFeatureName = value; }
        }
        

        private string RawTrainPath { get { return Path.Combine(Model.ModelDirectory, "TrainRaw.csv"); } }

        public double AdaptationRate;

        private string RawPredictionInstancesPath { get { return Path.Combine(Model.ModelDirectory, "PredRaw.csv"); } }

        private string PredictionsPath { get { return Path.Combine(Model.ModelDirectory, "Predictions.csv"); } }

        public MultiTaskClassifier()
            : this(false, null,"zip",0.01 )
        {
         
        }
        public MultiTaskClassifier(bool runFeatureSelection, FeatureBasedDCM model, string ZipcodeFeatureName,double AdaptationRate)
            : base(runFeatureSelection, model)
        {
            this.ZipcodeFeatureName = ZipcodeFeatureName;
            this.AdaptationRate = AdaptationRate;
        }
        public override void Initialize()
        {
            jarPath = Configuration.ClassifierTypeOptions[GetType()]["path"];
            // write training file
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
            int truePoints=0;
            if (featureVectors != null && featureVectors.Count > 0)
            {
                using (StreamWriter trainingFile = new StreamWriter(RawTrainPath, true))
                {
                    foreach (FeatureVector vector in featureVectors)
                    {
                        if (vector.DerivedFrom.TrueClass != "NULL") truePoints++;
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
            string numericalColumns = "";
            int columnNumber = 2;
            string zipCodeFeature = "";
            int numberOfFeatures = Model.Features.Count;
            foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
            {

                if (f.Description == ZipcodeFeatureName+" (attribute)")
                    zipCodeFeature = f.Id;
                else
                    numericalColumns +=  columnNumber  + ",";
                columnNumber++;
            }
            string incidentType = Model.IncidentTypes.First();

            StringBuilder rCmd = new StringBuilder(@"
trainRaw=read.csv(""" + RawTrainPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
trainNorm=trainRaw
trainNorm$X" + zipCodeFeature + @" <- as.factor(trainNorm$X" + zipCodeFeature + @")
trainNorm[is.na(trainNorm)]=0
trainNorm$Class<-as.character(trainNorm$Class)
trainNorm$Class[which (trainNorm$Class=='" + incidentType + @"')]<-'1'
trainNorm$Class[which (trainNorm$Class=='NULL')]<-'-1'
trainNorm$Class<-as.factor(trainNorm$Class)
write.table(data.frame(trainNorm), file=""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", row.names=FALSE, col.names=TRUE, sep=',',quote=FALSE)");
            string output = "", error = "";
            R.Execute(rCmd.ToString(), false, out output, out error);


             #region train
             // start external application
             using (Process classifyProcess = new Process())
             {

                 classifyProcess.StartInfo.FileName = "java";
                 classifyProcess.StartInfo.Arguments = "-jar \""+jarPath+"\" adapt_MT 0 " + numberOfFeatures + " " + zipCodeFeature + " \"" + RawPredictionInstancesPath.Replace("\\", "/") + "\" \"" + Model.ModelDirectory + "\" " + "\"_\" \"" + incidentType + "\" " + AdaptationRate;
                 classifyProcess.StartInfo.CreateNoWindow = true;
                 classifyProcess.StartInfo.UseShellExecute = false;
                 classifyProcess.StartInfo.RedirectStandardOutput = true;
                 classifyProcess.StartInfo.RedirectStandardError = true;

                 if (classifyProcess.Start())
                 {
                     // read output and error...some programs stall if this is not read
                     string poutput = classifyProcess.StandardOutput.ReadToEnd().Trim();
                     string perror = classifyProcess.StandardError.ReadToEnd().Trim();

                     classifyProcess.WaitForExit();
                     if (perror != "")
                         throw new Exception(perror);
                 }
                 else
                     throw new Exception("Failed to start train process");
             }



             #endregion
             try { File.Delete(RawPredictionInstancesPath); }
             catch { }
        }
        public override IEnumerable<string> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection has not been implemented for MT-SVM classifiers.");
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
                string numericalColumns = "";
                int columnNumber = 2;
                string zipCodeFeature = "";
                foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
                {
                    if (f.Description == ZipcodeFeatureName + " (attribute)")
                        zipCodeFeature = f.Id;
                    else
                        numericalColumns += columnNumber + ",";
                    columnNumber++;
                }
                string incidentType = Model.IncidentTypes.First();

                StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
predNorm=predRaw
predNorm$X" + zipCodeFeature + @" <- as.factor(predNorm$X" + zipCodeFeature + @")
predNorm[is.na(predNorm)]=0
predNorm$Class<-as.character(predNorm$Class)
predNorm$Class[which (predNorm$Class=='" + incidentType + @"')]<-'1'
predNorm$Class[which (predNorm$Class=='NULL')]<-'-1'
write.table(data.frame(predNorm), file=""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", row.names=FALSE, col.names=TRUE, sep=',',quote=FALSE)");
                string output = "", error = "";
                R.Execute(rCmd.ToString(), false, out output, out error);
                #region classifiy
               
                // start external application
                using (Process classifyProcess = new Process())
                {

                    classifyProcess.StartInfo.FileName = "java";
                    classifyProcess.StartInfo.Arguments = "-jar \""+jarPath+"\" predict_MT 0 " + Model.Features.Count + " " + zipCodeFeature + " \"" + RawPredictionInstancesPath.Replace("\\", "/") + "\" \"" + Model.ModelDirectory + "\" " + "\"_\" \"" + incidentType + "\" " + AdaptationRate;
                    classifyProcess.StartInfo.CreateNoWindow = true;
                    classifyProcess.StartInfo.UseShellExecute = false;
                    classifyProcess.StartInfo.RedirectStandardOutput = true;
                    classifyProcess.StartInfo.RedirectStandardError = true;

                    if (classifyProcess.Start())
                    {
                        // read output and error...some programs stall if this is not read
                        string poutput = classifyProcess.StandardOutput.ReadToEnd().Trim();
                        string perror = classifyProcess.StandardError.ReadToEnd().Trim();

                        classifyProcess.WaitForExit();
                        if (perror != "")
                            throw new Exception(perror);
                    }
                    else
                        throw new Exception("Failed to start classify process");
                }



                #endregion 
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
                    throw new Exception("ERROR:  MT-SVM failed to classify points. Output and error messages follow:" + Environment.NewLine +
                                        "\tException message:  " + ex.Message + Environment.NewLine +
                                        "\tR output:  " + output + Environment.NewLine +
                                        "\tR orror:  " + error);
                }
                finally
                {
                    try { File.Delete(RawPredictionInstancesPath); }
                    catch { }
                }
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation)
        {
            return "No details available for MT-SVM predictions.";
        }

        public override Classifier Copy()
        {
            return new MultiTaskClassifier(RunFeatureSelection, Model,ZipcodeFeatureName,AdaptationRate);
        }

        internal override void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId)
        {
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine  +
                   indent + "zipcode feature name:  " + ZipcodeFeatureName;
        }
    }
}
