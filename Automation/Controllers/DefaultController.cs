using Automation.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
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
            //string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", "7114", "10", "1", "0", "0", "7103", "2018/6/2 10:00:00");
            //contents = "【连卡佛】" + contents + "\n/回TD退订";
            //string sendUrl = "http://sdk4rptws.eucp.b2m.cn:8080/sdkproxy/sendsms.action";
            //string sendKey = "6SDK-EMY-6688-KCZLS";
            //string sendPwd = "304304";
            //string mobile = "18621727650,18601797523,13162231668,13880737877";
            //string param = "cdkey={0}&password={1}&phone={2}&message={3}&addserial=&smspriority=3";
            //param = string.Format(param, sendKey, sendPwd, mobile, contents);

            //string result = HttpsPost(param, sendUrl);

            //return Content(result);
            return View();
        }
        public string HttpsPost(string data, string url)
        {
            HttpWebResponse response = CreatePostHttpResponse(data, url);
            if (response == null)
            {
                return string.Empty;
            }
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string html = sr.ReadToEnd();
            return html;
        }
        private HttpWebResponse CreatePostHttpResponse(string data, string url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(data);
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch
            {
                response = null;
            }
            return response;


        }
    }
}