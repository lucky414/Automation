using Automation.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        private IRepository.IRepository repository;
        public DefaultController()
        {
            repository = new Repository.Repository(new ConnectionString());
        }
        public ActionResult Index()
        {

            int count = 685;
            int ExeCount = count / 10;
            int i = count % 10;
            return View();
        }
      
    }
}