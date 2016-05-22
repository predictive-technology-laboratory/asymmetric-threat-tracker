using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 
using PTL.ATT.Models;
namespace PTL.ATT.GUI
{
    public partial class AreaSpecificFeatureBasedDcmForm : Form
    {
        private AreaSpecificFeatureBasedDCM  _resultingModel;

        public AreaSpecificFeatureBasedDCM ResultingModel
        {
            get { return _resultingModel; }
        }

        public AreaSpecificFeatureBasedDcmForm()
        {
            InitializeComponent();
            discreteChoiceModelOptions.trainingAreas.SelectedValueChanged += (o, e) =>
            {
                 featureBasedDcmOptions.TrainingArea = discreteChoiceModelOptions.TrainingArea;
            };

            featureBasedDcmOptions.GetFeatures = new Func<Area, List<Feature>>(a => AreaSpecificFeatureBasedDCM.GetAvailableFeatures(a).ToList());

            discreteChoiceModelOptions.RefreshAreas();
        }
       



          public AreaSpecificFeatureBasedDcmForm(AreaSpecificFeatureBasedDCM current)
            : this()
        {
            discreteChoiceModelOptions.DiscreteChoiceModel = featureBasedDcmOptions.FeatureBasedDCM = areaSpecificFeatureBasedDcmOptions.AreaSpecificFeatureBasedDCM = current;
        }
        private void oKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errors = discreteChoiceModelOptions.ValidateInput() + featureBasedDcmOptions.ValidateInput() + areaSpecificFeatureBasedDcmOptions.ValidateInput();
            if (errors != "")
            {
                MessageBox.Show(errors);
                return;
            }

            _resultingModel = areaSpecificFeatureBasedDcmOptions.AreaSpecificFeatureBasedDCM;
            if (_resultingModel == null)
                _resultingModel = new AreaSpecificFeatureBasedDCM();

            discreteChoiceModelOptions.CommitValues(_resultingModel);
            featureBasedDcmOptions.CommitValues(_resultingModel);
            areaSpecificFeatureBasedDcmOptions.CommitValues(_resultingModel);

            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
