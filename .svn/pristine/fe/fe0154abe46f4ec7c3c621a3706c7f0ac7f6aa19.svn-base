using Automation.Models;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class ScheduleTask : Registry
    {

        public ScheduleTask()
        {
            string hours = System.Configuration.ConfigurationManager.AppSettings["RunTime"];
            int hh = Int32.Parse(hours.Split(':')[0]);
            int mm = Int32.Parse(hours.Split(':')[1]);
            Schedule<AutoJob>().ToRunNow().AndEvery(1).Days().At(hh, mm);
        }
    }
}