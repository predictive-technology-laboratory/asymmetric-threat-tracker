using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace PTL.ATT.GUI.Visualization
{
    public partial class Visualizer : UserControl
    {
        private Prediction _displayedPrediction;
        private IEnumerable<Overlay> _overlays;

        internal Prediction DisplayedPrediction
        {
            get { return _displayedPrediction; }
        }

        internal IEnumerable<Overlay> Overlays
        {
            get { return _overlays; }
        }

        public Visualizer()
        {
            InitializeComponent();
        }

        public virtual void Display(Prediction prediction, IEnumerable<Overlay> overlays)
        {
            Invoke(new Action(Clear));

            _displayedPrediction = prediction;
            _overlays = overlays;
        }

        public virtual void Clear()
        {
            _displayedPrediction = null;
            _overlays = null;
        }
    }
}
