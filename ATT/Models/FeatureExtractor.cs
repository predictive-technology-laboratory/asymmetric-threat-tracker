#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
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
        public abstract IEnumerable<Feature> AvailableFeatures { get; }

        public abstract void Initialize(DiscreteChoiceModel model, Dictionary<string, string> configurationOptions);

        public abstract IEnumerable<FeatureVectorList> ExtractFeatures(Type callingType, Prediction prediction, FeatureVectorList vectors, bool training, int idOfSpatiotemporallyIdenticalPrediction);

        public abstract string GetDetails(int indentLevel);

        public abstract Dictionary<int, string> GetDetails(Prediction p);

        public abstract int GetNumFeaturesExtractedFor(Prediction p, Type modelType);
    }
}
