using ShortUrl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ShortUrl
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logs.WriteLog("OnStart");
            //CreateShortUrl instance = new CreateShortUrl();
            //instance.Create();
            Task task1 = new Task((task) =>
            {
                CreateShortUrl instance = new CreateShortUrl();
                instance.Init();
            }, 1);
            task1.Start();
        }

        protected override void OnStop()
        {
            Logs.WriteLog("OnStop");
        }
    }
}
