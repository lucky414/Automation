using AutoSMS.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AutoSMS
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task task1 = new Task((task) =>
            {
                DataParser job = new DataParser();
                job.Init();
            }, 1);
            task1.Start();
        }

        protected override void OnStop()
        {
            LogUtil log = new LogUtil();
            log.WriteLog("Service Stop");
        }
    }
}
