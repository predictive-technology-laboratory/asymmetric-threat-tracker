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
using System.Threading.Tasks;

namespace PTL.ATT.Models
{
    [Serializable]
    public class FeatureParameterCollection : IEnumerable<Enum>
    {
        Dictionary<Enum, Tuple<string, string>> _parameterValueTip;

        public int Count
        {
            get { return _parameterValueTip.Count; }
        }

        public FeatureParameterCollection()
        {
            _parameterValueTip = new Dictionary<Enum, Tuple<string, string>>();
        }

        public void Add(Enum parameter, string value, string tip)
        {
            _parameterValueTip.Add(parameter, new Tuple<string, string>(value, tip));
        }

        public void SetValue(Enum parameter, string value)
        {
            if (!_parameterValueTip.ContainsKey(parameter))
                throw new KeyNotFoundException("Cannot set missing parameter:  " + parameter);

            _parameterValueTip[parameter] = new Tuple<string, string>(value, _parameterValueTip[parameter].Item2);
        }

        public int GetIntegerValue(Enum parameter)
        {
            return int.Parse(_parameterValueTip[parameter].Item1);
        }

        public float GetFloatValue(Enum parameter)
        {
            return float.Parse(_parameterValueTip[parameter].Item1);
        }

        public TimeSpan GetTimeSpanValue(Enum parameter)
        {
            return TimeSpan.Parse(_parameterValueTip[parameter].Item1);
        }

        public string GetStringValue(Enum parameter)
        {
            return _parameterValueTip[parameter].Item1;
        }

        public string GetTip(Enum parameter)
        {
            return _parameterValueTip[parameter].Item2;
        }

        public bool Contains(Enum parameter)
        {
            return _parameterValueTip.ContainsKey(parameter);
        }

        public IEnumerator<Enum> GetEnumerator()
        {
            return _parameterValueTip.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parameterValueTip.Keys.GetEnumerator();
        }
    }
}
