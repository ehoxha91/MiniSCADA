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

namespace ScadaOtrila.Guis.Oraret
{
    /// <summary>
    /// Interaction logic for OrariDitaConfig.xaml
    /// </summary>
    public partial class OrariDitaConfig : Window
    {
        public OrariDitaConfig()
        {
            InitializeComponent();
        }
        private bool isEdit = false;
        private int id_update;
        public OrariDitaConfig(int _id)
        {
            InitializeComponent();
            isEdit = true;
            id_update = _id;
            Task.Factory.StartNew(() => LoadData(_id));
        }

        private void LoadData(int id)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                DataOtrila dataOtrila = new DataOtrila();
                (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).FillDataByID(dataOtrila.OrariDite,id);
                foreach (DataOtrila.OrariDiteRow dite in dataOtrila.OrariDite.Rows)
                {
                    txtOraF.Text = dite.OraFillimit.ToString();
                    txtMinF.Text = dite.MinutaFillimit.ToString();
                    txtOraM.Text = dite.OraMbarimit.ToString();
                    txtMinM.Text = dite.MinutaMbarimit.ToString();
                    cmbSezona.SelectedValue = dite.Sezona;
                    txtAHUTemp.Text = dite.AHU_Temp.ToString();
                    txtAHUAir.Text = dite.AHU_Air.ToString();
                    txtAHUHum.Text = dite.AHU_Humid.ToString();
                    txtAHURecycle.Text = dite.AHU_Recycle.ToString();
                    txtTempSalla1.Text = dite.Salla_1_Temp.ToString();
                    txtPresSalla1.Text = dite.Salla_1_Pressure.ToString();
                    txtLagSalla1.Text = dite.Salla_1_Humid.ToString();
                    txtTempSalla2.Text =dite.Salla_2_Temp.ToString();
                    txtPresSalla2.Text = dite.Salla_2_Pressure.ToString(); 
                    txtLagSalla2.Text = dite.Salla_2_Humid.ToString();
                    txtTempSalla3.Text = dite.Salla_3_Temp.ToString();
                    txtPresSalla3.Text = dite.Salla_3_Pressure.ToString();
                    txtLagSalla3.Text = dite.Salla_3_Humid.ToString();
                }
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string emri = Convert.ToString(txtEmri.Text);
                int orafillimit = Convert.ToInt32(txtOraF.Text);
                int minfillimit = Convert.ToInt32(txtMinF.Text);
                int orambarimit = Convert.ToInt32(txtOraM.Text);
                int minmbarimit = Convert.ToInt32(txtMinM.Text);
                string sezona = Convert.ToString(cmbSezona.SelectedValue);
                double ahu_temp = Convert.ToDouble(txtAHUTemp.Text);
                int ahu_air_input = Convert.ToInt32(txtAHUAir.Text);
                int ahu_humid = Convert.ToInt32(txtAHUHum.Text);
                int ahu_recycle = Convert.ToInt32(txtAHURecycle.Text);
                double salla1_temp = Convert.ToDouble(txtTempSalla1.Text);
                int salla1_press = Convert.ToInt32(txtPresSalla1.Text);
                int salla1_humid = Convert.ToInt32(txtLagSalla1.Text);
                double salla2_temp = Convert.ToDouble(txtTempSalla2.Text);
                int salla2_press = Convert.ToInt32(txtPresSalla2.Text);
                int salla2_humid = Convert.ToInt32(txtLagSalla2.Text);
                double salla3_temp = Convert.ToDouble(txtTempSalla3.Text);
                int salla3_press = Convert.ToInt32(txtPresSalla3.Text);
                int salla3_humid = Convert.ToInt32(txtLagSalla3.Text);

                if (isEdit)
                {
                    (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).UpdateByID(emri,orafillimit,minfillimit, orambarimit, minmbarimit, sezona, ahu_temp, ahu_humid, ahu_air_input, ahu_recycle,
                        salla1_temp, salla1_humid, salla1_press, salla2_temp, salla2_humid, salla2_press, salla3_temp, salla3_humid, salla3_press, id_update);
                    (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, " ", "U ndryshua orari i dites " + id_update.ToString() +"!" );
                    this.Close();
                }
                else
                {
                    (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).Insert(emri, orafillimit, minfillimit, orambarimit, minmbarimit, sezona, ahu_temp, ahu_humid, ahu_air_input, ahu_recycle,
                        salla1_temp, salla1_humid, salla1_press, salla2_temp, salla2_humid, salla2_press, salla3_temp, salla3_humid, salla3_press);
                    (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, " ", "U shtua nje dite ne listen e diteve te orareve!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
