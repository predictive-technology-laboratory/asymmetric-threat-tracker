#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
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
using PTL.ATT.Evaluation;

namespace PTL.ATT
{
    public class PredictionGroup
    {
        private string _name;
        private List<PredictionGroup> _subGroups;
        private Prediction _prediction;
        private Plot _aggregatePlot;

        public Plot AggregatePlot
        {
            get { return _aggregatePlot; }
            set { _aggregatePlot = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public List<PredictionGroup> SubGroups
        {
            get { return _subGroups; }
        }

        public Prediction Prediction
        {
            get { return _prediction; }
        }

        public IEnumerable<Plot> Plots
        {
            get
            {
                if (_aggregatePlot != null)
                    yield return _aggregatePlot;

                if (_prediction != null)
                    foreach (Plot plot in _prediction.AssessmentPlots)
                        yield return plot;

                foreach (PredictionGroup subGroup in _subGroups)
                    foreach (Plot plot in subGroup.Plots)
                        yield return plot;
            }
        }

        public int Count
        {
            get { return (_prediction == null ? 0 : 1) + _subGroups.Sum(g => g.Count); }
        }

        public PredictionGroup(string name)
        {
            _name = name;
            _subGroups = new List<PredictionGroup>();
            _prediction = null;
        }

        public PredictionGroup(string name, Prediction prediction)
            : this(name)
        {
            _prediction = prediction;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
