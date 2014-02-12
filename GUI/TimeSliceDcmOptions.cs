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

        public TimeSliceDcmOptions()
        {
            _initializing = true;
            InitializeComponent();
            _initializing = false;
        }

        public void RefreshAll()
        {
            if (_initializing)
                return;

            if(_timeSliceDCM != null)
            {
                timeSliceHours.Value = _timeSliceDCM.TimeSliceHours;
                timeSlicesPerPeriod.Value = _timeSliceDCM.PeriodTimeSlices;
            }
        }

        public string ValidateInput()
        {
            return "";
        }
    }
}
