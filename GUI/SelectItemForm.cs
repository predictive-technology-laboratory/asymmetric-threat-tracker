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
    public partial class SelectItemForm<T> : Form where T : class
    {
        public T SelectedItem
        {
            get { return itemList.SelectedItem as T; }
        }

        public T[] SelectedItems
        {
            get { return itemList.SelectedItems.Cast<T>().ToArray(); }
        }

        public SelectItemForm(T[] items, string text, SelectionMode selectionMode)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("Must pass items from which to select");
                
            InitializeComponent();

            foreach (T item in items)
                itemList.Items.Add(item);

            itemList.SelectedIndex = 0;

            Text = text;
            itemList.SelectionMode = selectionMode;
            Size = PreferredSize;
            DialogResult = DialogResult.Cancel;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
