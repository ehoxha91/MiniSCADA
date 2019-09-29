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
    /// Interaction logic for MenaxhimiDiteveOraret.xaml
    /// </summary>
    public partial class MenaxhimiDiteveOraret : Window
    {
        List<DataOtrila.OrariDiteRow> ditetLista = new List<DataOtrila.OrariDiteRow>();
        public MenaxhimiDiteveOraret()
        {
            InitializeComponent();
            Task.Factory.StartNew(() => LoadData());
        }

        void LoadData()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate () 
            {
                listDitet.Items.Clear();
                DataOtrila dataOtrila = new DataOtrila();
                (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).Fill(dataOtrila.OrariDite);
                foreach (DataOtrila.OrariDiteRow dite in dataOtrila.OrariDite.Rows)
                {
                    
                    Label listLabel = new Label() { Content = dite.Emri, FontSize = 16, Width = listDitet.ActualWidth };
                    ditetLista.Add(dite);
                    listDitet.Items.Add(listLabel);
                }
            }));
        }

        private void BtnMbylle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnNdrysho_Click(object sender, RoutedEventArgs e)
        {
            (new OrariDitaConfig(ditetLista[listDitet.SelectedIndex].ID)).Show();
            Task.Factory.StartNew(() => LoadData());
        }

        private void BtnShlyej_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id_delete = ditetLista[listDitet.SelectedIndex].ID;
                (new DataOtrilaTableAdapters.OrariDiteTableAdapter()).DeleteByID(id_delete);
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, " ", "U shlye orari i dites " + id_delete.ToString());
                Task.Factory.StartNew(() => LoadData());
            }
            catch (Exception)
            {

            }

        }
    }
}
