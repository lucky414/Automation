using Auto_BL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Auto_BL
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Models.LogUtil.WriteLog("Service Start");
            Task.Factory.StartNew(() => {
                SendTask t1 = new SendTask();
                t1.Init();
            });
        }

        protected override void OnStop()
        {
            Models.LogUtil.WriteLog("Service Stop");
        }
    }
}
