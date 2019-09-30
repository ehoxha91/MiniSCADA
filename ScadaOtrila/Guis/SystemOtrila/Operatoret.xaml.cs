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

namespace ScadaOtrila.Guis.SystemOtrila
{
    /// <summary>
    /// Interaction logic for Operatoret.xaml
    /// </summary>
    public partial class Operatoret : Window
    {
        private List<DataOtrila.LoginRow> _listLogin = new List<DataOtrila.LoginRow>();
        private DataOtrilaTableAdapters.LoginTableAdapter login_ta = new DataOtrilaTableAdapters.LoginTableAdapter();
        private DataOtrila dt = new DataOtrila();
        string _operatori;
        public Operatoret(string _operator)
        {
            InitializeComponent();
            _operatori = _operator;
            LoadData();
        }

        void LoadData()
        {
            try
            {
                _listLogin.Clear();
                listLogins.Items.Clear();
                login_ta.Fill(dt.Login);
                foreach (DataOtrila.LoginRow loginRow in dt.Login.Rows)
                {
                    _listLogin.Add(loginRow);
                    listLogins.Items.Add(loginRow.Username);
                }
            }
            catch (Exception)
            {

            }
        }

        private void BtnRuaj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string password = Classes.EncryptionHelper.Encrypt(txtPassword.Password, "otrila");
                if (listLogins.SelectedIndex !=-1)
                {
                    int ID = _listLogin[listLogins.SelectedIndex].ID;
                    (new DataOtrilaTableAdapters.LoginTableAdapter()).UpdateByID(txtUsername.Text, password, txtEmriMbiemri.Text, txtEmail.Text, true, ID);
                    string _msg = "Operatori: " + _operatori + " ndryshoj te dhenat per operatorin: " + _listLogin[listLogins.SelectedIndex].Username;
                    (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operatori, _msg);
                }
                else
                {
                    (new DataOtrilaTableAdapters.LoginTableAdapter()).Insert(txtUsername.Text, password, txtEmriMbiemri.Text, txtEmail.Text, true);
                    string _msg = "Operatori: " + _operatori + " operatorin e ri: " + txtUsername.Text;
                    (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operatori, _msg);
                }
            }
            catch (Exception)
            {

            }
        }

        private void BtnShlyej_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listLogins.SelectedIndex != -1)
                {
                    (new DataOtrilaTableAdapters.LoginTableAdapter()).DeleteById(_listLogin[listLogins.SelectedIndex].ID);
                    string _msg = "Operatori: " + _operatori + " shlyejti operatorin: " + txtUsername.Text;
                    (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operatori, _msg);
                }
            }
            catch (Exception)
            {

            }
        }

        private void ListLogins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataOtrila.LoginRow usr = _listLogin[listLogins.SelectedIndex];
                txtEmail.Text = usr.Email;
                txtUsername.Text = usr.Username;
                txtPassword.Password = Classes.EncryptionHelper.Decrypt(usr.Password, "otrila");
                txtEmriMbiemri.Text = usr.FullName;
            }
            catch (Exception)
            {

            }
        }
    }
}
