using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaOtrila.Classes
{
    public static class VRFModbus
    {
        public static string status = "OFF";
        public static int setpoint = 20;

        public static void SetSummerMode(ushort _coolsetpoint)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(Properties.Settings.Default.VRF_Ip, Properties.Settings.Default.VRF_Port);
                ModbusIpMaster ipMaster = ModbusIpMaster.CreateIp(tcpClient);
                byte address = Properties.Settings.Default.slaveAddress;
                ushort addr = 0;
                ushort val = 1; //this value sets default 17*C setpoint, medium speed fan, no timer, heat mode
                ipMaster.WriteSingleRegister(address, addr, val);
                Thread.Sleep(2000);
                addr = 3;
                ipMaster.WriteSingleRegister(address, addr, _coolsetpoint);
                Thread.Sleep(2000);
                ipMaster.Dispose();
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch { }
        }

        public static void SetWinterMode(ushort _heatsetpoint)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(Properties.Settings.Default.VRF_Ip, Properties.Settings.Default.VRF_Port);
                ModbusIpMaster ipMaster = ModbusIpMaster.CreateIp(tcpClient);
                byte address = Properties.Settings.Default.slaveAddress;
                ushort addr = 0;
                ushort val = 5; //this value sets default 26*C setpoint, medium speed fan, no timer, heat mode
                ipMaster.WriteSingleRegister(address, addr, val);
                Thread.Sleep(2000);
                addr = 3;
                ipMaster.WriteSingleRegister(address, addr, _heatsetpoint);
                Thread.Sleep(2000);
                ipMaster.Dispose();
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch { }
        }

        public static void Setpoint(ushort _setpoint)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(Properties.Settings.Default.VRF_Ip, Properties.Settings.Default.VRF_Port);
                ModbusIpMaster ipMaster = ModbusIpMaster.CreateIp(tcpClient);
                byte address = Properties.Settings.Default.slaveAddress;
                ushort addr = 3;
                ipMaster.WriteSingleRegister(address, addr, _setpoint);
                Thread.Sleep(2000);
                setpoint = _setpoint;
                ipMaster.Dispose();
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch { }
        }

        public static void TurnOFF()
        {
            try
            {
                TcpClient tcpClient = new TcpClient(Properties.Settings.Default.VRF_Ip, Properties.Settings.Default.VRF_Port);
                ModbusIpMaster ipMaster = ModbusIpMaster.CreateIp(tcpClient);
                byte address = Properties.Settings.Default.slaveAddress;
                ushort addr = 0;
                ushort val = 0;
                ipMaster.WriteSingleRegister(address, addr, val);
                Thread.Sleep(2000);
                status = "OFF";
                ipMaster.Dispose();
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch { }
        }

        public static void ReadStatus()
        {
            /*
            TcpClient tcpClient = new TcpClient(Properties.Settings.Default.VRF_Ip, Properties.Settings.Default.VRF_Port);
            ModbusIpMaster ipMaster = ModbusIpMaster.CreateIp(tcpClient);
            byte address = Properties.Settings.Default.slaveAddress;
            ushort addr = 0;
            vrfStatus = ipMaster.ReadCoils(address, 2, 2);
            Thread.Sleep(2000);
            if (Properties.OpcSettings.Default.vrfsrvctest)
            {
                string msg = "[heat] =" + vrfStatus[2].ToString() + "\n [cool] = " + vrfStatus[3].ToString() + "\n[ON/OFF] =" + vrfStatus[7].ToString();
                global::System.Windows.Forms.MessageBox.Show(msg);
            }
            ipMaster.Dispose();
            tcpClient.GetStream().Close();
            tcpClient.Close(); */
        }
    }

    public enum VRFState
    {
        OFF = 1,
        HEATING = 2,
        COOLING = 3,
        AUTO = 4
    }
}
