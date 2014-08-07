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
using Npgsql;
using LAIR.MachineLearning;
using LAIR.MachineLearning.ClassifierWrappers.LibLinear;
using LAIR.MachineLearning.FeatureNameTransformation;
using LAIR.MachineLearning.FeatureSpaceRepresentation;
using LAIR.IO;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.MachineLearning.FeatureSelection.FeatureSelector;
using System.IO;
using LAIR.MachineLearning.FeatureSelection.FeatureSelector.Scorers;
using LAIR.MachineLearning.FeatureSelection.FeatureSelector.Wrappers;
using PTL.ATT.Models;
using LAIR.Extensions;
using System.Xml.Serialization;
using System.IO.Compression;
using LAIR.MachineLearning.ClassifierWrappers;
using LAIR.MachineLearning.FeatureSelection.FeatureSelector.FeatureFilters;

namespace PTL.ATT.Classifiers
{
    [Serializable]
    public class LibLinear : Classifier
    {
        public enum PositiveClassWeighting
        {
            NegativePositiveRatio,
            None
        }

        [NonSerialized]
        private LibLinearClassifier _libLinear;
        private PositiveClassWeighting _positiveClassWeighting;

        public PositiveClassWeighting Weighting
        {
            get { return _positiveClassWeighting; }
            set { _positiveClassWeighting = value; }
        }

        public string CompressedTrainingInstancesPath
        {
            get { return _libLinear.TrainingInstancesPath + ".gz"; }
        }

        public LibLinear()
            : this(false, null, PositiveClassWeighting.None)
        {
        }

        public LibLinear(bool runFeatureSelection, FeatureBasedDCM model, PositiveClassWeighting positiveClassWeighting)
            : base(runFeatureSelection, model)
        {
            _positiveClassWeighting = positiveClassWeighting;
        }

        public override void Initialize()
        {
            LibLinearClassifier.Solver solver = (LibLinearClassifier.Solver)Enum.Parse(typeof(LibLinearClassifier.Solver), Configuration.ClassifierTypeOptions[GetType()]["solver"]);
            string trainPath = Configuration.ClassifierTypeOptions[GetType()]["train"];
            string predictPath = Configuration.ClassifierTypeOptions[GetType()]["predict"];

            _libLinear = new LibLinearClassifier(solver, NumericFeatureNameTransform.AccessMethod.Memory, FeatureSpace.AccessMethod.Memory, true, Model.ModelDirectory, trainPath, predictPath, null, float.MinValue);
            _libLinear.OutputProbabilities = true;
        }

        public override void Consume(FeatureVectorList featureVectors)
        {
            base.Consume(featureVectors);

            if (featureVectors != null)
                _libLinear.ConsumeTrainingVectors(featureVectors);
        }

        private Dictionary<int, float> GetPerClassWeights(StreamReader trainingInstancesReader)
        {
            Dictionary<int, int> classCount = new Dictionary<int, int>();
            string line;
            while (trainingInstancesReader.TryReadLine(out line))
            {
                int firstSpace = line.IndexOf(' ');
                if (firstSpace == -1)
                    firstSpace = line.Length;

                int classNum = int.Parse(line.Substring(0, firstSpace));
                classCount.EnsureContainsKey(classNum, typeof(int));
                classCount[classNum]++;
            }

            Dictionary<int, float> classWeight = new Dictionary<int, float>();
            int total = classCount.Values.Sum();
            foreach (int classNum in classCount.Keys)
                if (_libLinear.GetUnmappedLabel(classNum.ToString()) != PointPrediction.NullLabel)
                    classWeight.Add(classNum, (total - classCount[classNum]) / (float)classCount[classNum]);

            return classWeight;
        }

