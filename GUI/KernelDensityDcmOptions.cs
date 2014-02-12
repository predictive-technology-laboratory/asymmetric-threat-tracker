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

        public int TrainingSampleSize
        {
            get { return (int)trainingSampleSize.Value; }
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

            trainingSampleSize.Value = 500;
            normalize.Checked = false;

            if (_kernelDensityDCM != null)
            {
                trainingSampleSize.Value = _kernelDensityDCM.TrainingSampleSize;
                normalize.Checked = _kernelDensityDCM.Normalize;
            }
        }

        public string ValidateInput()
        {
            return "";
        }
    }
}
