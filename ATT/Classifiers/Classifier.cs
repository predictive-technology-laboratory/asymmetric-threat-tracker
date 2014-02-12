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

        private int _modelId;
        private bool _runFeatureSelection;

        public int ModelId
        {
            get { return _modelId; }
            set { _modelId = value; }
        }

        public IFeatureBasedDCM Model
        {
            get { return DiscreteChoiceModel.Instantiate(_modelId) as IFeatureBasedDCM; }
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
            : this(false, -1)
        {
        }

        protected Classifier(bool runFeatureSelection, int modelId)
        {
            _modelId = modelId;
            _runFeatureSelection = runFeatureSelection;
        }

        public abstract void Initialize();

        public virtual void Consume(FeatureVectorList featureVectors)
        {
            if (featureVectors != null)
            {
                IFeatureBasedDCM model = Model;
                Area trainingArea = model.TrainingArea;
                long timeSliceTicks = model is TimeSliceDCM ? (model as TimeSliceDCM).TimeSliceTicks : -1;

                using (StreamWriter instanceLocationsFile = new StreamWriter(TrainingInstanceLocationsPath, true))
                {
                    foreach (Point point in featureVectors.Select(v => v.DerivedFrom as Point))
                    {
                        long slice = timeSliceTicks > 0 ? point.Time.Ticks / timeSliceTicks : 1;
                        int row = (int)((point.Location.Y - trainingArea.BoundingBox.MinY) / model.PointSpacing);
                        int col = (int)((point.Location.X - trainingArea.BoundingBox.MinX) / model.PointSpacing);
                        instanceLocationsFile.WriteLine(slice + " " + row + " " + col);
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

        public abstract IEnumerable<int> SelectFeatures(Prediction prediction);

        public abstract void Classify(FeatureVectorList featureVectors);

        public virtual string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "Type:  " + GetType() + Environment.NewLine +
                   indent + "Run feature selection:  " + _runFeatureSelection;
        }

        public abstract Classifier Copy();

        internal abstract string GetDetails(Prediction prediction, Dictionary<int, string> attFeatureIdInformation);

        internal abstract void ChangeFeatureIds(Dictionary<int, int> oldNewFeatureId);
    }
}