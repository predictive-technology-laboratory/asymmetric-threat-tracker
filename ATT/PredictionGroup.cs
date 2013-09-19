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
