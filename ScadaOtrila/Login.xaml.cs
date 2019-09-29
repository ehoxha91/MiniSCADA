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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScadaOtrila
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            Properties.Settings.Default.lastUser = username;    //Last username will always appear
            Properties.Settings.Default.Save();

            string password = Classes.SecurityManager.CalculateMD5Hash(txtPassword.Password);   //Convert to md5

            DataOtrilaTableAdapters.LoginTableAdapter login_ta = new DataOtrilaTableAdapters.LoginTableAdapter();   //Create a table adapter

            if (password == login_ta.GetPasswordByUsername(username)) //Read encrypted password from database and compare with the input;
            {
                Guis.MainWindow mainOperatorWindow = new Guis.MainWindow(username); //Open the main window
                mainOperatorWindow.Show();                                  //Show main window
                this.Close();                                               //Close login window
            }   
            else
            {
                MessageBox.Show("Wrong password!");
                txtPassword.Clear();
                txtUsername.Focus();
            }

        }
    }
}
