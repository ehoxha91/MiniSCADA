using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScadaOtrila.Guis.Tags
{
    /// <summary>
    /// Interaction logic for TagEditor.xaml
    /// </summary>
    public partial class TagEditor : Window
    {
        private bool isEdit = false;
        public TagEditor(DataOtrila.TagsRow actualSelectedTag, bool _isEdit)
        {
            InitializeComponent();
            cmbType.Items.Add("Analog TAG");
            cmbType.Items.Add("Digital TAG");
            cmbArchive.Items.Add("YES");
            cmbArchive.Items.Add("NO");
            cmbAlarm.Items.Add("YES");
            cmbAlarm.Items.Add("NO");
            LoadData(actualSelectedTag, _isEdit);
        }

        private void LoadData(DataOtrila.TagsRow tagRow, bool _isEdit)
        {
            isEdit = _isEdit;
            if (tagRow != null && isEdit)
            {
                txtName.Text = tagRow.Name;
                txtOPC.Text = tagRow.OpcServer;
                txtMachineName.Text = tagRow.MachineName;
                txtDescription.Text = tagRow.Description;
                txtTAG.Text = tagRow.Tag;
                txtRefreshTime.Text = tagRow.RefreshTime.ToString();
                if (tagRow.Type == 1)
                    cmbType.SelectedValue = "Analog TAG";
                else
                    cmbType.SelectedValue = "Digital TAG";

                if (tagRow.Alarm)
                    cmbAlarm.SelectedValue = "YES";
                else
                    cmbAlarm.SelectedValue = "NO";

                if (tagRow.Archive)
                    cmbArchive.SelectedValue = "YES";
                else
                    cmbArchive.SelectedValue = "NO";

                txtScale.Text = tagRow.Scale.ToString();

                txtID.Text = tagRow.ID.ToString();
                txtUnit.Text = tagRow.Unit;
            }
        }

        private void btnRuaje_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int type;
                bool alarm;
                bool archive;
                if (cmbType.SelectedValue.ToString() == "Analog TAG")
                { type = 1; }
                else { type = 2; }

                if (cmbAlarm.SelectedValue.ToString() == "YES")
                    alarm = true;
                else alarm = false;

                if (cmbArchive.SelectedValue.ToString() == "YES")
                    archive = true;
                else archive = false;

                if (isEdit)
                {
                    new DataOtrilaTableAdapters.TagsTableAdapter().UpdateTag(txtName.Text, txtMachineName.Text, txtOPC.Text, txtDescription.Text, txtTAG.Text, type,
                        Convert.ToInt32(txtRefreshTime.Text), txtUnit.Text, alarm, archive, Convert.ToInt32(txtScale.Text), Convert.ToInt16(txtID.Text));
                }
                else
                {
                (new DataOtrilaTableAdapters.TagsTableAdapter()).Insert(txtName.Text, txtMachineName.Text, txtOPC.Text, txtDescription.Text, txtTAG.Text, type,
                                                                        Convert.ToInt32(txtRefreshTime.Text), txtUnit.Text, alarm, archive,Convert.ToInt32(txtScale.Text));
                        
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
