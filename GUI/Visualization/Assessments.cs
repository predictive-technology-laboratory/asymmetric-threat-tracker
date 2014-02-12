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

        public int GetIndexOf(Control plot)
        {
            return plots.Controls.IndexOf(plot);
        }
    }
}
