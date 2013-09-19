using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAIR.MachineLearning;
using LAIR.MachineLearning.ClassifierWrappers.SvmRank;
using LAIR.MachineLearning.FeatureNameTransformation;
using LAIR.MachineLearning.FeatureSpaceRepresentation;
using System.IO;
using System.IO.Compression;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.MachineLearning.ClassifierWrappers;

namespace PTL.ATT.Classifiers
{
    [Serializable]
    public class SvmRank : Classifier
    {
        [NonSerialized]
        private SvmRankClassifier _svmRank;
        private float _c;

        public float C
        {
            get { return _c; }
            set { _c = value; }
        }

        public string CompressedTrainingInstancesPath
        {
            get { return _svmRank.TrainingInstancesPath + ".gz"; }
        }

        public SvmRank()
            : this(0.01f, false, -1)
        {
        }

        public SvmRank(float c, bool runFeatureSelection, int modelId)
            : base(runFeatureSelection, modelId)
        {
            _c = c;
        }

        protected override void Initialize(Prediction prediction)
        {
            base.Initialize(prediction);

            if (_svmRank == null)
            {
                string learnPath = Configuration.ClassifierTypeOptions[GetType()]["learn"];
                string classifyPath = Configuration.ClassifierTypeOptions[GetType()]["classify"];

                _svmRank = new SvmRankClassifier(_c, NumericFeatureNameTransform.AccessMethod.Memory, FeatureSpace.AccessMethod.Memory, true, null, learnPath, classifyPath, null);
            }

            _svmRank.ModelDirectory = prediction.ModelDirectory;
        }

        public override void Consume(FeatureVectorList featureVectors, Prediction prediction)
        {
            base.Consume(featureVectors, prediction);

            if (featureVectors != null && featureVectors.Count > 0)
            {
                if (prediction.IncidentTypes.Count != 1)
                    throw new Exception("SvmRank cannot be used for multi-incident predictions. Select a single incident type.");

                Dictionary<int, Point> idPoint = new Dictionary<int, Point>(featureVectors.Count);
                foreach (Point point in prediction.Points)
                    idPoint.Add(point.Id, point);

                foreach (FeatureVector vector in featureVectors)
                {
                    PostGIS.Point vectorLocation = (vector.DerivedFrom as Point).Location;
                    int count = idPoint.Values.Count(p => p.Location.DistanceTo(vectorLocation) <= prediction.PointSpacing / 2d && p.IncidentType != PointPrediction.NullLabel);
                    vector.DerivedFrom.TrueClass = count + " qid:1";
                }

                _svmRank.ConsumeTrainingVectors(featureVectors);
            }
        }

        protected override void BuildModel()
        {
            int maxCount = File.ReadLines(_svmRank.TrainingInstancesPath).Max(l => int.Parse(l.Substring(0, l.IndexOf(' '))));
            string tempPath = Path.GetTempFileName();
            StreamWriter tempFile = new StreamWriter(tempPath);
            foreach (string line in File.ReadLines(_svmRank.TrainingInstancesPath))
                tempFile.WriteLine((maxCount - int.Parse(line.Substring(0, line.IndexOf(' '))) + 1) + line.Substring(line.IndexOf(' ')));
            tempFile.Close();
            File.Delete(_svmRank.TrainingInstancesPath);
            File.Move(tempPath, _svmRank.TrainingInstancesPath);

            _svmRank.Learn();

            LAIR.IO.File.Compress(_svmRank.TrainingInstancesPath, CompressedTrainingInstancesPath, true);
            File.Delete(_svmRank.TrainingInstancesPath);
        }

        public override IEnumerable<int> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException();
        }

        public override void Classify(FeatureVectorList featureVectors, Prediction prediction)
        {
            base.Classify(featureVectors, prediction);

            if (prediction.IncidentTypes.Count != 1)
                throw new Exception("SvmRank cannot be used for multi-incident predictions. Select a single incident type.");

            string incident = prediction.IncidentTypes.First();

            _svmRank.Classify(featureVectors);

            int maxRank = featureVectors.Max(vector => int.Parse(vector.DerivedFrom.PredictionConfidenceScores.Keys.First())) + 1;

            foreach (FeatureVector vector in featureVectors)
            {
                int rank = int.Parse(vector.DerivedFrom.PredictionConfidenceScores.Keys.First()) + 1;
                float score = (maxRank - rank) / (float)maxRank;
                vector.DerivedFrom.PredictionConfidenceScores.Clear();
                vector.DerivedFrom.PredictionConfidenceScores.Add(incident, score);
            }
        }

        internal override string GetDetails(Prediction prediction, Dictionary<int, string> attFeatureIdInformation)
        {
            return "No details available for SVM Rank classifiers.";
        }

        public override Classifier Copy()
        {
            return new SvmRank(_c, RunFeatureSelection, ModelId);
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "C:  " + _c;
        }

        internal override void ChangeFeatureIds(Prediction prediction, Dictionary<int, int> oldNewFeatureId)
        {
            Dictionary<string, string> oldNameNewName = new Dictionary<string, string>();
            foreach (int oldFeatureId in oldNewFeatureId.Keys)
                oldNameNewName.Add(oldFeatureId.ToString(), oldNewFeatureId[oldFeatureId].ToString());

            NumberedFeatureClassifier.ChangeFeatureNames(prediction.ModelDirectory, oldNameNewName);
        }
    }
}
