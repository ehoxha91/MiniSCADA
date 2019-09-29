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
    /// Interaction logic for TagManagement.xaml
    /// </summary>
    public partial class TagManagement : Window
    {
        DataOtrila dataOpc = new DataOtrila();
        DataOtrilaTableAdapters.TagsTableAdapter tagsTableAdapter = new DataOtrilaTableAdapters.TagsTableAdapter();
        private static List<DataOtrila.TagsRow> _lista = new List<DataOtrila.TagsRow>();

        public TagManagement()
        {
            InitializeComponent();
            this.Activated += TagManagement_Activated;
            this.Loaded += TagManagement_Loaded;
        }

        private void TagManagement_Loaded(object sender, RoutedEventArgs e)
        {
            selectedprevious = tagDataGrid.SelectedIndex;
            Load();
        }

        private void TagManagement_Activated(object sender, EventArgs e)
        {
            selectedprevious = tagDataGrid.SelectedIndex;
            Load();
        }

        private int selectedprevious;
        private void Load()
        {
            tagsTableAdapter.Fill(dataOpc.Tags);
            _lista.Clear();
            if(tagsTableAdapter != null)
            {
                foreach (DataOtrila.TagsRow row in dataOpc.Tags.Rows)
                {
                    _lista.Add(row);
                }
                this.Dispatcher.BeginInvoke(new Action(delegate
                {
                    tagDataGrid.ItemsSource = null;
                    tagDataGrid.ItemsSource = _lista;
                    tagDataGrid.SelectedIndex = selectedprevious;
                }));
            }

        }

        /// <summary>
        /// Add new tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenTagEditor_Click(object sender, RoutedEventArgs e)
        {
            (new TagEditor(((DataOtrila.TagsRow)(tagDataGrid.SelectedItem)), false)).ShowDialog();
            Task.Factory.StartNew(() => Load());
        }

        /// <summary>
        /// Edit tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNdrysho_Click(object sender, RoutedEventArgs e)
        {
            DataOtrila.TagsRow tagRow = ((DataOtrila.TagsRow)(tagDataGrid.SelectedItem));
            new DataOtrilaTableAdapters.TagsTableAdapter().UpdateTag(tagRow.Name, tagRow.MachineName, tagRow.OpcServer, tagRow.Description, tagRow.Tag, tagRow.Type,
                                                                tagRow.RefreshTime, tagRow.Unit, tagRow.Alarm, tagRow.Archive, tagRow.Scale, tagRow.ID);
            Task.Factory.StartNew(() => Load());
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true)
                    return;
                printDialog.PrintVisual(mainCanvas, "TagList");
            }
            catch { }
        }


        /// <summary>
        /// Set archiving as false;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToogleArchiving(object sender, RoutedEventArgs e)
        {
            int selectedID = ((DataOtrila.TagsRow)(tagDataGrid.SelectedItem)).ID;
            bool archive = ((DataOtrila.TagsRow)(tagDataGrid.SelectedItem)).Archive;
            archive = !archive;
            (new DataOtrilaTableAdapters.TagsTableAdapter()).UpdateArchive(archive, selectedID);
        }


        /// <summary>
        /// Delete tag from database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {
            int selectedID = ((DataOtrila.TagsRow)(tagDataGrid.SelectedItem)).ID;
            (new DataOtrilaTableAdapters.TagsTableAdapter()).DeleteTag(selectedID);
        }

    }
}
