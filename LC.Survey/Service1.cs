using LC.Survey.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.WriteLog("Service Start ");
            ParserHepler parse = new ParserHepler();
            parse.Init();
            parse.Check();
        }

        protected override void OnStop()
        {
            LogHelper.WriteLog("Service Stop ");
        }
    }
}