        protected override void BuildModel()
        {
            if (_positiveClassWeighting == PositiveClassWeighting.NegativePositiveRatio)
                using (StreamReader trainingInstancesFile = new StreamReader(_libLinear.TrainingInstancesPath))
                    _libLinear.PerClassWeights = GetPerClassWeights(trainingInstancesFile);
            else if (_positiveClassWeighting != PositiveClassWeighting.None)
                throw new NotImplementedException("Unknown LibLinear weighting scheme:  " + _positiveClassWeighting);

            _libLinear.Learn();

            LAIR.IO.File.Compress(_libLinear.TrainingInstancesPath, CompressedTrainingInstancesPath, true);
            System.IO.File.Delete(_libLinear.TrainingInstancesPath);
        }

        public override IEnumerable<string> SelectFeatures(Prediction prediction)
        {
            _libLinear.LoadClassificationModelFiles();

            string logPath = Path.Combine(Model.ModelDirectory, "feature_selection_log.txt");
            System.IO.File.Delete(logPath);

            int nullClass = -1;
            foreach (string unmappedLabel in _libLinear.Labels)
                if (unmappedLabel == PointPrediction.NullLabel)
                    if (nullClass == -1)
                        nullClass = int.Parse(_libLinear.GetMappedLabel(unmappedLabel));
                    else
                        throw new Exception("Multiple null classes in label map");

            if (nullClass == -1)
                throw new Exception("Failed to find null class");

            string featureSelectionTrainingPath = Path.GetTempFileName();

            using (FileStream compressedTrainingInstancesFile = new FileStream(CompressedTrainingInstancesPath, FileMode.Open, FileAccess.Read))
            using (GZipStream compressedTrainingInstancesGzip = new GZipStream(compressedTrainingInstancesFile, CompressionMode.Decompress))
            using (StreamReader trainingInstancesFile = new StreamReader(compressedTrainingInstancesGzip))
            using (FileStream compressedTrainingInstanceLocationsFile = new FileStream(CompressedTrainingInstanceLocationsPath, FileMode.Open, FileAccess.Read))
            using (GZipStream compressedTrainingInstanceLocationsGzip = new GZipStream(compressedTrainingInstanceLocationsFile, CompressionMode.Decompress))
            using (StreamReader trainingInstanceLocationsFile = new StreamReader(compressedTrainingInstanceLocationsGzip))
            using (StreamWriter featureSelectionTrainingFile = new StreamWriter(featureSelectionTrainingPath))
            {
                try
                {
                    string instance;
                    while ((instance = trainingInstancesFile.ReadLine()) != null)
                    {
                        string location = trainingInstanceLocationsFile.ReadLine();
                        if (location == null)
                            throw new Exception("Missing location for training instance");

                        featureSelectionTrainingFile.WriteLine(instance + " # " + location);
                    }

                    if ((instance = trainingInstanceLocationsFile.ReadToEnd()) != null && (instance = instance.Trim()) != "")
                        throw new Exception("Extra training instance locations:  " + instance);

                    trainingInstancesFile.Close();
                    trainingInstanceLocationsFile.Close();
                    featureSelectionTrainingFile.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to read training instances:  " + ex.Message);
                }
            }

            string groupNamePath = Path.GetTempFileName();
            Dictionary<string, string> groupNameFeatureId = new Dictionary<string, string>();
            using (StreamWriter groupNameFile = new StreamWriter(groupNamePath))
            {
                foreach (PTL.ATT.Models.Feature feature in Model.Features)
                {
                    int featureNumber;
                    if (_libLinear.TryGetFeatureNumber(feature.Id, out featureNumber))
                    {
                        string groupName = feature.ToString().ReplacePunctuation(" ").RemoveRepeatedWhitespace().Replace(' ', '_').Trim('_');
                        groupNameFile.WriteLine(featureNumber + " " + groupName);
                        groupNameFeatureId.Add(groupName, feature.Id);
                    }
                }
                groupNameFile.Close();
            }

            Options featureSelectionOptions = new Options();
            featureSelectionOptions.Add(FeatureSelector.Option.ExitOnErrorAction, FeatureSelector.ExitOnErrorAction.ThrowException.ToString());
            featureSelectionOptions.Add(FeatureSelector.Option.FeatureFilters, typeof(ZeroVectorFeatureFilter).FullName + "," + typeof(CosineSimilarityFeatureFilter).FullName);
            featureSelectionOptions.Add(FeatureSelector.Option.FloatingSelection, false.ToString());
            featureSelectionOptions.Add(FeatureSelector.Option.GroupNamePath, groupNamePath);
            featureSelectionOptions.Add(FeatureSelector.Option.LogPath, logPath);
            featureSelectionOptions.Add(FeatureSelector.Option.MaxThreads, Configuration.ProcessorCount.ToString());
            featureSelectionOptions.Add(FeatureSelector.Option.PerformanceIncreaseRequirement, float.Epsilon.ToString());
            featureSelectionOptions.Add(FeatureSelector.Option.Scorer, typeof(SurveillancePlotScorer).FullName);
            featureSelectionOptions.Add(FeatureSelector.Option.TrainingInstancesInMemory, true.ToString());
            featureSelectionOptions.Add(FeatureSelector.Option.TrainingInstancesPath, featureSelectionTrainingPath);
            featureSelectionOptions.Add(FeatureSelector.Option.Verbosity, FeatureSelector.Verbosity.Debug.ToString());
            featureSelectionOptions.Add(SurveillancePlotScorer.Option.IgnoredSurveillanceClasses, nullClass.ToString());
            featureSelectionOptions.Add(CommonWrapper.Option.ClassifyExePath, Configuration.ClassifierTypeOptions[GetType()]["predict"]);
            featureSelectionOptions.Add(CommonWrapper.Option.TrainExePath, Configuration.ClassifierTypeOptions[GetType()]["train"]);
            featureSelectionOptions.Add(LibLinearWrapper.Option.IgnoredProbabilisticClasses, nullClass.ToString());
            featureSelectionOptions.Add(LibLinearWrapper.Option.SumInstanceProbabilities, true.ToString());
            featureSelectionOptions.Add(CrossFoldValidator.Option.RandomizeInstanceBlocks, true.ToString());
            featureSelectionOptions.Add(CrossFoldValidator.Option.InstanceBlockRandomizationSeed, (498734983).ToString());
            featureSelectionOptions.Add(CrossFoldValidator.Option.NumFolds, (2).ToString());
            featureSelectionOptions.Add(CosineSimilarityFeatureFilter.Option.Threshold, (0.98).ToString());

            if (_positiveClassWeighting == PositiveClassWeighting.NegativePositiveRatio)
            {
                using (FileStream compressedTrainingInstancesFile = new FileStream(CompressedTrainingInstancesPath, FileMode.Open, FileAccess.Read))
                using (GZipStream compressedTrainingInstancesGzip = new GZipStream(compressedTrainingInstancesFile, CompressionMode.Decompress))
                using (StreamReader compressedTrainingInstancesReader = new StreamReader(compressedTrainingInstancesGzip))
                {
                    Dictionary<int, float> classWeight = GetPerClassWeights(compressedTrainingInstancesReader);
                    foreach (int classNum in classWeight.Keys)
                        featureSelectionOptions.Add((LibLinearWrapper.Option)Enum.Parse(typeof(LibLinearWrapper.Option), "W" + classNum), classWeight[classNum].ToString());
                }
            }
            else if (_positiveClassWeighting != PositiveClassWeighting.None)
                throw new Exception("Unrecognized positive class weighting scheme:  " + _positiveClassWeighting);

            FeatureSelector.Run(_libLinear, featureSelectionOptions);

            float score;
            Dictionary<string, Tuple<int, float>> featureRankContribution;
            FeatureSelector.GetResults(logPath, out score, out featureRankContribution);

            System.IO.File.Delete(featureSelectionTrainingPath);
            System.IO.File.Delete(groupNamePath);

            return featureRankContribution.Keys.Select(groupName => groupNameFeatureId[groupName]);
        }

