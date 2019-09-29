using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ScadaOtrila.UserControls
{
    /// <summary>
    /// Interaction logic for AnalogTag.xaml
    /// </summary>
    /// 
    [System.Runtime.InteropServices.Guid("F3129398-CC78-4DC3-870E-B53918E043EA")]
    public partial class AnalogTag : UserControl
    {
        #region Properties

        [Category("AnalogTag")]
        public double Value { get; set; } = -1;

        [Category("AnalogTag")]
        public double Scaling { get; set; } = 1;

        [Category("AnalogTag")]
        public string MachineName { get; set; } = "";

        [Category("AnalogTag")]
        public string OpcServer { get; set; } = "CyProOPC.DA2";

        [Category("AnalogTag")]
        public string OpcTag { get; set; } = "";

        [Category("AnalogTag")]
        public string Description { get; set; }

        [Category("AnalogTag")]
        public int TagId { get; set; } = -1;

        [Category("AnalogTag")]
        public bool ReadOnly { get; set; } = true;

        [Category("AnalogTag")]
        public bool Archive { get; set; } = false;

        [Category("AnalogTag")]
        public bool Alarm { get; set; } = false;

        [Category("AnalogTag")]
        public string Unit { get; set; } = "";

        [Category("AnalogTag")]
        public int RefreshRate { get; set; }

        [Category("AnalogTag")]
        public string TagName { get; set; } = "Temperatura";

        #endregion
        public AnalogTag()
        {
            InitializeComponent();
        }
    }
}
