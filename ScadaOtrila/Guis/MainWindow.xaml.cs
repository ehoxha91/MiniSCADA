using ScadaOtrila.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Modbus.Device;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Resources;

namespace ScadaOtrila.Guis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer _opcStackTimer;     //When timer elapses: reads the values from database and updates whatever needed
        private System.Timers.Timer _checkOrar;         //Needed for Orars only
        private bool timeToUpdateOrars = true;          //Tells us when we should update everything; Rising edge only and falling edge!

        private double[] hall_temp = new double[3];     //Values of temperatures inside the halls
        private double[] hall_press = new double[3];    //Pressure values
        private double[] hall_humid = new double[3];    //Humid values;
        private double[] hall_temp_sp = new double[3];  //Setpoints
        private double[] hall_press_sp = new double[3];  
        private double[] hall_humid_sp = new double[3];
        private bool[] hall_status = new bool[3];       //ON/OFF
        private double[] hall_damper_opening = new double[3];
        private int[] hall_air_input_sp = new int[3];
        private bool season;
        private bool small_heaters;

        private double ahu_temp_setpoint; //temperatura e deshiruar e ajrit nga ahu
        private double ahu_temp_current;  //temperatura aktuale ne dalje te ahu-se
        private double ahu_air_in_setpoint; //Air input setpoint
        private double ahu_air_in;  //Ajri ne dalje te ahu-se
        private double ahu_air_out; //Ajri ne hyrje te ahu-se
        private double ahu_humid_current; //lageshtia aktuale
        private double ahu_humid_setpoint; //lageshtia e deshiruar
        private double ahu_recycle_setpoint; //recycle
        private double ahu_temp_out;

        private bool[] trend = new bool[3];
        //VRF
        VRFState vrfState = VRFState.AUTO;
        VRFState forDisplay = VRFState.AUTO;
        private int VRFTag = 0; // off;
        private bool vrf_changed = true;
        private ushort heat_setpoint_vrf= 25;
        private ushort cool_setpoint_vrf= 17;

        private ModbusSerialMaster master;
        private SerialPort _modbusSerialPort;

        Thread _opcThread;

        private string _operator;
        private bool debugging = false;

        TrendBrowserViewModelMT viewModelSalla; //Model for binding the plot

        public MainWindow(string _user)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Activated += MainWindow_Activated;
            this.Closing += MainWindow_Closing;
            _operator = _user;
            if (Properties.Settings.Default.SERIALNUMBER == "ejuphoxha123123otrila44" || (DateTime.Now.Year<2021 && DateTime.Now.Month < 8))
            {
                trend[0] = false;
                trend[1] = false;
                trend[2] = false;
                //Create serial port that we will use for ahu communication
                ConfigSerial();

                hall_air_input_sp[2] = Properties.Settings.Default.AirSp1;
                hall_air_input_sp[1] = Properties.Settings.Default.AirSp2;
                hall_air_input_sp[0] = Properties.Settings.Default.AirSp3;
                txtAirFlowSP1.Text = Properties.Settings.Default.AirSp1.ToString();
                txtAirFlowSP2.Text = Properties.Settings.Default.AirSp2.ToString();
                txtAirFlowSP3.Text = Properties.Settings.Default.AirSp3.ToString();

                btnSeason.Content = " -- ";
                btnSeason.Background = new SolidColorBrush(Colors.LightGray);
                //Probably is better if we just run this a seperate thread, since communicates with
                //the main program only through Database.
                _opcThread = new Thread(new ThreadStart(OpcManager.StartOpcMasteR));
                _opcThread.Start();

                _opcStackTimer = new System.Timers.Timer(1000);
                _opcStackTimer.Elapsed += _opcStackTimer_Elapsed;
                _opcStackTimer.Start();

                //_checkOrar = new System.Timers.Timer(50000);
                //_checkOrar.Elapsed += _checkOrar_Elapsed;
                //_checkOrar.Start();
            }
        }

        private void _checkOrar_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _checkOrar.Stop();
            CheckOraret();
            _checkOrar.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _opcStackTimer.Stop();
                _opcStackTimer.Dispose();
                if (_modbusSerialPort.IsOpen)
                    _modbusSerialPort.Close();
                master.Dispose();
                _opcThread.Abort();
                OpcManager.StopThread();
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex){ }
        }

        private void _opcStackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DataOtrilaTableAdapters.TagLiveTableAdapter tagLiveTableAdapter = new DataOtrilaTableAdapters.TagLiveTableAdapter();
            DataOtrila data = new DataOtrila();
            tagLiveTableAdapter.Fill(data.TagLive);

            foreach (DataOtrila.TagLiveRow tag in data.TagLive.Rows)
            {
                switch(tag.TagName)
                {
                    case "TempSalla1":
                        hall_temp[0] = tag.Value;
                        break;
                    case "TempSalla2":
                        hall_temp[1] = tag.Value;
                        break;
                    case "TempSalla3":
                        hall_temp[2] = tag.Value;
                        break;
                    case "TempSetpointS1":
                        hall_temp_sp[0] = tag.Value;
                        break;
                    case "TempSetpointS2":
                        hall_temp_sp[1] = tag.Value;
                        break;
                    case "TempSetpointS3":
                        hall_temp_sp[2] = tag.Value;
                        break;
                    case "PresioniSalla1":
                        hall_press[0] = tag.Value;
                        break;
                    case "PresioniSalla2":
                        hall_press[1] = tag.Value;
                        break;
                    case "PresioniSalla3":
                        hall_press[2] = tag.Value;
                        break;
                    case "PresureSetpS1":
                        hall_press_sp[0] = tag.Value;
                        break;
                    case "PresureSetpS2":
                        hall_press_sp[1] = tag.Value;
                        break;
                    case "PresureSetpS3":
                        hall_press_sp[2] = tag.Value;
                        break;
                    case "LagshtiaSalla1":
                        hall_humid[0] = tag.Value;
                        break;
                    case "LagshtiaSalla2":
                        hall_humid[1] = tag.Value;
                        break;
                    case "LagshtiaSalla3":
                        hall_humid[2] = tag.Value;
                        break;
                    case "HumidSetpS1":
                        hall_humid_sp[0] = tag.Value;
                        break;
                    case "HumidSetpS2":
                        hall_humid_sp[1] = tag.Value;
                        break;
                    case "HumidSetpS3":
                        hall_humid_sp[2] = tag.Value;
                        break;
                    case "HallStatus1":
                        hall_status[0] = Convert.ToBoolean(tag.Value);
                        break;
                    case "HallStatus2":
                        hall_status[1] = Convert.ToBoolean(tag.Value);
                        break;
                    case "HallStatus3":
                        hall_status[2] = Convert.ToBoolean(tag.Value);
                        break;
                    case "Season":
                        season = Convert.ToBoolean(tag.Value);
                        break;
                    case "VRFStatus":
                        int _val = (int)tag.Value;
                        if(VRFTag != _val)
                        {
                            vrf_changed = true;
                        }
                        VRFTag = _val;

                        break;
                    case "DamperS1":
                        hall_damper_opening[0] = tag.Value;
                        break;
                    case "DamperS2":
                        hall_damper_opening[1] = tag.Value;
                        break;
                    case "DamperS3":
                        hall_damper_opening[2] = tag.Value;
                        break;
                    case "SmallHeaters":
                        small_heaters = Convert.ToBoolean(tag.Value);
                        break;
                    default:
                        break;
                }
            }
                       
            CheckOraret();
            ReadAHU();
            UpdateAHU();
            UpdateVRF();
            Task.Factory.StartNew(()=>UpdateUI());
        }

        private void CheckOraret()
        {
            try
            {
                DataOtrila dataOtrila = new DataOtrila();
                DataOtrilaTableAdapters.OraretTableAdapter oraret_ta = new DataOtrilaTableAdapters.OraretTableAdapter();
                oraret_ta.Fill(dataOtrila.Oraret);
                foreach (DataOtrila.OraretRow orar in dataOtrila.Oraret.Rows)
                {
                    if(orar.IsActive)
                    {
                        OrarProcess(orar);
                        break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Problem me kompletimin e orarit!");
            }
        }

        private void OrarProcess(DataOtrila.OraretRow _orari)
        {
            try
            {
                if(_orari.DataFillimit.Year == DateTime.Now.Year)
                {
                    if(_orari.DataFillimit.Month == DateTime.Now.Month)
                    {
                        if(_orari.DataFillimit.Day >= DateTime.Now.Day)
                        {
                            //Orari egzekutohet
                            int diff = DateTime.Now.Day - _orari.DataFillimit.Day;
                            switch (diff)
                            {
                                case 0:
                                    //ExecuteDay(diff);
                                    break;
                                case 1:
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
                                    break;
                                case 5:
                                    break;
                                case 6:
                                    break;
                                case 7:
                                    break;
                                case 8:
                                    break;
                                case 9:
                                    break;
                                case 10:
                                    break;
                                case 11:
                                    break;
                                case 12:
                                    break;
                                case 13:
                                    break;
                                case 14:
                                    break;
                                case 15:
                                    break;
                                case 16:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else if (_orari.DataFillimit.Month > DateTime.Now.Month)
                    {
                        //Orari egzekutohet
                    }
                    else
                    {
                        //Orari nuk egzekutohet
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void AddToStack(string opcServer, string tag, double newValue)
        {
            DataOtrila data = new DataOtrila();
            DataOtrilaTableAdapters.OpcStackOtrilaTableAdapter stack_ta = new DataOtrilaTableAdapters.OpcStackOtrilaTableAdapter();
            stack_ta.Insert(opcServer, tag, newValue, DateTime.Now);
        }

        private void UpdateUI()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                txtTmpAs1.Text = hall_temp[0].ToString() + " *C";
                txtTmpAs2.Text = hall_temp[1].ToString() + " *C";
                txtTmpAs3.Text = hall_temp[2].ToString() + " *C";

                txtTmpSets1.Text = hall_temp_sp[0].ToString() + " *C";
                txtTmpSets2.Text = hall_temp_sp[1].ToString() + " *C";
                txtTmpSet3.Text = hall_temp_sp[2].ToString() + " *C";

                txtPrAs1.Text = hall_press[0].ToString() + " Pa";
                txtPrAs2.Text = hall_press[1].ToString() + " Pa";
                txtPrAs3.Text = hall_press[2].ToString() + " Pa";

                txtPrSet1.Text = hall_press_sp[0].ToString() + " Pa";
                txtPrSet2.Text = hall_press_sp[1].ToString() + " Pa";
                txtPrSet3.Text = hall_press_sp[2].ToString() + " Pa";

                txtLagAs1.Text = hall_humid[0].ToString() + " %";
                txtLagAs2.Text = hall_humid[1].ToString() + " %";
                txtLagAs3.Text = hall_humid[2].ToString() + " %";

                txtLagSet1.Text = hall_humid_sp[0].ToString() + " %";
                txtLagSet2.Text = hall_humid_sp[1].ToString() + " %";
                txtLagSet3.Text = hall_humid_sp[2].ToString() + " %";

                txtDamper1.Text = (hall_damper_opening[2] / 10.0).ToString() + " %";
                txtDamper2.Text = (hall_damper_opening[1] / 10.0).ToString() + " %";
                txtDamper3.Text = (hall_damper_opening[0] / 10.0).ToString() + " %";

                if (small_heaters)
                {
                    btnNxemsatOnOff.Content = "Nxemsat ON";
                    btnNxemsatOnOff.Background = new SolidColorBrush(Colors.LightSalmon);
                }
                else
                {
                    btnNxemsatOnOff.Content = "Nxemsat OFF";
                    btnNxemsatOnOff.Background = new SolidColorBrush(Colors.LightGray);
                }

                if (hall_status[0])
                {
                    btnSalla1OnOff.Content = "ON";
                    btnSalla1OnOff.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    btnSalla1OnOff.Content = "OFF";
                    btnSalla1OnOff.Background = new SolidColorBrush(Colors.LightGray);
                }

                if (hall_status[1])
                {
                    btnSalla2OnOff.Content = "ON";
                    btnSalla2OnOff.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    btnSalla2OnOff.Content = "OFF";
                    btnSalla2OnOff.Background = new SolidColorBrush(Colors.LightGray);
                }

                if (hall_status[2])
                {
                    btnSalla3OnOff.Content = "ON";
                    btnSalla3OnOff.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    btnSalla3OnOff.Content = "OFF";
                    btnSalla3OnOff.Background = new SolidColorBrush(Colors.LightGray);
                }

                if(season == false)
                {
                    btnSeason.Content = "NGROHJE";
                    btnSeason.Background = new SolidColorBrush(Colors.LightSalmon);
                }
                else if (season == true)
                {
                    btnSeason.Content = "FTOHJE";
                    btnSeason.Background = new SolidColorBrush(Colors.LightBlue);
                }

                //AHU Page updates:

                txtTempCurrentAHU.Text =  ahu_temp_current.ToString() + " *C";
                txtTempSetpointAHU.Text = ahu_temp_setpoint.ToString() + " *C";
                txtAirInput.Text = ahu_air_in.ToString() + " m3/h";
                txtAirInputSp.Text = ahu_air_in_setpoint.ToString() + " m3/h";
                txtAirOut.Text = ahu_air_out.ToString() + " m3/h";
                txtLageshtiaAktuale.Text = ahu_humid_current.ToString() + " %";
                txtLageshtiaSp.Text = ahu_humid_setpoint.ToString() + " %";
                txtRiciklimi.Text = ahu_recycle_setpoint.ToString() + " %";
                txtOutsideTemp.Text = "Temperatura e jashtme: "+ ahu_temp_out.ToString() + " *C";

                lblAlarmS1.Content = "";
                lblAlarmS1.Background = Brushes.Transparent;
                lblAlarmS2.Content = "";
                lblAlarmS2.Background = Brushes.Transparent;
                lblAlarmS3.Content = "";
                lblAlarmS3.Background = Brushes.Transparent;

                if (hall_temp[0] > 26)
                {
                    lblAlarmS1.Content ="Temperatura e lartë!";
                    lblAlarmS1.Background = Brushes.LightSalmon;
                }
                else if(hall_temp[0] < 17)
                {
                    lblAlarmS1.Content = "Temperatura e ulët!";
                    lblAlarmS1.Background = Brushes.LightBlue;
                }
                if (hall_temp[1] > 26)
                {
                    lblAlarmS2.Content = "Temperatura e lartë!";
                    lblAlarmS2.Background = Brushes.LightSalmon;
                }
                else if (hall_temp[1] < 17)
                {
                    lblAlarmS2.Content = "Temperatura e ulët!";
                    lblAlarmS2.Background = Brushes.LightBlue;
                }

                if (hall_temp[2] > 26)
                {
                    lblAlarmS3.Content = "Temperatura e lartë!";
                    lblAlarmS3.Background = Brushes.LightSalmon;
                }
                else if (hall_temp[2] < 17)
                {
                    lblAlarmS3.Content = "Temperatura e ulët!";
                    lblAlarmS3.Background = Brushes.LightBlue;
                }

                //VRF Page
                switch (forDisplay)
                {
                    case VRFState.OFF:
                        //Turn OFF
                        try
                        {
                            btnNgrohjeVRF.Background = new SolidColorBrush(Colors.LightGray);
                            btnFtohjeVRF.Background = new SolidColorBrush(Colors.LightGray);
                            btnOffVRF.Background = new SolidColorBrush(Colors.LightSalmon);
                        }
                        catch (Exception ex)
                        {
                            //Log errors;
                        }
                        break;
                    case VRFState.HEATING:
                        //Heating Mode
                        try
                        {
                            btnNgrohjeVRF.Background = new SolidColorBrush(Colors.LightSalmon);
                            btnFtohjeVRF.Background = new SolidColorBrush(Colors.LightGray);
                            btnOffVRF.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        catch (Exception)
                        {
                            //Log erros
                        }

                        break;
                    case VRFState.COOLING:
                        //Cooling Mode
                        try
                        {
                            btnNgrohjeVRF.Background = new SolidColorBrush(Colors.LightGray);
                            btnFtohjeVRF.Background = new SolidColorBrush(Colors.LightBlue);
                            btnOffVRF.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        catch (Exception ex)
                        {
                            //Log error
                        }
                        break;
                    default: break;
                }
                if (vrfState == VRFState.AUTO)
                    btnAutoVRF.Background = new SolidColorBrush(Colors.LightGreen);

                DataOtrila dataOtrila = new DataOtrila();
                DataOtrilaTableAdapters.EventLogsTableAdapter events_ta = new DataOtrilaTableAdapters.EventLogsTableAdapter();
                events_ta.FillTop10(dataOtrila.EventLogs);
                listBoxLiveEvents.Items.Clear();
                foreach (DataOtrila.EventLogsRow row in dataOtrila.EventLogs.Rows)
                {
                    listBoxLiveEvents.Items.Add("["+row.DateTime.ToString()+"]   "+row.Event);
                }

            }));
        }

        //Read variables from AHU
        private void ReadAHU()
        {
            if (!_modbusSerialPort.IsOpen)
            {
                try
                {
                    _modbusSerialPort.Open();
                }
                catch (Exception ex12)
                {
                    File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex12.Message + ": Opening serial port");
                }
            }
            if(_modbusSerialPort.IsOpen)
            {
                //Temperature Setpoint
                ushort[] vals4 = master.ReadHoldingRegisters(1, 32, 1);
                try
                {
                    ahu_temp_setpoint = Convert.ToDouble(vals4[0]) * 0.1;
                }
                catch (Exception ex){ File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message +"\nError reading setpoint temp\n"); }

                //AHU Output Temp.
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 45, 1);
                    ahu_temp_current = Convert.ToDouble(vals4[0]) * 0.1;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\nError reading current temp\n" ); }

                //Outdoors temp.
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 43, 1);
                    ahu_temp_out = Convert.ToDouble(vals4[0]) * 0.1;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\nError reading outside temp."); }

                //humidity setpoint
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 28, 1);
                    ahu_humid_setpoint = Convert.ToDouble(vals4[0]) * 0.1;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\n Error reading humidity setpoint\n"); }
                //current humidity
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 48, 1);
                    ahu_humid_current = Convert.ToDouble(vals4[0]) * 0.1;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message +"\nError reading current humidity\n"); }

                //Air flow setpoint
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 341, 1);
                    ahu_air_in_setpoint = Convert.ToDouble(vals4[0]) * 1.0;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\nError reading airflow setpoint\n"); }
                //Air flow input
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 323, 2);
                    ahu_air_in = Convert.ToDouble(vals4[0]) * 1.0;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message +"\nError reading airflow input\n"); }
                //Air flow output
                try
                {
                    vals4[0] = 0;
                    vals4 = master.ReadHoldingRegisters(1, 324, 1);
                    ahu_air_out = Convert.ToDouble(vals4[0]) * 1.0;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\nError reading airflow output\n"); }

                //Recycle setpoint
                try
                {
                    vals4 = master.ReadHoldingRegisters(1, 327, 1);
                    ahu_recycle_setpoint = Convert.ToDouble(vals4[0]) * 1.0;
                }
                catch (Exception ex) { File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\n Error reading recycle setpoint\n"); }
            }
        }

        //Update ahu variables
        private void UpdateAHU()
        {
            if(!_modbusSerialPort.IsOpen)
            {
                try
                {
                    _modbusSerialPort.Open();
                }
                catch (Exception)
                {
                    File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", "Unable to open serial port! Time: " + DateTime.Now.ToString());
                }
            }
            if(_modbusSerialPort.IsOpen)
            {
                try
                {
                    int active_halls = 0;
                    double max_humid = 0.0;
                    for (int i = 0; i <= 2; i++)
                    {
                        if (max_humid < hall_humid_sp[i])
                            max_humid = hall_humid_sp[i];
                        if (hall_status[i])
                            active_halls++;
                    }
                    max_humid = max_humid * 10;
                    if (max_humid != ahu_humid_current)
                    {
                        try
                        {
                            master.WriteSingleRegister(1, 28, (ushort)max_humid);
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + ": Unable to write humidity setpoint! " + DateTime.Now.ToString());
                        }
                    }

                    double air_flow_sp = 0;
                    switch (active_halls)
                    {
                        case 0:
                            air_flow_sp = 0;
                            break;
                        case 1:
                            {
                                if (hall_status[2])
                                    air_flow_sp = hall_air_input_sp[2] * (hall_damper_opening[2] / 1000);
                                else if (hall_status[1])
                                    air_flow_sp = hall_air_input_sp[1] * (hall_damper_opening[1] / 1000);
                                else if (hall_status[0])
                                    air_flow_sp = hall_air_input_sp[0] * (hall_damper_opening[0] / 1000);
                            }
                            break;
                        case 2:
                            {
                                if (hall_status[2] && hall_status[1])
                                {
                                    air_flow_sp = hall_air_input_sp[2] * (hall_damper_opening[2] / 1000) + hall_air_input_sp[1] * (hall_damper_opening[1] / 1000);
                                }
                                else if (hall_status[2] && hall_status[0])
                                {
                                    air_flow_sp = hall_air_input_sp[2] * (hall_damper_opening[2] / 1000) + hall_air_input_sp[0] * (hall_damper_opening[0] / 1000);
                                }
                                else if (hall_status[1] && hall_status[0])
                                {
                                    air_flow_sp = hall_air_input_sp[1] * (hall_damper_opening[1] / 1000) + hall_air_input_sp[0] * (hall_damper_opening[0] / 1000);
                                }
                            }
                            break;
                        case 3:
                            {
                                air_flow_sp = hall_air_input_sp[2] * (hall_damper_opening[2] / 1000)
                                    + hall_air_input_sp[1] * (hall_damper_opening[1] / 1000)
                                    + hall_air_input_sp[0] * (hall_damper_opening[0] / 1000);
                            }
                            break;
                        default: air_flow_sp = 0; break;
                    }

                    air_flow_sp += additionalAir;
                    if (air_flow_sp != ahu_air_in_setpoint)
                    {
                        try
                        {
                            master.WriteSingleRegister(1, 341, (ushort)air_flow_sp);
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + ": Unable to write air_flow setpoint! " + DateTime.Now.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + ": " + DateTime.Now.ToString());
                }

            }
        }

        //Configure Modbus
        private void ConfigSerial()
        {
            try
            {
                //AHU Modbus
                _modbusSerialPort = new SerialPort();
                _modbusSerialPort.PortName = "COM1";
                _modbusSerialPort.BaudRate = 19200;
                _modbusSerialPort.DataBits = 8;
                _modbusSerialPort.StopBits = StopBits.Two;
                _modbusSerialPort.Parity = Parity.None;
                master = ModbusSerialMaster.CreateRtu(_modbusSerialPort);
                master.Transport.ReadTimeout = 2000;
                master.Transport.WriteTimeout = 5000;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error!", MessageBoxButton.OK, MessageBoxImage.Hand); }

            /*
  			//Panels Modbus
			sp2 = new SerialPort();
			sp2.PortName = "COM4";
			sp2.BaudRate = 9600;
			sp2.DataBits = 8;
			sp2.StopBits = StopBits.One;
			sp2.Parity = Parity.None;
			master2 = ModbusSerialMaster.CreateRtu(sp2);
			master2.Transport.ReadTimeout = 3000;
			master2.Transport.WriteTimeout = 5000;
			pollTimer = new Timer();
			pollTimer.Interval = 3000.0;
			pollTimer.Elapsed += pollTimer_Elapsed;
			pollTimer.Start();
             */
        }

        private void UpdateVRF()
        {
            if (vrfState == VRFState.AUTO && vrf_changed == true)
            {
                switch (VRFTag)
                {
                    case 0:
                        //Turn OFF
                        try
                        {
                            Task.Factory.StartNew(() => VRFModbus.TurnOFF());
                            forDisplay = VRFState.OFF;
                        }
                        catch (Exception ex)
                        {
                            //Log errors;
                        }
                        break;
                    case 1:
                        //Heating Mode
                        try
                        {
                            Task.Factory.StartNew(() => VRFModbus.SetWinterMode(heat_setpoint_vrf));
                            forDisplay = VRFState.HEATING;
                        }
                        catch (Exception)
                        {
                            //Log erros
                        }

                        break;
                    case 2:
                        //Cooling Mode
                        try
                        {
                            Task.Factory.StartNew(() => VRFModbus.SetSummerMode(cool_setpoint_vrf));
                            forDisplay = VRFState.COOLING;
                        }
                        catch (Exception ex)
                        {
                            //Log error
                        }
                        break;
                    default: break;
                }
                vrf_changed = false; //we applied the change.
            }
        }

        #region OTHER

        private void MainWindow_Activated(object sender, EventArgs e)
        {

            UpdateUI();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }


        private void MenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            ((MenuItem)sender).Background = Brushes.Aquamarine;
        }

        private void MenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            ((MenuItem)sender).Background = Brushes.Transparent;
        }

        private void MenuDatabase_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nuk keni qasje te mjaftueshme, pasi qe lehte mund te prishni konfigurimin e databazes!");
        }


        private void MenuOperators_Click(object sender, RoutedEventArgs e)
        {
            //Gui change password add operators!
            (new SystemOtrila.Operatoret(_operator)).ShowDialog();
        }

        private void menuOpcTagManagement_Click(object sender, RoutedEventArgs e)
        {
            (new Guis.Tags.TagManagement()).ShowDialog();
            //Task.Factory.StartNew(() => LoadDesktops());
        }

        private void menuPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dlg = new PrintDialog();
                if (dlg.ShowDialog() != true)
                    return;
                dlg.PrintVisual(mainGrid, "Trend");
            }
            catch (Exception ex) { }
        }


        private void menuTrends_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuAlarms_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Menu_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        #endregion

        #region Buttons SALLA 3

        private void BtnSalla1OnOff_Click(object sender, RoutedEventArgs e)
        {
            string _event;
            if(hall_status[0])
            {
                _event = "Salla 3 eshte deaktivizuar nga operatori "+_operator;
            }
            else
            {
                _event = "Salla 3 eshte aktivizuar nga operatori "+_operator;
            }
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _event);
            AddToStack("CyBroOPC.DA2", "c15842.hall_active[0]", Convert.ToDouble(!hall_status[0]));
        }

        private void BtnIncTempS1_Click(object sender, RoutedEventArgs e) //""
        {
            hall_temp_sp[0] +=1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 3 u ndryshua ne: "+hall_temp_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[0]", hall_temp_sp[0]);
        }

        private void BtnDecTempS1_Click(object sender, RoutedEventArgs e)
        {
            hall_temp_sp[0] -= 1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 3 u ndryshua ne: " + hall_temp_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[0]", hall_temp_sp[0]);
        }

        private void BtnIncPresS1_Click(object sender, RoutedEventArgs e) //
        {
            hall_press_sp[0] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 3 u ndryshua ne: " + hall_press_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[0]", hall_press_sp[0]);
        }

        private void BtnDecPresS1_Click(object sender, RoutedEventArgs e)
        {
            hall_press_sp[0] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 3 u ndryshua ne: " + hall_press_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[0]", hall_press_sp[0]);
        }

        private void BtnIncLagS1_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[0] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 3 u ndryshua ne: " + hall_humid_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[0]", hall_humid_sp[0]);
        }

        private void BtnDecLagS1_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[0] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 3 u ndryshua ne: " + hall_humid_sp[0].ToString()
                + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[0]", hall_humid_sp[0]);
        }

        #endregion

        #region Buttons SALLA 2
        private void BtnSalla2OnOff_Click(object sender, RoutedEventArgs e)
        {
            string _event;
            if (hall_status[1])
            {
                _event = "Salla 2 eshte deaktivizuar nga operatori " + _operator;
            }
            else
            {
                _event = "Salla 2 eshte aktivizuar nga operatori " + _operator;
            }
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _event);
            AddToStack("CyBroOPC.DA2", "c15842.hall_active[1]", Convert.ToDouble(!hall_status[1]));
        }

        private void BtnIncTempS2_Click(object sender, RoutedEventArgs e)
        {
            hall_temp_sp[1] += 1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 2 u ndryshua ne: " + hall_temp_sp[1].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[1]", hall_temp_sp[1]);
        }

        private void BtnDecTempS2_Click(object sender, RoutedEventArgs e)
        {
            hall_temp_sp[1] -= 1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 2 u ndryshua ne: " + hall_temp_sp[1].ToString()
+ ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[1]", hall_temp_sp[1]);
        }

        private void BtnIncPresS2_Click(object sender, RoutedEventArgs e)
        {
            hall_press_sp[1] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 2 u ndryshua ne: " + hall_press_sp[1].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[1]", hall_press_sp[1]);
        }

        private void BtnDecPresS2_Click(object sender, RoutedEventArgs e)
        {
            hall_press_sp[1] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 2 u ndryshua ne: " + hall_press_sp[1].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[1]", hall_press_sp[1]);
        }

        private void BtnIncLagS2_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[1] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 2 u ndryshua ne: " + hall_humid_sp[1].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[1]", hall_humid_sp[1]);
        }

        private void BtnDecLagS2_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[1] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 2 u ndryshua ne: " + hall_humid_sp[1].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[1]", hall_humid_sp[1]);
        }

        #endregion

        #region Buttons SALLA 1

        private void BtnSalla3OnOff_Click(object sender, RoutedEventArgs e)
        {
            string _event;
            if (hall_status[2])
            {
                _event = "Salla 1 eshte deaktivizuar nga operatori " + _operator;
            }
            else
            {
                _event = "Salla 1 eshte aktivizuar nga operatori " + _operator;
            }
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _event);
            AddToStack("CyBroOPC.DA2", "c15842.hall_active[2]", Convert.ToDouble(!hall_status[2]));
        }

        private void BtnIncTempS3_Click(object sender, RoutedEventArgs e)
        {
            hall_temp_sp[2] += 1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 1 u ndryshua ne: " + hall_temp_sp[2].ToString()
            + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[2]", hall_temp_sp[2]);
        }

        private void BtnDecTempS3_Click(object sender, RoutedEventArgs e)
        {
            hall_temp_sp[2] -=1;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Temperatura e deshiruar ne sallen 1 u ndryshua ne: " + hall_temp_sp[2].ToString()
+ ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.temp_set[2]", hall_temp_sp[2]);
        }

        private void BtnIncPresS3_Click(object sender, RoutedEventArgs e)
        {
            hall_press_sp[2] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 1 u ndryshua ne: " + hall_press_sp[2].ToString()
+ ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[2]", hall_press_sp[2]);
        }

        private void BtnDecPresS3_Click(object sender, RoutedEventArgs e)
        {
            hall_press_sp[2] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Presioni i deshiruar ne sallen 1 u ndryshua ne: " + hall_press_sp[2].ToString()
+ ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.pressure_set[2]", hall_press_sp[2]);
        }

        private void BtnIncLagS3_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[2] += 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 1 u ndryshua ne: " + hall_humid_sp[2].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[2]", hall_humid_sp[2]);
        }

        private void BtnDecLagS3_Click(object sender, RoutedEventArgs e)
        {
            hall_humid_sp[2] -= 5;
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Lageshtia e deshiruar ne sallen 1 u ndryshua ne: " + hall_humid_sp[2].ToString()
    + ". Nga Operatori: " + _operator);
            AddToStack("CyBroOPC.DA2", "c15842.humid_set[2]", hall_humid_sp[2]);
        }

        #endregion

        #region Klima Komora

        private void BtnResetoAlarmet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool[] bools = new bool[1]
                {
                true
                };
                master.WriteMultipleCoils(1, 102, bools);
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Operatori: " + _operator +" resetoi alarmet e AHU-së!");
            }
            catch
            {
                File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", "\n Unable to reset alarms! " + DateTime.Now.ToString());
            }
        }

        private void BtnSeason_Click(object sender, RoutedEventArgs e)
        {
            int _val = 2;
            if (season == true)
            {
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Operatori: " + _operator + " ndryshoi modin nga Ftohje ne Ngrohje!");
                _val = 0;
            }
            else if(season == false)
            {
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Operatori: " + _operator + " ndryshoi modin nga Ngrohje ne Ftohje!");
                _val = 1;
            }
            AddToStack("CyBroOPC.DA2", "c15842.season", Convert.ToDouble(_val));
        }

        private void BtnDecTempSetAHU_Click(object sender, RoutedEventArgs e)
        {
            ushort _newTemp = (ushort)((ahu_temp_setpoint -0.5)*10);
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Operatori: " + _operator + " ndryshoi temperaturen e dëshiruar në AHU! " +
                "Vlera e re: " +(ahu_temp_setpoint-0.5).ToString());
            master.WriteSingleRegister(1, 32, _newTemp);
        }

        private void BtnIncTempSetAHU_Click(object sender, RoutedEventArgs e)
        {
            ushort _newTemp = (ushort)((ahu_temp_setpoint + 0.5) * 10);
            master.WriteSingleRegister(1, 32, _newTemp);
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, "Operatori: " + _operator + " ndryshoi temperaturen e dëshiruar në AHU! " +
            "Vlera e re: " + (ahu_temp_setpoint + 0.5).ToString());
        }

        private void BtnDecAirInSp_Click(object sender, RoutedEventArgs e)
        {
            additionalAir -= 100;
        }

        private float additionalAir = 0.0f;
        private void BtnIncAirInSp_Click(object sender, RoutedEventArgs e)
        {
            additionalAir += 100;
        }

        private void BtnNxemsatOnOff_Click(object sender, RoutedEventArgs e)
        {
            string _msg;
            if (small_heaters)
                _msg = "Operatori: " + _operator + " i ndali nxemsat ndihmes!";
            else
                _msg = "Operatori: " + _operator + " i aktivizoj nxemsat ndihmes!";

            AddToStack("CyProOPC.DA2", "c15842.cybro_qx00", Convert.ToDouble(!small_heaters));
            (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
        }

        private void BtnDecLageshtia_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnIncLageshtia_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnIncRecycle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                master.WriteSingleRegister(1, 327, (ushort)(ahu_recycle_setpoint + 5));
                string _msg = "Operatori: " + _operator + " ndryshoj % e riciklimit te ajrit ne AHU, ne: " + (ahu_recycle_setpoint + 5).ToString() ;
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception ex)
            {
                File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\n Error updating recycle setpoing; INCREMENT \n " + DateTime.Now.ToString());
            }
        }

        private void BtnDecRecycle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                master.WriteSingleRegister(1, 327, (ushort)(ahu_recycle_setpoint -5));
                string _msg = "Operatori: " + _operator + " ndryshoj % e riciklimit te ajrit ne AHU, ne: " + (ahu_recycle_setpoint - 5).ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception ex)
            {
                File.WriteAllText("D:\\ErrorLogOtrilaScada.txt", ex.Message + "\n Error updating recycle setpoing. DECREMENT\n" + DateTime.Now.ToString());
            }
        }

        private void btnIncAirSp1_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP1.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP1.Text) + 100);
            hall_air_input_sp[2] = Convert.ToInt32(txtAirFlowSP1.Text);
            Properties.Settings.Default.AirSp1 = Convert.ToInt32(txtAirFlowSP1.Text);
            Properties.Settings.Default.Save();
        }
        private void btnDecAirSp1_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP1.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP1.Text) - 100);
            hall_air_input_sp[2] = Convert.ToInt32(txtAirFlowSP1.Text);
            Properties.Settings.Default.AirSp1 = Convert.ToInt32(txtAirFlowSP1.Text);
            Properties.Settings.Default.Save();
        }

        private void btnIncAirSp2_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP2.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP2.Text) + 100);
            hall_air_input_sp[1] = Convert.ToInt32(txtAirFlowSP2.Text);
            Properties.Settings.Default.AirSp2 = Convert.ToInt32(txtAirFlowSP2.Text);
            Properties.Settings.Default.Save();
        }
        private void btnDecAirSp2_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP2.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP2.Text) - 100);
            hall_air_input_sp[1] = Convert.ToInt32(txtAirFlowSP2.Text);
            Properties.Settings.Default.AirSp2 = Convert.ToInt32(txtAirFlowSP2.Text);
            Properties.Settings.Default.Save();
        }

        private void btnIncAirSp3_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP3.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP3.Text) + 100);
            hall_air_input_sp[0] = Convert.ToInt32(txtAirFlowSP3.Text);
            Properties.Settings.Default.AirSp3 = Convert.ToInt32(txtAirFlowSP3.Text);
            Properties.Settings.Default.Save();
        }
        private void btnDecAirSp3_Click(object sender, RoutedEventArgs e)
        {
            txtAirFlowSP3.Text = Convert.ToString(Convert.ToInt32(txtAirFlowSP3.Text) - 100);
            hall_air_input_sp[0] = Convert.ToInt32(txtAirFlowSP3.Text);
            Properties.Settings.Default.AirSp3 = Convert.ToInt32(txtAirFlowSP3.Text);
            Properties.Settings.Default.Save();
        }

        #endregion

        #region VRF

        private void BtnNgrohje_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                heat_setpoint_vrf = Convert.ToUInt16(txtVRFHeatSetpoint.Text);
                Task.Factory.StartNew(() => VRFModbus.SetWinterMode(heat_setpoint_vrf));
                vrfState = VRFState.HEATING;
                string _msg = "Operatori: " + _operator + " e vendosi VRF ne modin manual te ngrohjes! Temperatura e deshiruar: "+heat_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception)
            {
                //Log erros
            }
        }

        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Auto logic
                vrfState = VRFState.AUTO;
                vrf_changed = false; //If needed it will change the season
                string _msg = "Operatori: " + _operator + " e vendosi VRF ne modin AUTOMATIK!";
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception)
            {
                  //Log errors
            }
        }

        private void BtnOff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() => VRFModbus.TurnOFF());
                vrfState = VRFState.OFF;
                string _msg = "Operatori: " + _operator + " e deaktivizoj VRF-n!";
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception ex)
            {
                //Log errors;
            }
        }

        private void BtnFtohje_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cool_setpoint_vrf = Convert.ToUInt16(txtVRFCoolSetpoint.Text);
                Task.Factory.StartNew(() => VRFModbus.SetSummerMode(cool_setpoint_vrf));
                vrfState = VRFState.COOLING;
                string _msg = "Operatori: " + _operator + " e vendosi VRF ne modin manual te ftohjes! Temperatura e deshiruar: " + cool_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);

            }
            catch (Exception ex)
            {
                //Log error
            }
        }

        private void BtnIncVRFHeatSetpoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                heat_setpoint_vrf++;
                txtVRFHeatSetpoint.Text = heat_setpoint_vrf.ToString();
                VRFModbus.Setpoint(heat_setpoint_vrf);
                string _msg = "Operatori: " + _operator + " ndryshoi temperaturen e deshiruar gjate NGROHJES ne VRF. Vlera e re: " + heat_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception)
            {
                //Log error
            }

        }

        private void BtnDecVRFHeatSetpoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                heat_setpoint_vrf--;
                txtVRFHeatSetpoint.Text = heat_setpoint_vrf.ToString();
                VRFModbus.Setpoint(heat_setpoint_vrf);
                string _msg = "Operatori: " + _operator + " ndryshoi temperaturen e deshiruar gjate NGROHJES ne VRF. Vlera e re: " + heat_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch (Exception)
            {
                //
            }
        }

        private void BtnIncVRFCoolSetpoint_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                cool_setpoint_vrf++;
                txtVRFCoolSetpoint.Text = cool_setpoint_vrf.ToString();
                VRFModbus.Setpoint(cool_setpoint_vrf);
                string _msg = "Operatori: " + _operator + " ndryshoi temperaturen e deshiruar gjate FTOHJES ne VRF. Vlera e re: " + heat_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch
            {

            }
        }

        private void BtnDecVRFCoolSetpoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cool_setpoint_vrf--;
                txtVRFCoolSetpoint.Text = cool_setpoint_vrf.ToString();
                VRFModbus.Setpoint(cool_setpoint_vrf);
                string _msg = "Operatori: " + _operator + " ndryshoi temperaturen e deshiruar gjate FTOHJES ne VRF. Vlera e re: " + heat_setpoint_vrf.ToString();
                (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, _operator, _msg);
            }
            catch
            {

            }
        }


        #endregion

        #region ORARET
        private void BtnConfigDite_Click(object sender, RoutedEventArgs e)
        {
            (new Guis.Oraret.OrariDitaConfig()).ShowDialog();
        }

        private void BtnMenaxhoDitet_Click(object sender, RoutedEventArgs e)
        {
            (new Guis.Oraret.MenaxhimiDiteveOraret()).ShowDialog();
        }

        private void BtnOraret_Click(object sender, RoutedEventArgs e)
        {
            (new Guis.Oraret.OraretConfig()).ShowDialog();
        }
        #endregion

        #region TRENDS
        private void MenuArchivingTags_Click(object sender, RoutedEventArgs e)
        {
            (new Guis.Tags.TagTrend()).Show();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Salla1.IsSelected && trend[0])
            {
                UpdatePlot(1);
            }
            else if (Salla2.IsSelected && trend[1])
            {
                UpdatePlot(2);
            }
            else if (Salla3.IsSelected && trend[2])
            {
                UpdatePlot(3);
            }
        }

        private void UpdatePlot(int _salla)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                switch (_salla)
                {
                    case 1:
                        {
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            try
                            {
                                viewModelSalla = null;
                                DateTime from = DateTime.Now;
                                from = from.AddDays(-1);

                                DateTime to = DateTime.Now;
                                viewModelSalla = new TrendBrowserViewModelMT(3, from, to);
                                DataContext = viewModelSalla;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        break;
                    case 2:
                        {
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            try
                            {
                                viewModelSalla = null;
                                DateTime from = DateTime.Now;
                                from = from.AddDays(-1);

                                DateTime to = DateTime.Now;
                                viewModelSalla = new TrendBrowserViewModelMT(2, from, to);
                                DataContext = viewModelSalla;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        break;
                    case 3:
                        {
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            try
                            {
                                viewModelSalla = null;
                                DateTime from = DateTime.Now;
                                from = from.AddDays(-1);

                                DateTime to = DateTime.Now;
                                viewModelSalla = new TrendBrowserViewModelMT(1, from, to);
                                DataContext = viewModelSalla;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        break;
                    default:
                        break;
                }
            }));

        }

        private void BtnTrendS1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (trend[0])
                {
                    Uri resourceUri = new Uri("Resources/TrendOFF.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS1.Background = brush;
                    DataContext = null;
                }
                else
                {
                    Uri resourceUri = new Uri("Resources/TrendON.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS1.Background = brush;
                    UpdatePlot(3);
                }
                trend[0] = !trend[0];

            }
            catch (Exception)
            {

            }
        }

        private void BtnTrendS2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (trend[1])
                {
                    Uri resourceUri = new Uri("Resources/TrendOFF.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS2.Background = brush;
                    DataContext = null;
                }
                else
                {
                    Uri resourceUri = new Uri("Resources/TrendON.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS2.Background = brush;
                    UpdatePlot(2);
                }
                trend[1] = !trend[1];

            }
            catch (Exception)
            {

            }
        }

        private void BtnTrendS3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (trend[2])
                {
                    Uri resourceUri = new Uri("Resources/TrendOFF.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS3.Background = brush;
                    DataContext = null;
                }
                else
                {
                    Uri resourceUri = new Uri("Resources/TrendON.png", UriKind.Relative);
                    StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                    BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                    var brush = new ImageBrush();
                    brush.ImageSource = temp;
                    btnTrendS3.Background = brush;
                    UpdatePlot(1);
                }
                trend[2] = !trend[2];

            }
            catch (Exception)
            {

            }
        }



        #endregion

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Per info kontakto: ejup.yup@hotmail.com");
        }
    }
}
