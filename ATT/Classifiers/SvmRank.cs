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
using LAIR.MachineLearning.ClassifierWrappers.SvmRank;
using LAIR.MachineLearning.FeatureNameTransformation;
using LAIR.MachineLearning.FeatureSpaceRepresentation;
using System.IO;
using System.IO.Compression;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.MachineLearning.ClassifierWrappers;
using PTL.ATT.Models;

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
            : this(false, null, 0.01f)
        {
        }

        public SvmRank(bool runFeatureSelection, FeatureBasedDCM model, float c)
            : base(runFeatureSelection, model)
        {
            _c = c;
        }

        public override void Initialize()
        {
            string learnPath = Configuration.ClassifierTypeOptions[GetType()]["learn"];
            string classifyPath = Configuration.ClassifierTypeOptions[GetType()]["classify"];
            _svmRank = new SvmRankClassifier(_c, NumericFeatureNameTransform.AccessMethod.Memory, FeatureSpace.AccessMethod.Memory, true, Model.ModelDirectory, learnPath, classifyPath, null);
        }

        public override void Consume(FeatureVectorList featureVectors)
        {
            base.Consume(featureVectors);

            if (featureVectors != null && featureVectors.Count > 0)
            {
                if (Model.IncidentTypes.Count != 1)
                    throw new Exception("SvmRank cannot be used for multi-incident predictions. Select a single incident type.");

                Dictionary<int, Point> idPoint = new Dictionary<int, Point>(featureVectors.Count);
                foreach (Point point in featureVectors.Select(vector => vector.DerivedFrom as Point))
                    idPoint.Add(point.Id, point);

                foreach (FeatureVector vector in featureVectors)
                {
                    Point point = vector.DerivedFrom as Point;
                    if (point == null)
                        throw new NullReferenceException("Expected Point object in DerivedFrom");

                    PostGIS.Point vectorLocation = point.Location;
                    int count = idPoint.Values.Count(p => p.Location.DistanceTo(vectorLocation) <= Model.TrainingPointSpacing / 2d && p.IncidentType != PointPrediction.NullLabel);
                    vector.DerivedFrom.TrueClass = count + " qid:1";
                }

                _svmRank.ConsumeTrainingVectors(featureVectors);
            }
        }

        protected override void BuildModel()
        {
            int maxCount = File.ReadLines(_svmRank.TrainingInstancesPath).Max(l => int.Parse(l.Substring(0, l.IndexOf(' '))));
            string tempPath = Path.GetTempFileName();
            using (StreamWriter tempFile = new StreamWriter(tempPath))
            {
                foreach (string line in File.ReadLines(_svmRank.TrainingInstancesPath))
                    tempFile.WriteLine((maxCount - int.Parse(line.Substring(0, line.IndexOf(' '))) + 1) + line.Substring(line.IndexOf(' ')));
                tempFile.Close();
            }

            File.Delete(_svmRank.TrainingInstancesPath);
            File.Move(tempPath, _svmRank.TrainingInstancesPath);

            _svmRank.Learn();

            LAIR.IO.File.Compress(_svmRank.TrainingInstancesPath, CompressedTrainingInstancesPath, true);
            File.Delete(_svmRank.TrainingInstancesPath);
        }

        public override IEnumerable<string> SelectFeatures(Prediction prediction)
        {
            throw new NotImplementedException("Feature selection is not implemented for SVM Rank classifiers.");
        }

        public override void Classify(FeatureVectorList featureVectors)
        {
            base.Classify(featureVectors);

            if (Model.IncidentTypes.Count != 1)
                throw new Exception("SvmRank cannot be used for multi-incident predictions. Select a single incident type.");

            string incident = Model.IncidentTypes.First();

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

        internal override string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation)
        {
            return "No details available for SVM Rank classifiers.";
        }

        public override Classifier Copy()
        {
            return new SvmRank(RunFeatureSelection, Model, _c);
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "C:  " + _c;
        }

        internal override void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId)
        {
            Dictionary<string, string> oldNameNewName = new Dictionary<string, string>();
            foreach (string oldFeatureId in oldNewFeatureId.Keys)
                oldNameNewName.Add(oldFeatureId.ToString(), oldNewFeatureId[oldFeatureId].ToString());

            NumberedFeatureClassifier.ChangeFeatureNames(Model.ModelDirectory, oldNameNewName);
        }
    }
}
