using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for TagTrends.xaml
    /// </summary>
    public partial class TagTrend : Window
    {
        Classes.TrendBrowserViewModel viewModel;
        private static DataOtrilaTableAdapters.TagArchivesTableAdapter _trendTags_ta = new DataOtrilaTableAdapters.TagArchivesTableAdapter();
        DataOtrila dataOpc = new DataOtrila();

        public TagTrend()
        {
            InitializeComponent();

            dateFrom.SelectedDate = DateTime.Now;
            dateTo.SelectedDate = DateTime.Now;
            this.Loaded += TagTrends_Loaded;

        }

        private void TagTrends_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            _trendTags_ta.Fill(dataOpc.TagArchives);
            (new DataOtrilaTableAdapters.TagsTableAdapter()).Fill(dataOpc.Tags);
            cmbTagHis.Items.Clear();
            foreach (DataOtrila.TagsRow tag_ in dataOpc.Tags.Rows)
            {
                if (tag_.Archive == true)
                {
                    cmbTagHis.Items.Add(tag_.Tag);
                }
            }
        }

        private void btnSavetoPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dlg = new PrintDialog();
                if (dlg.ShowDialog() != true)
                    return;
                dlg.PrintVisual(Plot1, "Trend");
            }
            catch (Exception ex) { }
        }

        private void cmbTagHis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                viewModel = null;
                DataContext = null;
                DateTime from = dateFrom.SelectedDate.Value;
                DateTime to = dateTo.SelectedDate.Value;
                viewModel = new Classes.TrendBrowserViewModel(Convert.ToString(cmbTagHis.SelectedValue), from, to);
                DataContext = viewModel;
            }
            catch (Exception ex)
            {
                //dump file                
            }
        }
    }
}
