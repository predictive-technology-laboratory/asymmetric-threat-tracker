using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Evaluation;

namespace PTL.ATT.GUI.Visualization
{
    public partial class Assessments : UserControl
    {
        public Assessments()
        {
            InitializeComponent();
        }

        public void ClearPlots()
        {
            plots.Controls.Clear();
        }

        public void AddPlot(Control plot)
        {
            plots.Controls.Add(plot);
        }
    }
}
