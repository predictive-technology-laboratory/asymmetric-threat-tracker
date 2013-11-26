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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PTL.ATT.GUI
{
    public partial class ParameterizeForm : Form
    {
        private FlowLayoutPanel _mainPanel;
        private Dictionary<string, Func<object>> _valueIdReturn;
        private MessageBoxButtons _buttons;

        public ParameterizeForm(string title = "", MessageBoxButtons buttons = MessageBoxButtons.OKCancel)
        {
            InitializeComponent();

            Text = title;
            _buttons = buttons;
            if(_buttons != MessageBoxButtons.OK && _buttons != MessageBoxButtons.OKCancel)
                throw new NotImplementedException("Only OK and Cancel buttons are implemented");

            _valueIdReturn = new Dictionary<string, Func<object>>();

            _mainPanel = new FlowLayoutPanel();
            _mainPanel.FlowDirection = FlowDirection.TopDown;

            Shown += new EventHandler(ParameterizationForm_Shown);
        }

        private void ParameterizationForm_Shown(object sender, EventArgs args)
        {
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;

            if (_buttons == MessageBoxButtons.OK || _buttons == MessageBoxButtons.OKCancel)
            {
                Button okBtn = new Button();
                okBtn.Text = "OK";
                okBtn.Size = okBtn.PreferredSize;
                okBtn.Click += new EventHandler((o, e) =>
                    {
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                        Close();
                    });

                buttonPanel.Controls.Add(okBtn);
            }

            if (_buttons == MessageBoxButtons.OKCancel)
            {
                Button cancelBtn = new Button();
                cancelBtn.Text = "Cancel";
                cancelBtn.Size = cancelBtn.PreferredSize;
                cancelBtn.Click += new EventHandler((o, e) =>
                    {
                        DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        Close();
                    });

                buttonPanel.Controls.Add(cancelBtn);
            }

            buttonPanel.Size = buttonPanel.PreferredSize;

            _mainPanel.Controls.Add(buttonPanel);
            _mainPanel.Size = _mainPanel.PreferredSize;

            Controls.Add(_mainPanel);
            Size = PreferredSize;
        }

        public void AddTextBox(string label, string text, string valueId, char passwordChar = '\0', bool onlyUseTextWidth = false)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            TextBox tb = new TextBox();
            tb.Name = valueId;
            tb.Text = text;
            tb.PasswordChar = passwordChar;
            tb.Size = new Size(tb.PreferredSize.Width, tb.PreferredSize.Height);
            tb.KeyDown += new KeyEventHandler((o, args) =>
                {
                    if (args.KeyCode == Keys.Enter)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                        args.Handled = true;
                    }
                });

            if (onlyUseTextWidth)
                tb.Text = "";

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(tb);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<string>(() => tb.Text));
        }

        internal void AddCheckBox(string label, RightToLeft rightToLeft, bool isChecked, string valueId)
        {
            CheckBox cb = new CheckBox();
            cb.Text = label;
            cb.RightToLeft = rightToLeft;
            cb.TextAlign = rightToLeft == System.Windows.Forms.RightToLeft.Yes ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            cb.Checked = isChecked;
            cb.Size = cb.PreferredSize;

            _mainPanel.Controls.Add(cb);
            _valueIdReturn.Add(valueId, new Func<object>(() => cb.Checked));
        }

        internal void AddDropDown(string label, Array values, object selected, string valueId)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            ListBox lb = new ListBox();
            foreach (object o in values)
                lb.Items.Add(o);

            lb.Size = lb.PreferredSize;

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(lb);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<object>(() => lb.SelectedItem));

            if (selected != null)
                lb.SetSelected(lb.Items.IndexOf(selected), true);
        }

        public object GetValue(string valueId)
        {
            object value = _valueIdReturn[valueId]();
            if(value == null)
                throw new NullReferenceException("Parameterize return value function returned null");

            return value;
        }
    }
}
