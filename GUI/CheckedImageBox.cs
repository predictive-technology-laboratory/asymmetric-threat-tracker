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
