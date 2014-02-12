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

namespace PTL.ATT.GUI
{
    public partial class CheckedImageBox : UserControl
    {
        private Plot _plot;

        public bool Checked
        {
            get { return checkBox.Checked; }
        }

        public Plot Plot
        {
            get { return _plot; }
        }

        public CheckedImageBox(Plot plot, bool check)
        {
            InitializeComponent();

            _plot = plot;

            image.Image = _plot.Image;
            checkBox.Checked = check;
            Size = new System.Drawing.Size(_plot.Image.Width, _plot.Image.Height + checkBox.Height + 10);
        }

        private void image_Click(object sender, EventArgs e)
        {
            checkBox.Checked = !checkBox.Checked;
        }

        private void CheckedImageBox_Click(object sender, EventArgs e)
        {

            checkBox.Checked = !checkBox.Checked;
        }
    }
}
