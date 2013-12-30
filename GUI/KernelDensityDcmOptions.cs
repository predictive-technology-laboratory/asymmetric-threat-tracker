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
    public partial class KernelDensityDcmOptions : UserControl
    {
        private KernelDensityDCM _kernelDensityDCM;
        private bool _initializing;

        public KernelDensityDCM KernelDensityDCM
        {
            get { return _kernelDensityDCM; }
            set
            {
                _kernelDensityDCM = value;

                if (_kernelDensityDCM != null)
                    RefreshAll();
            }
        }

        public bool Normalize
        {
            get { return normalize.Checked; }
        }

        public KernelDensityDcmOptions()
        {
            _initializing = true;
            InitializeComponent();
            _initializing = false;

            RefreshAll();
        }

        public void RefreshAll()
        {
            if (_initializing)
                return;

            normalize.Checked = false;

            if (_kernelDensityDCM != null)
                normalize.Checked = _kernelDensityDCM.Normalize;
        }
    }
}