        public override void Classify(FeatureVectorList featureVectors)
        {
            _libLinear.Classify(featureVectors);
        }

        internal override string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation)
        {
            StringBuilder report = new StringBuilder("Details for model created by prediction \"" + prediction.Name + "\"" + Environment.NewLine);
            Dictionary<int, Dictionary<int, double>> classFeatureWeight = LibLinearClassifier.GetFeatureWeights(Path.Combine(Model.ModelDirectory, LibLinearClassifier.ModelFileName));
            LabelMap labelMap = new LabelMap(Path.Combine(Model.ModelDirectory, LibLinearClassifier.LabelMapFileName));
            MemoryNumericFeatureNameTransform featureNameTransform = new MemoryNumericFeatureNameTransform(Path.Combine(Model.ModelDirectory, LibLinearClassifier.FeatureNameTransformFileName));

            Dictionary<int, string> liblinearFeatureNumberAttFeatureId = new Dictionary<int, string>();
            foreach (string attFeatureId in featureNameTransform)
            {
                // nominal features in the ATT have IDs in the transform that include the nominal feature value - trim this off to recover the original ID
                int dashIndex = attFeatureId.IndexOf('-');
                if (dashIndex >= 0)
                    liblinearFeatureNumberAttFeatureId.Add(featureNameTransform.GetFeatureNumber(attFeatureId), attFeatureId.Substring(0, dashIndex));
                else
                    liblinearFeatureNumberAttFeatureId.Add(featureNameTransform.GetFeatureNumber(attFeatureId), attFeatureId);
            }

            Dictionary<string, string> attFeatureIdDesc = new Dictionary<string, string>();
            foreach (PTL.ATT.Models.Feature f in Model.Features)
                attFeatureIdDesc.Add(f.Id, "Feature \"" + f.Description + "\"");

            foreach (int classNumber in classFeatureWeight.Keys.OrderBy(i => i))
            {
                report.AppendLine("\tClass \"" + labelMap.GetUnmappedLabel(classNumber.ToString()) + "\"");

                int maxFeatureNameWidth = classFeatureWeight[classNumber].Keys.Max(f => attFeatureIdDesc[liblinearFeatureNumberAttFeatureId[f]].Length);
                foreach (int liblinearFeatureNumber in classFeatureWeight[classNumber].Keys.OrderBy(f => -Math.Abs(classFeatureWeight[classNumber][f])))
                {
                    string desc = attFeatureIdDesc[liblinearFeatureNumberAttFeatureId[liblinearFeatureNumber]];
                    double weight = classFeatureWeight[classNumber][liblinearFeatureNumber];
                    string attFeatureId = liblinearFeatureNumberAttFeatureId[liblinearFeatureNumber];
                    string information = (attFeatureIdInformation == null || !attFeatureIdInformation.ContainsKey(attFeatureId) ? "" : Environment.NewLine +
                                         "\t\t\tInformation:  " + attFeatureIdInformation[attFeatureId]);

                    report.AppendLine(string.Format("\t\t{0,-" + maxFeatureNameWidth + "}: weight = {1:0.00}", desc, weight) + information + Environment.NewLine);
                }

                report.AppendLine();
            }

            return report.ToString();
        }

        public override Classifier Copy()
        {
            return new LibLinear(RunFeatureSelection, Model, _positiveClassWeighting);
        }

        internal override void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId)
        {
            Dictionary<string, string> oldNameNewName = new Dictionary<string, string>();
            foreach (string oldFeatureId in oldNewFeatureId.Keys)
                oldNameNewName.Add(oldFeatureId, oldNewFeatureId[oldFeatureId]);

            NumberedFeatureClassifier.ChangeFeatureNames(Model.ModelDirectory, oldNameNewName);
        }

        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "Positive class weighting:  " + _positiveClassWeighting;
        }
    }
}
