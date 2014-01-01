﻿#region copyright
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
using System.Threading;
using PTL.ATT.Models;
using PTL.ATT.Smoothers;

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

        public int PointSpacing
        {
            get { return (int)pointSpacing.Value; }
        }

        public DateTime TrainingStart
        {
            get { return trainingStart.Value; }
        }

        public DateTime TrainingEnd
        {
            get { return trainingEnd.Value; }
        }

        public string[] IncidentTypes
        {
            get { return incidentTypes.SelectedItems.Cast<string>().ToArray(); }
        }

        public Smoother[] Smoothers
        {
            get { return smoothers.SelectedItems.Cast<Smoother>().ToArray(); }
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
                if (TrainingArea != null && IncidentTypes.Length > 0)
                    numIncidents = Incident.Count(trainingStart.Value, trainingEnd.Value, TrainingArea, IncidentTypes);

                toolTip.SetToolTip(incidentTypes, numIncidents + " total incidents selected");
            }
        }
        #endregion

        #region refreshing
        public void RefreshAll()
        {
            if (_initializing)
                return;

            _setIncidentsToolTip = true;

            modelName.Text = "";
            trainingAreas.Items.Clear();
            pointSpacing.Value = 200;
            trainingStart.Value = DateTime.Today.Add(new TimeSpan(-7, 0, 0, 0));
            trainingEnd.Value = trainingStart.Value.Add(new TimeSpan(6, 23, 59, 59));
            incidentTypes.Items.Clear();
            smoothers.Populate(_discreteChoiceModel);

            try
            {
                RefreshAreas();
                RefreshIncidentTypes();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Failed to refresh information from database:  " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            if (_discreteChoiceModel != null)
            {
                modelName.Text = _discreteChoiceModel.Name;
                trainingAreas.SelectedItem = _discreteChoiceModel.TrainingArea;
                pointSpacing.Value = _discreteChoiceModel.PointSpacing;
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

            incidentTypes.Items.Clear();
            if (TrainingArea != null)
                foreach (string incidentType in Incident.GetUniqueTypes(trainingStart.Value, trainingEnd.Value, TrainingArea))
                    incidentTypes.Items.Add(incidentType);

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

            if (IncidentTypes.Length == 0)
                errors.AppendLine("One or more incident types must be selected.");

            return errors.ToString();
        }
    }
}
