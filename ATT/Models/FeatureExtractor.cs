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

namespace PTL.ATT.Models
{
    public abstract class FeatureExtractor
    {
        public abstract IEnumerable<Feature> GetAvailableFeatures(Area area);

        public abstract void Initialize(DiscreteChoiceModel model, Dictionary<string, string> configurationOptions);

        public abstract IEnumerable<FeatureVectorList> ExtractFeatures(Type callingType, Prediction prediction, FeatureVectorList vectors, bool training);

        public abstract string GetDetails(int indentLevel);

        public abstract Dictionary<int, string> GetDetails(Prediction p);

        public abstract int GetNumFeaturesExtractedFor(Prediction p, Type modelType);
    }
}
