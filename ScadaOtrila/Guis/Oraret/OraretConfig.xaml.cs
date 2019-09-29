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
    /// Interaction logic for OraretConfig.xaml
    /// </summary>
    public partial class OraretConfig : Window
    {
        private List<DataOtrila.OraretRow> _listaOraret = new List<DataOtrila.OraretRow>();
        private List<DataOtrila.OrariDiteRow> _listaDitet = new List<DataOtrila.OrariDiteRow>();

        public OraretConfig()
        {
            InitializeComponent();
            Task.Factory.StartNew(() => LoadData());
        }

        void LoadData()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate () 
            {
                listOrars.Items.Clear();
                _listaOraret.Clear();
                _listaDitet.Clear();
                cmbStatusi.Items.Clear();
                cmb1.Items.Clear();
                cmb2.Items.Clear();
                cmb3.Items.Clear();
                cmb4.Items.Clear();
                cmb5.Items.Clear();
                cmb6.Items.Clear();
                cmb7.Items.Clear();
                cmb8.Items.Clear();
                cmb9.Items.Clear();
                cmb10.Items.Clear();
                cmb11.Items.Clear();
                cmb12.Items.Clear();
                cmb13.Items.Clear();
                cmb14.Items.Clear();
                cmb15.Items.Clear();
                cmb16.Items.Clear();
                cmb1.Items.Add("JO");
                cmb2.Items.Add("JO");
                cmb3.Items.Add("JO");
                cmb4.Items.Add("JO");
                cmb5.Items.Add("JO");
                cmb6.Items.Add("JO");
                cmb7.Items.Add("JO");
                cmb8.Items.Add("JO");
                cmb9.Items.Add("JO");
                cmb10.Items.Add("JO");
                cmb11.Items.Add("JO");
                cmb12.Items.Add("JO");
                cmb13.Items.Add("JO");
                cmb14.Items.Add("JO");
                cmb15.Items.Add("JO");
                cmb16.Items.Add("JO");
                cmbStatusi.Items.Add("AKTIV");
                cmbStatusi.Items.Add("JOAKTIV");

                DataOtrila dataOtrila = new DataOtrila();
                (new DataOtrilaTableAdapters.OraretTableAdapter()).Fill(dataOtrila.Oraret);
                (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).Fill(dataOtrila.OrariDite);
                foreach (DataOtrila.OraretRow orar in dataOtrila.Oraret.Rows)
                {
                    Label listLabel = new Label() { Content = orar.Emri, FontSize = 16, Width = listOrars.ActualWidth };
                    listOrars.Items.Add(listLabel);
                    _listaOraret.Add(orar);
                }
                foreach (DataOtrila.OrariDiteRow dite in dataOtrila.OrariDite.Rows)
                {
                    _listaDitet.Add(dite);
                    cmb1.Items.Add(dite.Emri);
                    cmb2.Items.Add(dite.Emri);
                    cmb3.Items.Add(dite.Emri);
                    cmb4.Items.Add(dite.Emri);
                    cmb5.Items.Add(dite.Emri);
                    cmb6.Items.Add(dite.Emri);
                    cmb7.Items.Add(dite.Emri);
                    cmb8.Items.Add(dite.Emri);
                    cmb9.Items.Add(dite.Emri);
                    cmb10.Items.Add(dite.Emri);
                    cmb11.Items.Add(dite.Emri);
                    cmb12.Items.Add(dite.Emri);
                    cmb13.Items.Add(dite.Emri);
                    cmb14.Items.Add(dite.Emri);
                    cmb15.Items.Add(dite.Emri);
                    cmb16.Items.Add(dite.Emri);
                }

            }));
        }

        private void ListOrars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(listOrars.SelectedIndex != -1)
                {
                    DataOtrila.OraretRow orariSelektuar = _listaOraret[listOrars.SelectedIndex];
                    cmb1.SelectedValue = orariSelektuar.Dita1;
                    cmb2.SelectedValue = orariSelektuar.Dita2;
                    cmb3.SelectedValue = orariSelektuar.Dita3;
                    cmb4.SelectedValue = orariSelektuar.Dita4;
                    cmb5.SelectedValue = orariSelektuar.Dita5;
                    cmb6.SelectedValue = orariSelektuar.Dita6;
                    cmb7.SelectedValue = orariSelektuar.Dita7;
                    cmb8.SelectedValue = orariSelektuar.Dita8;
                    cmb9.SelectedValue = orariSelektuar.Dita9;
                    cmb10.SelectedValue = orariSelektuar.Dita10;
                    cmb11.SelectedValue = orariSelektuar.Dita11;
                    cmb12.SelectedValue = orariSelektuar.Dita12;
                    cmb13.SelectedValue = orariSelektuar.Dita13;
                    cmb14.SelectedValue = orariSelektuar.Dita14;
                    cmb15.SelectedValue = orariSelektuar.Dita15;
                    cmb16.SelectedValue = orariSelektuar.Dita16;
                    dateFrom.SelectedDate = orariSelektuar.DataFillimit;
                    dateTo.SelectedDate = orariSelektuar.DataMbarimit;
                    txtEmriOrarit.Text = orariSelektuar.Emri;
                    txtPerseritja.Text = orariSelektuar.PeriodaPerseritjes.ToString();
                    keepStatus = true; //This doesn't allow update status on selection change
                    if (orariSelektuar.IsActive)
                        cmbStatusi.SelectedValue = "AKTIV";
                    else cmbStatusi.SelectedValue = "JOAKTIV";
                    keepStatus = false;
                }
            }
            catch (Exception)
            {
            }
        }

        private void BtnKrijoOrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] ditet = new string[18];
                ditet[0] = cmb1.SelectedValue.ToString();
                ditet[1] = cmb2.SelectedValue.ToString();
                ditet[2] = cmb3.SelectedValue.ToString();
                ditet[3] = cmb4.SelectedValue.ToString();
                ditet[4] = cmb5.SelectedValue.ToString();
                ditet[5] = cmb6.SelectedValue.ToString();
                ditet[6] = cmb7.SelectedValue.ToString();
                ditet[7] = cmb8.SelectedValue.ToString();
                ditet[8] = cmb9.SelectedValue.ToString();
                ditet[9] = cmb10.SelectedValue.ToString();
                ditet[10] = cmb11.SelectedValue.ToString();
                ditet[11] = cmb12.SelectedValue.ToString();
                ditet[12] = cmb13.SelectedValue.ToString();
                ditet[13] = cmb14.SelectedValue.ToString();
                ditet[14] = cmb15.SelectedValue.ToString();
                ditet[15] = cmb16.SelectedValue.ToString();

                (new DataOtrilaTableAdapters.OraretTableAdapter()).Insert(txtEmriOrarit.Text, (DateTime)dateFrom.SelectedDate, (DateTime)dateTo.SelectedDate,
                    Convert.ToInt32(txtPerseritja.Text), false, ditet[0], ditet[1], ditet[2], ditet[3], ditet[4], ditet[5], ditet[6], ditet[7], ditet[8], 
                    ditet[9], ditet[10], ditet[11], ditet[12], ditet[13], ditet[14], ditet[15], "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, "", "U krijua orar i ri me emrin: " + txtEmriOrarit.Text);
                Task.Factory.StartNew(() => LoadData());
            }
            catch (Exception)
            {
            }
        }

        private void BtnRuajOrar_Click(object sender, RoutedEventArgs e)
        {
            if(listOrars.SelectedIndex != -1)
            {
                string[] ditet = new string[18];
                ditet[0] = cmb1.SelectedValue.ToString();
                ditet[1] = cmb2.SelectedValue.ToString();
                ditet[2] = cmb3.SelectedValue.ToString();
                ditet[3] = cmb4.SelectedValue.ToString();
                ditet[4] = cmb5.SelectedValue.ToString();
                ditet[5] = cmb6.SelectedValue.ToString();
                ditet[6] = cmb7.SelectedValue.ToString();
                ditet[7] = cmb8.SelectedValue.ToString();
                ditet[8] = cmb9.SelectedValue.ToString();
                ditet[9] = cmb10.SelectedValue.ToString();
                ditet[10] = cmb11.SelectedValue.ToString();
                ditet[11] = cmb12.SelectedValue.ToString();
                ditet[12] = cmb13.SelectedValue.ToString();
                ditet[13] = cmb14.SelectedValue.ToString();
                ditet[14] = cmb15.SelectedValue.ToString();
                ditet[15] = cmb16.SelectedValue.ToString();

                DataOtrila.OraretRow _or = _listaOraret[listOrars.SelectedIndex];
               
                DataOtrilaTableAdapters.OraretTableAdapter orar_ta = new DataOtrilaTableAdapters.OraretTableAdapter();
                orar_ta.DeleteByID(_or.ID);

                if (txtEmriOrarit.Text != "Orari 1")
                    _or.Emri = txtEmriOrarit.Text;
                
                orar_ta.Insert(_or.Emri, (DateTime)dateFrom.SelectedDate, (DateTime)dateTo.SelectedDate, Convert.ToInt32(txtPerseritja.Text), _or.IsActive,
                    ditet[0], ditet[1], ditet[2], ditet[3], ditet[4], ditet[5], ditet[6], ditet[7], ditet[8], ditet[9], ditet[10],
                    ditet[11], ditet[12], ditet[13], ditet[14], ditet[15], "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, "", "U ndryshua orari: " + _or.Emri);
                Task.Factory.StartNew(() => LoadData());
            }
        }

        bool keepStatus = false;
        private void CmbStatusi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (listOrars.SelectedIndex != -1)
                {
                    DataOtrilaTableAdapters.OraretTableAdapter orar_ta = new DataOtrilaTableAdapters.OraretTableAdapter();
                    DataOtrila dataOtrila = new DataOtrila();
                    orar_ta.Fill(dataOtrila.Oraret);
                    if (keepStatus == false)
                    {
                        DataOtrila.OraretRow orariSelektuar = _listaOraret[listOrars.SelectedIndex];
                        if (cmbStatusi.SelectedValue.ToString() == "AKTIV")
                        {
                            //Make all of them deactive
                            foreach (DataOtrila.OraretRow orar in dataOtrila.Oraret.Rows)
                            {
                                orar_ta.UpdateActiveStatus(false, orar.ID);
                            }
                            //Activated the selected one
                            orar_ta.UpdateActiveStatus(true, orariSelektuar.ID);
                        }
                        else if (cmbStatusi.SelectedValue.ToString() == "JOAKTIV")
                        {
                            orar_ta.UpdateActiveStatus(false, orariSelektuar.ID);
                        }
                        Task.Factory.StartNew(() => LoadData());
                    }
                }
                
            }
            catch (Exception) { }
        }
    }
}
