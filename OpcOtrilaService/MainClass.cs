using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OpcOtrilaService 
{
    public partial class MainClass : ServiceBase
    {
        public MainClass()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            InitializeServiceManager();
        }

        private static void InitializeServiceManager()
        {
            try
            {
                OpcManager.StartOpcMasteR(); //Start the master
            }
            catch (Exception) { }
        }

        protected override void OnStart(string[] args)
        {
            //Start the service here
            try
            {
                InitializeServiceManager();
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected override void OnStop()
        {
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }
    }
}
