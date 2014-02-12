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

namespace PTL.ATT.GUI.Visualization
{
    public partial class ColoredCheckBox : UserControl
    {
        public event EventHandler CheckBoxCheckedChanged;
        public event EventHandler CheckBoxCheckStateChanged;
        public event EventHandler LabelClicked;

        public CheckBox CheckBox
        {
            get { return checkBox; }
        }

        public Label Label
        {
            get { return label; }
        }

        public override string Text
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public bool Checked
        {
            get { return checkBox.Checked; }
        }

        public CheckState CheckState
        {
            get { return checkBox.CheckState; }
        }

        public ColoredCheckBox(bool threeState, CheckState checkState, string text, Color backColor)
        {
            InitializeComponent();

            if (checkState == CheckState.Indeterminate && !threeState)
                throw new Exception("Cannot set check state to Indeterminate for two-state checkboxes.");

            checkBox.ThreeState = threeState;
            checkBox.CheckState = checkState;
            label.Text = text;
            label.BackColor = backColor;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxCheckedChanged != null)
                CheckBoxCheckedChanged(this, e);
        }

        private void checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (CheckBoxCheckStateChanged != null)
                CheckBoxCheckStateChanged(this, e);
        }

        private void label_Click(object sender, EventArgs e)
        {
            if (LabelClicked != null)
                LabelClicked(this, e);
        }

        private void label_BackColorChanged(object sender, EventArgs e)
        {
            if (label.BackColor.GetBrightness() >= 0.5f)
                ForeColor = Color.Black;
            else
                ForeColor = Color.White;
        }
    }
}
