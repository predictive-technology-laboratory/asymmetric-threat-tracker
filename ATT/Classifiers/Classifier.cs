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
using System.Reflection;
using System.IO;
using LAIR.Extensions;
using LAIR.MachineLearning;
using LAIR.ResourceAPIs.PostgreSQL;
using PTL.ATT.Models;
using System.IO.Compression;

namespace PTL.ATT.Classifiers
{
    [Serializable]
    public abstract class Classifier
    {
        public static IEnumerable<Classifier> Available
        {
            get { return Assembly.GetAssembly(typeof(Classifier)).GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Classifier))).Select(t => Activator.CreateInstance(t)).Cast<Classifier>(); }
        }

        private FeatureBasedDCM _model;
        private bool _runFeatureSelection;
        protected int _numFeaturesInEachVector;

        public FeatureBasedDCM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public bool RunFeatureSelection
        {
            get { return _runFeatureSelection; }
            set { _runFeatureSelection = value; }
        }

        private string TrainingInstanceLocationsPath
        {
            get { return Path.Combine(Model.ModelDirectory, "training_instance_locations"); }
        }

        public string CompressedTrainingInstanceLocationsPath
        {
            get { return TrainingInstanceLocationsPath + ".gz"; }
        }

        protected Classifier()
            : this(false, null)
        {
        }

        protected Classifier(bool runFeatureSelection, FeatureBasedDCM model)
        {
            _model = model;
            _runFeatureSelection = runFeatureSelection;
            _numFeaturesInEachVector = -1;
        }

        public abstract void Initialize();

        public virtual void Consume(FeatureVectorList featureVectors)
        {
            if (featureVectors != null)
            {
                long timeSliceTicks = _model is TimeSliceDCM ? (_model as TimeSliceDCM).TimeSliceTicks : -1;

                using (StreamWriter instanceLocationsFile = new StreamWriter(TrainingInstanceLocationsPath, true))
                {
                    foreach (FeatureVector featureVector in featureVectors)
                    {
                        Point point = featureVector.DerivedFrom as Point;
                        long slice = timeSliceTicks > 0 ? point.Time.Ticks / timeSliceTicks : 1;
                        int row = (int)((point.Location.Y - _model.TrainingArea.BoundingBox.MinY) / _model.TrainingPointSpacing);
                        int col = (int)((point.Location.X - _model.TrainingArea.BoundingBox.MinX) / _model.TrainingPointSpacing);
                        instanceLocationsFile.WriteLine(slice + " " + row + " " + col);

                        if (_numFeaturesInEachVector == -1)
                            _numFeaturesInEachVector = featureVector.Count;
                        else if (_numFeaturesInEachVector != featureVector.Count)
                            throw new Exception("Feature vectors do not contain the same number of features. This probably indicates missing features during the feature extraction process.");
                    }

                    instanceLocationsFile.Close();
                }
            }
        }

        public void Train(FeatureVectorList featureVectors)
        {
            Consume(featureVectors);
            BuildModel();

            LAIR.IO.File.Compress(TrainingInstanceLocationsPath, CompressedTrainingInstanceLocationsPath, true);
            System.IO.File.Delete(TrainingInstanceLocationsPath);
        }

        public void Train()
        {
            Train(null);
        }

        protected abstract void BuildModel();

        public abstract IEnumerable<string> SelectFeatures(Prediction prediction);

        public virtual void Classify(FeatureVectorList featureVectors)
        {
            if (featureVectors != null)
                foreach (FeatureVector featureVector in featureVectors)
                    if (featureVector.Count != _numFeaturesInEachVector)
                        throw new Exception("Expected " + _numFeaturesInEachVector + " features in each vector, but saw " + featureVector.Count + ".");
        }

        public virtual string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "Type:  " + GetType() + Environment.NewLine +
                   indent + "Run feature selection:  " + _runFeatureSelection;
        }

        public abstract Classifier Copy();

        internal abstract string GetDetails(Prediction prediction, Dictionary<string, string> attFeatureIdInformation);

        internal abstract void ChangeFeatureIds(Dictionary<string, string> oldNewFeatureId);
    }
}