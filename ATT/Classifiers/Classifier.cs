#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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
            get { return Assembly.GetAssembly(typeof(Classifier)).GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Classifier)) && Configuration.ClassifierTypeOptions.ContainsKey(t)).Select(t => Activator.CreateInstance(t)).Cast<Classifier>(); }
        }

        private int _modelId;
        private bool _runFeatureSelection;
        private string _modelDirectory;

        public int ModelId
        {
            get { return _modelId; }
            set { _modelId = value; }
        }

        public DiscreteChoiceModel Model
        {
            get { return DiscreteChoiceModel.Instantiate(_modelId); }
        }

        public bool RunFeatureSelection
        {
            get { return _runFeatureSelection; }
            set { _runFeatureSelection = value; }
        }

        private string TrainingInstanceLocationsPath
        {
            get { return Path.Combine(_modelDirectory, "training_instance_locations"); }
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

        public void Train(FeatureVectorList featureVectors, Prediction prediction)
        {
            Initialize(prediction);
            Consume(featureVectors, prediction);
            BuildModel();

            LAIR.IO.File.Compress(TrainingInstanceLocationsPath, CompressedTrainingInstanceLocationsPath, true);
            System.IO.File.Delete(TrainingInstanceLocationsPath);
        }

        public void Train(Prediction prediction)
        {
            Train(null, prediction);
        }

        protected virtual void Initialize(Prediction prediction)
        {
            _modelDirectory = prediction.ModelDirectory;
        }

        public virtual void Consume(FeatureVectorList featureVectors, Prediction prediction)
        {
            Initialize(prediction);

            if (featureVectors != null)
            {
                DiscreteChoiceModel model = Model;
                Area trainingArea = model.TrainingArea;
                long timeSliceTicks = model is TimeSliceDCM ? (model as TimeSliceDCM).TimeSliceTicks : -1;

                StreamWriter instanceLocationsFile = new StreamWriter(TrainingInstanceLocationsPath, true);
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

        protected abstract void BuildModel();

        public virtual void Classify(FeatureVectorList featureVectors, Prediction prediction)
        {
            Initialize(prediction);
        }

        public virtual string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "Type:  " + GetType() + Environment.NewLine +
                   indent + "Run feature selection:  " + _runFeatureSelection;
        }

        public virtual IEnumerable<int> SelectFeatures(Prediction prediction)
        {
            Initialize(prediction);
            return null;
        }

        public abstract Classifier Copy();

        internal abstract string GetDetails(Prediction prediction, Dictionary<int, string> attFeatureIdInformation);

        internal abstract void ChangeFeatureIds(Prediction prediction, Dictionary<int, int> oldNewFeatureId);
    }
}