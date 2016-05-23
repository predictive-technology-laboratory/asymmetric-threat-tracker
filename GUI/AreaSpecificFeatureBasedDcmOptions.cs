using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using PTL.ATT.Models;

namespace PTL.ATT.GUI
{
    public partial class AreaSpecificFeatureBasedDcmOptions : UserControl
    {
        public AreaSpecificFeatureBasedDcmOptions()
        {
            _initializing = true;
            InitializeComponent();
            zipCodes = new List<int>();
            BuildzipcodesList(zipCodes, zipcodeShapefile);
            _ComboBoxZipcodeShapeFiles.Items.AddRange(Shapefile.GetAll().ToArray());
            _initializing = false;
        }
        private AreaSpecificFeatureBasedDCM _areaSpecificDCM;
        private bool _initializing;
        private List<int> zipCodes;
        private string zipcodeShapefile;
        private bool _buildingCheckboxItems;
        public AreaSpecificFeatureBasedDCM AreaSpecificFeatureBasedDCM
        {
            get { return _areaSpecificDCM; }
            set
            {
                _areaSpecificDCM = value;

                if (_areaSpecificDCM != null)
                    RefreshAll();
            }
        }

         
        public List<int>  ZipCodes
        {
            get { return zipCodes; }
        }
       

        public void RefreshAll()
        {
            if (_initializing)
                return;

            if(_areaSpecificDCM != null)
            {
                zipCodes = _areaSpecificDCM.AreasZipCodes;
                zipcodeShapefile = _areaSpecificDCM.ZipcodeShapeFile;
                 
                    int selectedItemIndex = -1;
                    for (int i = 0; i < _ComboBoxZipcodeShapeFiles.Items.Count; i++)
                    {
                        if (((Shapefile)_ComboBoxZipcodeShapeFiles.Items[i]).GeometryTable == zipcodeShapefile)
                        {
                            selectedItemIndex = i;
                            break;
                        }
                    }
                    if (selectedItemIndex!=-1)
                        _ComboBoxZipcodeShapeFiles.SelectedIndex = selectedItemIndex;
                
                BuildzipcodesList(_areaSpecificDCM.AreasZipCodes, zipcodeShapefile);
            }
        }

        public string ValidateInput()
        {
            return "";
        }

        internal void CommitValues(AreaSpecificFeatureBasedDCM model)
        {
            model.AreasZipCodes = zipCodes;
            model.ZipcodeShapeFile = zipcodeShapefile;
        }
        private void BuildzipcodesList(List<int> zipCodes,string shapefile)
        {
            if (_initializing)
                return;
            _CheckedListBoxZipCodes.Items.Clear();
            NpgsqlCommand cmd = DB.Connection.NewCommand("select distinct zip from " + shapefile + " order by zip");
            _buildingCheckboxItems = true;
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int zip = Convert.ToInt32(reader["zip"]);
                _CheckedListBoxZipCodes.Items.Add(zip, zipCodes.Contains(zip));
            }
            reader.Close();
            _buildingCheckboxItems = false;
            DB.Connection.Return(cmd.Connection);
        }

        
        private void _CheckedListBoxZipCodes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_buildingCheckboxItems&&!ischeckingall)
            {
                zipCodes = new List<int>();
                
                for (int i = 0; i < _CheckedListBoxZipCodes.Items.Count; i++)
                    if ((e.Index != i && _CheckedListBoxZipCodes.GetItemChecked(i)) || (e.Index == i && e.NewValue == CheckState.Checked))
                        zipCodes.Add(Convert.ToInt32(_CheckedListBoxZipCodes.Items[i]));
            }

        }
        bool checkall = true;
        bool ischeckingall = false;
        private void _btnCheckall_Click(object sender, EventArgs e)
        {
            ischeckingall = true;
            zipCodes = new List<int>();
            for (int i = 0; i < _CheckedListBoxZipCodes.Items.Count; i++)
            { _CheckedListBoxZipCodes.SetItemCheckState(i, checkall ? CheckState.Checked : CheckState.Unchecked); if (checkall)zipCodes.Add(Convert.ToInt32(_CheckedListBoxZipCodes.Items[i])); }
            checkall = !checkall;
            ischeckingall = false;
        }

        private void _ButtonLoadZipcodes_Click(object sender, EventArgs e)
        {
            if (zipcodeShapefile != "")
            {
                _CheckedListBoxZipCodes.Items.Clear();
                NpgsqlCommand cmd = DB.Connection.NewCommand("select distinct zip from " + zipcodeShapefile + " order by zip");
                _buildingCheckboxItems = true;
                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int zip = Convert.ToInt32(reader["zip"]);
                    _CheckedListBoxZipCodes.Items.Add(zip, zipCodes.Contains(zip));
                }
                reader.Close();
                _buildingCheckboxItems = false;
                DB.Connection.Return(cmd.Connection);
            }
        }

        private void _ComboBoxZipcodeShapeFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing)
                return;
            if (_ComboBoxZipcodeShapeFiles.SelectedIndex == -1)
                zipcodeShapefile = "";
            else
                zipcodeShapefile = ((Shapefile)_ComboBoxZipcodeShapeFiles.SelectedItem).GeometryTable;
        }

      
        

       
    }
}
