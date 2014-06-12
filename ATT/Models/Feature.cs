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
using LAIR.ResourceAPIs.PostgreSQL;
using PTL.ATT.Models;
using LAIR.Collections.Generic;
using LAIR.Extensions;

namespace PTL.ATT.Models
{
    [Serializable]
    public class Feature : IComparable<Feature>
    {
        private static int _featureNumber;

        static Feature()
        {
            _featureNumber = 0;
        }

        private string _id;
        private Type _enumType;
        private Enum _enumValue;
        private string _description;
        private string _trainingResourceId;
        private string _predictionResourceId;
        private Dictionary<string, string> _parameterValue;

        public string Id
        {
            get { return _id; }
        }

        public Type EnumType
        {
            get { return _enumType; }
        }

        public Enum EnumValue
        {
            get { return _enumValue; }
        }

        public string Description
        {
            get { return _description; }
        }

        public string TrainingResourceId
        {
            get { return _trainingResourceId; }
        }

        public string PredictionResourceId
        {
            get { return _predictionResourceId; }
            set { _predictionResourceId = value; }
        }

        public Dictionary<string, string> ParameterValue
        {
            get { return _parameterValue; }
            set { _parameterValue = value; }
        }

        public string RemapKey
        {
            get { return _enumType + "-" + _enumValue + "-" + _trainingResourceId; }
        }

        public Feature(Type enumType, Enum enumValue, string trainingResourceId, string predictionResourceId, string description, Dictionary<string, string> parameterValue)
        {
            _id = _featureNumber++.ToString();
            _enumType = enumType;
            _enumValue = enumValue;
            _description = description;
            _trainingResourceId = trainingResourceId == null ? "" : trainingResourceId;
            _predictionResourceId = predictionResourceId == null ? "" : predictionResourceId;
            _parameterValue = parameterValue;

            if (_parameterValue == null)
                _parameterValue = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return _description + (_predictionResourceId == _trainingResourceId ? "" : " --> " + _predictionResourceId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Feature))
                return false;

            Feature f = obj as Feature;

            return _enumType.FullName == f.EnumType.FullName && _enumValue.ToString() == f.EnumValue.ToString() && _trainingResourceId == f.TrainingResourceId && _predictionResourceId == f.PredictionResourceId;
        }

        public override int GetHashCode()
        {
            return (_enumType + "-" + _enumValue + "-" + _trainingResourceId + "-" + _predictionResourceId).GetHashCode();
        }

        public int CompareTo(Feature other)
        {
            int cmp = _enumType.ToString().CompareTo(other.EnumType.ToString());

            if (cmp == 0)
                cmp = _enumValue.ToString().CompareTo(other.EnumValue.ToString());

            if (cmp == 0 && _trainingResourceId != null && other.TrainingResourceId != null)
            {
                int r1, r2;
                if (int.TryParse(_trainingResourceId, out r1) && int.TryParse(other.TrainingResourceId, out r2))
                    cmp = r1.CompareTo(r2);
                else
                    cmp = _trainingResourceId.CompareTo(other.TrainingResourceId);
            }

            if (cmp == 0 && _predictionResourceId != null && other.PredictionResourceId != null)
            {
                int r1, r2;
                if (int.TryParse(_predictionResourceId, out r1) && int.TryParse(other.PredictionResourceId, out r2))
                    cmp = r1.CompareTo(r2);
                else
                    cmp = _predictionResourceId.CompareTo(other.PredictionResourceId);
            }

            return cmp;
        }
    }
}
