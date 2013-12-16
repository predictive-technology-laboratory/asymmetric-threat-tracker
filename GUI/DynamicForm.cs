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
    public partial class DynamicForm : Form
    {
        private FlowLayoutPanel _mainPanel;
        private Dictionary<string, Func<object>> _valueIdReturn;
        private MessageBoxButtons _buttons;

        public DynamicForm(string title = "", MessageBoxButtons buttons = MessageBoxButtons.OKCancel)
        {
            InitializeComponent();

            Text = title;
            _buttons = buttons;
            if (_buttons != MessageBoxButtons.OK && _buttons != MessageBoxButtons.OKCancel)
                throw new NotImplementedException("Only OK and Cancel buttons are implemented");

            _valueIdReturn = new Dictionary<string, Func<object>>();

            _mainPanel = new FlowLayoutPanel();
            _mainPanel.FlowDirection = FlowDirection.TopDown;

            Shown += new EventHandler(DynamicForm_Shown);
        }

        private void DynamicForm_Shown(object sender, EventArgs args)
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
            Width = Math.Max(Width, TextRenderer.MeasureText(Text, Font).Width + 50);
            TopMost = true;
        }

        public void AddTextBox(string label, string text, int widthInCharacters, string valueId, char passwordChar = '\0', bool onlyUseTextWidth = false)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = ContentAlignment.MiddleRight;
            l.Size = l.PreferredSize;
            l.Margin = new System.Windows.Forms.Padding(5);

            TextBox tb = new TextBox();
            tb.Name = valueId;
            tb.Text = text;
            tb.PasswordChar = passwordChar;
            tb.Size = widthInCharacters == -1 ? tb.PreferredSize : new Size(TextRenderer.MeasureText("".PadLeft(widthInCharacters), Font).Width, tb.PreferredSize.Height);
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

        public void AddNumericUpdown(string label, decimal value, int decimalPlaces, decimal minimum, decimal maximum, decimal increment, string valueId)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            NumericUpDown ud = new NumericUpDown();
            ud.Name = valueId;
            ud.DecimalPlaces = decimalPlaces;
            ud.Minimum = minimum;
            ud.Maximum = maximum;
            ud.Increment = increment;
            ud.Value = value;
            ud.Width = TextRenderer.MeasureText(value.ToString(), ud.Font).Width + 50;

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(ud);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<object>(() => ud.Value));
        }

        public void AddCheckBox(string label, ContentAlignment checkAlign, bool isChecked, string valueId)
        {
            CheckBox cb = new CheckBox();
            cb.Text = label;
            cb.CheckAlign = checkAlign;
            cb.Checked = isChecked;
            cb.Size = cb.PreferredSize;

            _mainPanel.Controls.Add(cb);
            _valueIdReturn.Add(valueId, new Func<object>(() => cb.Checked));
        }

        public void AddDropDown(string label, Array values, object selected, string valueId)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            ComboBox cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (object o in values)
                cb.Items.Add(o);

            cb.Width = cb.Items.Cast<object>().Max(o => TextRenderer.MeasureText(o.ToString(), cb.Font).Width) + 50;

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(cb);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<object>(() => cb.SelectedItem));

            if (selected != null)
                cb.SelectedItem = selected;
            else if (cb.Items.Count > 0)
                cb.SelectedIndex = 0;
        }

        public void AddListBox(string label, Array values, object selected, SelectionMode selectionMode, string valueId)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            ListBox lb = new ListBox();
            lb.SelectionMode = selectionMode;
            foreach (object o in values)
                lb.Items.Add(o);

            lb.Size = lb.PreferredSize;

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(lb);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<object>(() => lb.SelectedItems));

            if (selected != null)
                lb.SelectedItem = selected;
            else if (lb.Items.Count > 0)
                lb.SelectedIndex = 0;
        }

        public T GetValue<T>(string valueId)
        {
            T castValue;
            try { castValue = (T)_valueIdReturn[valueId](); }
            catch (InvalidCastException ex) { throw new InvalidCastException("Invalid cast in Parameterize form:  " + ex.Message); }

            if (castValue == null)
                throw new NullReferenceException("ParameterizeForm value function returned null");

            return castValue;
        }
    }
}
