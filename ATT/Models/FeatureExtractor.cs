using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAIR.MachineLearning;

namespace PTL.ATT.Models
{
    public abstract class FeatureExtractor
    {
        public abstract IEnumerable<Feature> AvailableFeatures { get; }

        public abstract void Initialize(DiscreteChoiceModel model, Dictionary<string, string> configurationOptions);

        public abstract IEnumerable<FeatureVectorList> ExtractFeatures(Type callingType, Prediction prediction, FeatureVectorList vectors, bool training, int idOfSpatiotemporallyIdenticalPrediction);

        public abstract string GetDetails(int indentLevel);

        public abstract Dictionary<int, string> GetDetails(Prediction p);

        public abstract int GetNumFeaturesExtractedFor(Prediction p, Type modelType);
    }
}
