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
using System.Threading;
using PTL.ATT.Models;
using PTL.ATT.Smoothers;
using LAIR.Collections.Generic;

namespace PTL.ATT.GUI
{
    public partial class DiscreteChoiceModelOptions : UserControl
    {
        private DiscreteChoiceModel _discreteChoiceModel;
        private bool _setIncidentsToolTip;
        private bool _initializing;

        public string ModelName
        {
            get { return modelName.Text.Trim(); }
        }

        public Area TrainingArea
        {
            get { return trainingAreas.SelectedItem as Area; }
        }

        public DateTime TrainingStart
        {
            get { return trainingStart.Value; }
        }

        public DateTime TrainingEnd
        {
            get { return trainingEnd.Value; }
        }

        public Set<string> IncidentTypes
        {
            get { return new Set<string>(incidentTypes.SelectedItems.Cast<string>().ToArray()); }
        }

        public List<Smoother> Smoothers
        {
            get { return smoothers.SelectedItems.Cast<Smoother>().ToList(); }
        }

        public DiscreteChoiceModel DiscreteChoiceModel
        {
            get { return _discreteChoiceModel; }
            set
            {
                _discreteChoiceModel = value;

                if (_discreteChoiceModel != null)
                    RefreshAll();
            }
        }

        public DiscreteChoiceModelOptions()
        {
            _initializing = true;
            InitializeComponent();
            _initializing = false;

            if (Configuration.MonoAddIncidentRefresh)
            {
                ToolStripMenuItem refresh = new ToolStripMenuItem("Refresh");
                refresh.Click += new EventHandler((o, args) => RefreshIncidentTypes());
                incidentTypesMenu.Items.Add(refresh);
            }

            RefreshAll();
        }

        #region training area
        private void trainingAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TrainingArea != null)
                toolTip.SetToolTip(trainingAreas, TrainingArea.GetDetails(0));

            RefreshIncidentTypes();
        }
        #endregion

        #region training dates
        private void trainingStart_ValueChanged(object sender, EventArgs e)
        {
            if (trainingEnd.Value < trainingStart.Value)
                trainingEnd.Value = trainingStart.Value + new TimeSpan(23, 59, 59);
        }

        private void trainingStart_CloseUp(object sender, EventArgs e)
        {
            RefreshIncidentTypes();
        }

        private void trainingEnd_ValueChanged(object sender, EventArgs e)
        {
            if (trainingEnd.Value < trainingStart.Value)
                trainingStart.Value = trainingEnd.Value - new TimeSpan(23, 59, 59);
        }

        private void trainingEnd_CloseUp(object sender, EventArgs e)
        {
            RefreshIncidentTypes();
        }
        #endregion

        #region incidents
        public void selectAllIncidentTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _setIncidentsToolTip = false;

            for (int i = 0; i < incidentTypes.Items.Count; ++i)
                incidentTypes.SetSelected(i, true);

            _setIncidentsToolTip = true;

            SetIncidentsToolTip();
        }

        private void incidentTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetIncidentsToolTip();
        }

        private void SetIncidentsToolTip()
        {
            if (_setIncidentsToolTip)
            {
                int numIncidents = 0;
                if (TrainingArea != null && IncidentTypes.Count > 0)
                    numIncidents = Incident.Count(trainingStart.Value, trainingEnd.Value, TrainingArea, IncidentTypes.ToArray());

                toolTip.SetToolTip(incidentTypes, numIncidents + " total incidents selected");
            }
        }
        #endregion

        #region refreshing
        public void RefreshAll()
        {
            if (_initializing || DB.Connection == null) // DB.Connection is null when the designer is using the form
                return;

            _setIncidentsToolTip = true;

            RefreshAreas();
            smoothers.Populate(_discreteChoiceModel);

            if (_discreteChoiceModel == null)
            {
                modelName.Text = "";
                Incident firstIncident = Incident.GetFirst(TrainingArea);
                Incident lastIncident = Incident.GetLast(TrainingArea);
                trainingStart.Value = firstIncident == null ? DateTime.Today.Add(new TimeSpan(-7, 0, 0, 0)) : new DateTime(firstIncident.Time.Year, firstIncident.Time.Month, firstIncident.Time.Day, 0, 0, 0);
                trainingEnd.Value = lastIncident == null ? trainingStart.Value.Add(new TimeSpan(6, 23, 59, 59)) : new DateTime(lastIncident.Time.Year, lastIncident.Time.Month, lastIncident.Time.Day, 23, 59, 59);
                RefreshIncidentTypes();
            }
            else
            {
                modelName.Text = _discreteChoiceModel.Name;
                trainingAreas.SelectedItem = _discreteChoiceModel.TrainingArea;
                trainingStart.Value = _discreteChoiceModel.TrainingStart;
                trainingEnd.Value = _discreteChoiceModel.TrainingEnd;
                RefreshIncidentTypes();

                foreach (string incidentType in _discreteChoiceModel.IncidentTypes)
                {
                    int index = incidentTypes.Items.IndexOf(incidentType);
                    if (index >= 0)
                        incidentTypes.SetSelected(index, true);
                }
            }

            SetIncidentsToolTip();
        }

        public void RefreshAreas()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshAreas));
                return;
            }

            toolTip.SetToolTip(trainingAreas, null);

            trainingAreas.Items.Clear();
            foreach (Area area in Area.GetAll())
                trainingAreas.Items.Add(area);

            if (_discreteChoiceModel != null)
                trainingAreas.SelectedItem = _discreteChoiceModel.TrainingArea;
            else if (trainingAreas.Items.Count > 0)
                trainingAreas.SelectedIndex = 0;
        }

        public void RefreshIncidentTypes()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshIncidentTypes));
                return;
            }

            _setIncidentsToolTip = false;

            List<string> selections = incidentTypes.SelectedItems.Cast<string>().ToList();

            incidentTypes.Items.Clear();
            if (TrainingArea != null)
                foreach (string incidentType in Incident.GetUniqueTypes(trainingStart.Value, trainingEnd.Value, TrainingArea))
                    incidentTypes.Items.Add(incidentType);

            foreach (string selection in selections)
            {
                int index = incidentTypes.Items.IndexOf(selection);
                if (index >= 0)
                    incidentTypes.SetSelected(index, true);
            }

            _setIncidentsToolTip = true;

            SetIncidentsToolTip();
        }
        #endregion

        public string ValidateInput()
        {
            StringBuilder errors = new StringBuilder();

            if (ModelName == "")
                errors.AppendLine("Model name cannot be empty.");

            if (TrainingArea == null)
                errors.AppendLine("A training area must be selected.");

            if (IncidentTypes.Count == 0)
                errors.AppendLine("One or more incident types must be selected.");

            return errors.ToString();
        }

        internal void CommitValues(DiscreteChoiceModel model)
        {
            model.Name = ModelName;
            model.TrainingArea = TrainingArea;
            model.TrainingStart = TrainingStart;
            model.TrainingEnd = TrainingEnd;
            model.IncidentTypes = IncidentTypes;
            model.Smoothers = Smoothers;
        }
    }
}
