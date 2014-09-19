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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PTL.ATT.GUI
{
    public partial class DynamicForm : Form
    {
        public enum CloseButtons
        {
            OK,
            OkCancel,
            OkClose,
            YesNo
        }

        private FlowLayoutPanel _mainPanel;
        private Dictionary<string, Func<object>> _valueIdReturn;
        private CloseButtons _buttons;

        public IEnumerable<string> ValueIds
        {
            get { return _valueIdReturn.Keys; }
        }

        public DynamicForm(string title, CloseButtons buttons)
        {
            InitializeComponent();

            Text = title;

            _buttons = buttons;
            _valueIdReturn = new Dictionary<string, Func<object>>();
            _mainPanel = new FlowLayoutPanel();
            _mainPanel.FlowDirection = FlowDirection.TopDown;

            Controls.Add(_mainPanel);

            Load += new EventHandler(DynamicForm_Load);
            Shown += new EventHandler(DynamicForm_Shown);
        }

        private void DynamicForm_Load(object sender, EventArgs args)
        {
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;

            Button firstButton = new Button();
            DialogResult firstButtonResult = System.Windows.Forms.DialogResult.None;
            if (_buttons == CloseButtons.OK || _buttons == CloseButtons.OkCancel || _buttons == CloseButtons.OkClose)
            {
                firstButton.Text = "OK";
                firstButtonResult = System.Windows.Forms.DialogResult.OK;
            }
            else if (_buttons == CloseButtons.YesNo)
            {
                firstButton.Text = "Yes";
                firstButtonResult = System.Windows.Forms.DialogResult.Yes;
            }
            else
                throw new NotImplementedException("Dynamic form first button not defined:  " + _buttons);

            firstButton.Size = firstButton.PreferredSize;
            firstButton.Click += new EventHandler((o, e) =>
                {
                    DialogResult = firstButtonResult;
                    Close();
                });

            buttonPanel.Controls.Add(firstButton);

            Button secondButton = new Button();
            DialogResult secondButtonResult = System.Windows.Forms.DialogResult.None;
            if (_buttons == CloseButtons.OkCancel)
            {
                secondButton.Text = "Cancel";
                secondButtonResult = System.Windows.Forms.DialogResult.Cancel;
            }
            else if (_buttons == CloseButtons.OkClose)
            {
                secondButton.Text = "Close";
                secondButtonResult = System.Windows.Forms.DialogResult.None;
            }
            else if (_buttons == CloseButtons.YesNo)
            {
                secondButton.Text = "No";
                secondButtonResult = System.Windows.Forms.DialogResult.No;
            }
            else
                secondButton = null;

            if (secondButton != null)
            {
                secondButton.Size = secondButton.PreferredSize;
                secondButton.Click += new EventHandler((o, e) =>
                    {
                        DialogResult = secondButtonResult;
                        Close();
                    });

                buttonPanel.Controls.Add(secondButton);
            }

            buttonPanel.Size = buttonPanel.PreferredSize;

            _mainPanel.Controls.Add(buttonPanel);
            _mainPanel.Size = _mainPanel.PreferredSize;

            Size = PreferredSize;
            Width = Math.Max(Width, TextRenderer.MeasureText(Text, Font).Width + 50);
        }

        private void DynamicForm_Shown(object sender, EventArgs args)
        {
            CenterToScreen();
            TopMost = true;
        }

        public void AddTextBox(string label, string text, int widthInCharacters, string valueId, char passwordChar = '\0', bool onlyUseTextWidth = false, bool addFileBrowsingButtons = false, string initialBrowsingDirectory = null, string fileFilter = null, Action<object, EventArgs> textChanged = null, string toolTipText = null)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = ContentAlignment.MiddleRight;
            l.Size = l.PreferredSize;
            l.Margin = new System.Windows.Forms.Padding(5);

            if (toolTipText != null)
                toolTip.SetToolTip(l, toolTipText);

            TextBox tb = new TextBox();
            tb.Name = valueId;
            tb.Text = text;
            tb.PasswordChar = passwordChar;
            tb.Size = widthInCharacters == -1 ? new Size(TextRenderer.MeasureText(tb.Text == "" ? "".PadLeft(50, ' ') : tb.Text, tb.Font).Width, tb.PreferredHeight) : new Size(TextRenderer.MeasureText("".PadLeft(widthInCharacters), tb.Font).Width, tb.PreferredHeight);
            tb.KeyDown += new KeyEventHandler((o, args) =>
                {
                    if (args.KeyCode == Keys.Enter)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                        args.Handled = true;
                    }
                });

            if (textChanged != null)
                tb.TextChanged += new EventHandler(textChanged);

            if (onlyUseTextWidth)
                tb.Text = "";

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(tb);

            if (addFileBrowsingButtons)
            {
                Button fileBrowse = new Button();
                fileBrowse.Text = "File...";
                fileBrowse.Click += (o, e) =>
                    {
                        tb.Text = LAIR.IO.File.PromptForOpenPath("Select file...", initialBrowsingDirectory, fileFilter);
                    };

                p.Controls.Add(fileBrowse);

                Button directoryBrowse = new Button();
                directoryBrowse.Text = "Directory...";
                directoryBrowse.Click += (o, e) =>
                    {
                        tb.Text = LAIR.IO.Directory.PromptForDirectory("Select directory...", initialBrowsingDirectory);
                    };

                p.Controls.Add(directoryBrowse);
            }

            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, new Func<string>(() => tb.Text));
        }

        public void AddNumericUpdown(string label, decimal value, int decimalPlaces, decimal minimum, decimal maximum, decimal increment, string valueId, string toolTipText = null)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            if (toolTipText != null)
                toolTip.SetToolTip(l, toolTipText);

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

        public void AddCheckBox(string label, ContentAlignment checkAlign, bool isChecked, string valueId, string toolTipText = null)
        {
            CheckBox cb = new CheckBox();
            cb.Name = valueId;
            cb.Text = label;
            cb.CheckAlign = checkAlign;
            cb.Checked = isChecked;
            cb.Size = cb.PreferredSize;

            if (toolTipText != null)
                toolTip.SetToolTip(cb, toolTipText);

            _mainPanel.Controls.Add(cb);
            _valueIdReturn.Add(valueId, new Func<object>(() => cb.Checked));
        }

        public void AddDropDown(string label, Array values, object selected, string valueId, bool sorted, Action<object, EventArgs> selectedValueChanged = null, string toolTipText = null)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            if (toolTipText != null)
                toolTip.SetToolTip(l, toolTipText);

            ComboBox cb = new ComboBox();
            cb.Name = valueId;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.Sorted = sorted;

            if (values != null)
            {
                foreach (object o in values)
                    cb.Items.Add(o);

                if (values.Length > 0)
                    cb.Width = cb.Items.Cast<object>().Max(o => TextRenderer.MeasureText(o.ToString(), cb.Font).Width) + 50;
            }

            if (selectedValueChanged != null)
                cb.SelectedValueChanged += new EventHandler(selectedValueChanged);

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

        public void AddListBox(string label, Array values, object selected, SelectionMode selectionMode, string valueId, bool sorted, string toolTipText = null)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            if (toolTipText != null)
                toolTip.SetToolTip(l, toolTipText);

            ListBox lb = new ListBox();
            lb.Name = valueId;
            lb.SelectionMode = selectionMode;
            lb.Sorted = sorted;

            if (values != null)
            {
                foreach (object o in values)
                    lb.Items.Add(o);

                if (values.Length > 0)
                    lb.Size = new System.Drawing.Size(lb.Items.Cast<object>().Max(o => TextRenderer.MeasureText(o.ToString(), lb.Font).Width + 30), lb.PreferredHeight);
            }

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

        public void AddPictureBox(Image image, string toolTipText = null)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.Size = pictureBox.PreferredSize;

            if (toolTipText != null)
                toolTip.SetToolTip(pictureBox, toolTipText);

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(pictureBox);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
        }

        public void AddControl(string label, Control control, Func<object> returnValueFunction, string valueId, string toolTipText = null)
        {
            Label l = new Label();
            l.Text = label;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Size = new System.Drawing.Size(l.PreferredSize.Width, l.Height);

            if (toolTipText != null)
                toolTip.SetToolTip(l, toolTipText);

            control.Name = valueId;
            control.Size = control.PreferredSize;

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.FlowDirection = FlowDirection.LeftToRight;
            p.Controls.Add(l);
            p.Controls.Add(control);
            p.Size = p.PreferredSize;

            _mainPanel.Controls.Add(p);
            _valueIdReturn.Add(valueId, returnValueFunction);
        }

        public T GetControl<T>(string valueId) where T : Control
        {
            Control[] controls = Controls.Find(valueId, true);
            if (controls.Length == 0)
                return null;
            else if (controls.Length == 1)
                return (T)controls[0];
            else
                throw new Exception("Multiple controls with ID of \"" + valueId + "\" were found.");
        }

        public T GetValue<T>(string valueId)
        {
            try { return (T)_valueIdReturn[valueId](); }
            catch (InvalidCastException ex) { throw new InvalidCastException("Invalid cast in Parameterize form:  " + ex.Message); }
        }
    }
}
