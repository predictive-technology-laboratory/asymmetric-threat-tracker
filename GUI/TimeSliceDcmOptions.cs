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
