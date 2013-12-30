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
    }
}
