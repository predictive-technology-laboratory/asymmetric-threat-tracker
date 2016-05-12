using LAIR.MachineLearning;
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
    public class AreaSpecificClassifier : Classifier
    {   private string RawTrainPath { get { return Path.Combine(Model.ModelDirectory, "TrainRaw.csv"); } }

    private string ColumnMaxMinPath { get { return Path.Combine(Model.ModelDirectory, "MaxMin.csv"); } }
        private string RawPredictionInstancesPath { get { return Path.Combine(Model.ModelDirectory, "PredRaw.csv"); } }

        private string PredictionsPath { get { return Path.Combine(Model.ModelDirectory, "Predictions"+iteration+".csv"); } }

        public AreaSpecificClassifier()
            : this(false, null )
        {
        }
        public AreaSpecificClassifier(bool runFeatureSelection, FeatureBasedDCM model)
            : base(runFeatureSelection, model)
        {
         //   _numFeaturesInEachVector = model.Features.Count;
        }
        public DiscreteChoiceModel GlobalModel;
        public TimeSpan period;
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
            // no build for area specific classifiers, just adaptation
            iteration = 0;
            string densityFeature = "";

            string zipCodeFeature = "";
            foreach (PTL.ATT.Models.Feature f in Model.Features.OrderBy(i => i.Id))
            {
                if (f.Description.Contains("density"))
                    densityFeature = f.Id;
                else
                    zipCodeFeature = f.Id;
            }
            string incidentType = Model.IncidentTypes.First();

            StringBuilder rCmd = new StringBuilder(@"
trainRaw=read.csv(""" + RawTrainPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
trainNorm=trainRaw
NumericalClmns=c(3)
cols=NCOL(trainRaw)
mxmn <- array(0, dim=c(2,cols-1))
options(scipen=999)
for(i in 2:cols) {
  if( i %in% NumericalClmns){
  cmax=max(trainRaw[,i])
  cmin=min(trainRaw[,i])
  mxmn[1,i-1]=cmax
  mxmn[2,i-1]=cmin
  trainNorm[,i]=(trainRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
 }
}
write.table(data.frame(mxmn), file=""" + ColumnMaxMinPath.Replace("\\", "/") + @""", row.names=FALSE, col.names=FALSE, sep=',')"  );
            string output = "", error = "";
            R.Execute(rCmd.ToString(), false, out output, out error);

          
                try { File.Delete(RawTrainPath); }
                catch { }
            
        }
        public override IEnumerable<string> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection has not been implemented for Area Specific classifiers.");
        }
         [NonSerialized]
        private int iteration = 0;
         public void Adapt(FeatureVectorList featureVectors)
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
                     if (!f.Description.Contains("density"))
                         zipCodeFeature = f.Id;
                 string incidentType = Model.IncidentTypes.First();

                 StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
mxmn=read.csv(""" + ColumnMaxMinPath.Replace("\\", "/") + @""", header = FALSE, sep = ',')" + @"
predNorm=predRaw
NumericalClmns=c(3)
for(i in 2:NCOL(predRaw)) {
 if( i %in% NumericalClmns){
  cmax=mxmn[1,i-1]
  cmin=mxmn[2,i-1]
  predNorm[,i] = (predRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
}
}
predNorm$X" + zipCodeFeature + @" <- as.factor(predNorm$X" + zipCodeFeature + @")
predNorm[is.na(predNorm)]=0
predNorm$Class<-as.character(predNorm$Class)
predNorm$Class[which (predNorm$Class=='" + incidentType + @"')]<-'1'
predNorm$Class[which (predNorm$Class=='NULL')]<-'0'
write.table(data.frame(predNorm), file=""" + RawPredictionInstancesPath.Replace("\\", "/") + iteration + @""", row.names=FALSE, col.names=TRUE, sep=',',quote=FALSE)");
                 string output = "", error = "";
                 R.Execute(rCmd.ToString(), false, out output, out error);
                 #region classifiy
                 // start external application
                 using (Process classifyProcess = new Process())
                 {

                     classifyProcess.StartInfo.FileName = "java";
                     classifyProcess.StartInfo.Arguments = "-jar \"C:\\Users\\Mohammad\\Desktop\\ATT research\\LinAdapt\\LinAdapt.jar\" adapt " + iteration + " 2 " + zipCodeFeature + " \"" + RawPredictionInstancesPath.Replace("\\", "/") + iteration + "\" \"" + Model.ModelDirectory + "\" " + "\"" + GlobalModel.ModelDirectory + "\" " + incidentType;
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
                         //if (perror != "")
                         //    throw new Exception(perror);
                     }
                     else
                         throw new Exception("Failed to start classify process");
                 }



                 #endregion
                  

                     try { File.Delete(RawPredictionInstancesPath); }
                     catch { }

                     iteration++;
             }

         }
        public override void Classify(FeatureVectorList featureVectors)
        {
         //   base.Classify(featureVectors);

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
                    if (!f.Description.Contains("density"))
                        zipCodeFeature = f.Id;
                string incidentType = Model.IncidentTypes.First();

                StringBuilder rCmd = new StringBuilder(@"
predRaw=read.csv(""" + RawPredictionInstancesPath.Replace("\\", "/") + @""", header = TRUE, sep = ',')" + @"
mxmn=read.csv(""" + ColumnMaxMinPath.Replace("\\", "/") + @""", header = FALSE, sep = ',')" + @"
predNorm=predRaw
NumericalClmns=c(3)
for(i in 2:NCOL(predRaw)) {
 if( i %in% NumericalClmns){
  cmax=mxmn[1,i-1]
  cmin=mxmn[2,i-1]
  predNorm[,i] = (predRaw[,i]-((cmax+cmin)/2))/((cmax-cmin)/2)
}
}
predNorm$X" + zipCodeFeature + @" <- as.factor(predNorm$X" + zipCodeFeature + @")
predNorm[is.na(predNorm)]=0
predNorm$Class<-as.character(predNorm$Class)
predNorm$Class[which (predNorm$Class=='" + incidentType + @"')]<-'1'
predNorm$Class[which (predNorm$Class=='NULL')]<-'0'
write.table(data.frame(predNorm), file=""" + RawPredictionInstancesPath.Replace("\\", "/") + iteration + @""", row.names=FALSE, col.names=TRUE, sep=',',quote=FALSE)");
                string output="", error="";
                R.Execute(rCmd.ToString(), false, out output, out error);
                #region classifiy
                // start external application
                using (Process classifyProcess = new Process())
                {

                    classifyProcess.StartInfo.FileName = "java";
                    classifyProcess.StartInfo.Arguments = "-jar \"C:\\Users\\Mohammad\\Desktop\\ATT research\\LinAdapt\\LinAdapt.jar\" predict "+iteration+" 2 " + zipCodeFeature + " \"" + RawPredictionInstancesPath.Replace("\\", "/") + iteration + "\" \"" + Model.ModelDirectory+"\" "+"\""+GlobalModel.ModelDirectory+"\" "+ incidentType;
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
                    throw new Exception("ERROR:  RandomForest failed to classify points. Output and error messages follow:" + Environment.NewLine +
                                        "\tException message:  " + ex.Message + Environment.NewLine +
                                        "\tR output:  " + output + Environment.NewLine +
                                        "\tR orror:  " + error);
                }
                finally
                {
                  
                    try { File.Delete(RawPredictionInstancesPath); }
                    catch { }
                    //try { File.Delete(PredictionsPath); }
                    //catch { }
                }
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation)
        {
            return "No details available for Area Specific predictions.";
        }

        public override Classifier Copy()
        {
            return new AreaSpecificClassifier(RunFeatureSelection, Model );
        }

        internal override void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId)
        {
        }
        public int Iteration
        {
            get { return iteration; }
        }
        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Global Model:" + GlobalModel + Environment.NewLine +
                   indent + "Period:" + period;
        }
    }
}
