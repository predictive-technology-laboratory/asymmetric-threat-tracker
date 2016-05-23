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
using System.Threading.Tasks;
using System.Windows.Forms;
using PTL.ATT.Models;

namespace PTL.ATT.GUI
{
    public partial class TimeSliceDcmOptions : UserControl
    {
        private TimeSliceDCM _timeSliceDCM;
        private bool _initializing;
        private int slicesToRun;
        private bool _buildingCheckboxItems;
        public TimeSliceDCM TimeSliceDCM
        {
            get { return _timeSliceDCM; }
            set
            {
                _timeSliceDCM = value;

                if (_timeSliceDCM != null)
                    RefreshAll();
            }
        }

        public int TimeSliceHours
        {
            get { return (int)timeSliceHours.Value; }
        }

        public int TimeSlicesPerPeriod
        {
            get { return (int)timeSlicesPerPeriod.Value; }
        }
        public int SlicesToRun
        {
            get { return slicesToRun; }
        }
        public TimeSliceDcmOptions()
        {
            _initializing = true;
            InitializeComponent();
            BuildSlicesToRun(slicesToRun);
            _initializing = false;

        }

        public void RefreshAll()
        {
            if (_initializing)
                return;

            if (_timeSliceDCM != null)
            {
                timeSliceHours.Value = _timeSliceDCM.TimeSliceHours;
                timeSlicesPerPeriod.Value = _timeSliceDCM.PeriodTimeSlices;
                BuildSlicesToRun(_timeSliceDCM.SlicesToRun);
            }
        }

        public string ValidateInput()
        {
            return "";
        }

        internal void CommitValues(TimeSliceDCM model)
        {
            model.TimeSliceHours = TimeSliceHours;
            model.PeriodTimeSlices = TimeSlicesPerPeriod;
            model.SlicesToRun = SlicesToRun;
        }
        private void BuildSlicesToRun(int slicesToRun)
        {
            _buildingCheckboxItems = true;
            _CheckedListBoxSlicesToRun.Items.Clear();
            DateTime TimeInterval = new DateTime(2000, 1, 1, 0, 0, 0);
            int Mask = (int)Math.Pow(2, (double)timeSlicesPerPeriod.Value - 1);

            for (int i = 0; i < timeSlicesPerPeriod.Value; i++)
            {
                _CheckedListBoxSlicesToRun.Items.Add(String.Format("{0} to {1}", TimeInterval.ToShortTimeString(), TimeInterval.AddHours(TimeSliceHours).AddTicks(-1).ToShortTimeString()), slicesToRun == 0 || (slicesToRun & Mask) != 0);
                TimeInterval = TimeInterval.AddHours(Convert.ToInt32(timeSliceHours.Value));
                Mask = Mask >> 1;
            }
            _buildingCheckboxItems = false;
        }
        private void timeSliceHours_ValueChanged(object sender, EventArgs e)
        {
            BuildSlicesToRun(slicesToRun);
        }



        private void _CheckedListBoxSlicesToRun_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_buildingCheckboxItems)
            {
                slicesToRun = (int)(Math.Pow(2, 4) - 1);
                for (int i = 0; i < _CheckedListBoxSlicesToRun.Items.Count; i++)
                    if ((e.Index != i && !_CheckedListBoxSlicesToRun.GetItemChecked(i)) || (e.Index == i && e.NewValue != CheckState.Checked))
                        slicesToRun -= (int)Math.Pow(2, _CheckedListBoxSlicesToRun.Items.Count - (i + 1));
            }
        }
    }
}
