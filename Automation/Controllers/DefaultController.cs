using Automation.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
            //string file = Server.MapPath("~/Files/Survey_SMS_CN_Vendor_10_2018_B1.csv");
            //DataTable dt = GetDataFromCSV(file);
            //int i = 0;
            //int j = 0;
            //foreach (DataRow dr in dt.Rows)
            //{
            //    try
            //    {
            //        if (IsDateTime(dr["Expired date"].ToString()))
            //        {
            //            j++;
            //        }
            //        else
            //        {
            //            i++;
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        i++;
                    
            //    }

            //}
            //string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />生成短链接失败数量：{3}<br />重复数量：{4}<br />发送数量：{5}<br />发送时间：{6}", "6515", "0", "0", "0", "0", "6515", "2018/7/14 10:00:00");
            ////httper.SendByFocusSend("Post Purchase Survey Auto SMS Notification", body);
            ////string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", totalCount.ToString(), u.ToString(), i.ToString(), j.ToString(), m.ToString(), n.ToString(), sendTime);
            ////httper.SendSMS(contents);
            ////string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", "6515", "0", "0", "0", "0", "6515", "2018/7/14 10:00:00");
            ////contents = "【连卡佛】" + contents + "\n/回TD退订";
            ////string sendUrl = "http://sdk4rptws.eucp.b2m.cn:8080/sdkproxy/sendsms.action";
            ////string sendKey = "6SDK-EMY-6688-KCZLS";
            ////string sendPwd = "304304";
            ////string mobile = "18621727650,18601797523,13162231668,13880737877";
            ////string param = "cdkey={0}&password={1}&phone={2}&message={3}&addserial=&smspriority=3";
            ////param = string.Format(param, sendKey, sendPwd, mobile, contents);

            ////string result = HttpsPost(param, sendUrl);
            //List<string> to = new List<string>();
            //to.Add("kaibin.xu@lead2win.com.cn");
            //to.Add("bob.zhang@lead2win.com.cn");
            //to.Add("FrancescaTan@lanecrawford.com");
            //to.Add("RitaDeng@lanecrawford.com");
            //SendEmail(to, "Post Purchase Survey Auto SMS Notification", body);

            return Content("ok");
            /// return View();
        }
        public ActionResult Test()
        {

            string smsid = DateTime.Now.ToString("yyyyMMddHHmmss");
            DateTime now = DateTime.Parse(DateTime.Now.ToShortDateString());
            DateTime startWeek = now.AddDays(1 - Convert.ToInt32(now.DayOfWeek.ToString("d")));  //本周周一  
            DateTime sendTime = startWeek.AddDays(5).AddHours(10);  //本周六10点 
            var totalList = repository.GetAll<LC_AUTOFILE>().Where(t=>t.FILENAME== "Survey_SMS_CN_Vendor_01_2019_B1.csv").ToList();
            int totalCount = totalList.Count;
            int i = 0; // 无效数据
            int j = 0;
            var files = totalList;
            var urls = repository.GetAll<LC_SHORTURL>().Where(t => t.URL_FILENAME == "Survey_SMS_CN_Vendor_01_2019_B1.csv").ToList();

            // 提交发送
            var td = repository.GetAll<TD_List>().Select(t => t.Mobile).ToList();
            var sendcount = urls.FindAll(t => !td.Contains(t.URL_MOBILE));
            // td 名单中数据
            var inTds = urls.Count - sendcount.Count;

            // 发送列表
            var sendList = sendcount.GroupBy(t => t.URL_MOBILE).Select(t => new { t.Key, Copy = t.Max(p => p.URL_COPY) }).ToList();
            //重复数据
            var repeats = sendcount.Count - sendList.Count;

            DateTime time = DateTime.Now;
            string sendTitle = "Automation" + DateTime.Now.ToString("yyyyMM");
            string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />生成短链接失败数量：{3}<br />重复数量：{4}<br />发送数量：{5}<br />发送时间：{6}", totalCount.ToString(), inTds.ToString(), i.ToString(), j.ToString(), repeats.ToString(), sendList.Count.ToString(), sendTime);
            string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", totalCount.ToString(), inTds.ToString(), i.ToString(), j.ToString(), repeats.ToString(), sendList.Count.ToString(), sendTime);
            return Content($"{body}；{contents}");
        }
        public bool IsDateTime(string date)
        {
            return Regex.IsMatch(date, "^((?<year>\\d{2,4})年)?(?<month>\\d{1,2})月((?<day>\\d{1,2})日)?$");
        }
        private DataTable GetDataFromCSV(string path)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                string strTitle = sr.ReadLine();

                string[] strColumTitle = strTitle.Split(',');   //CVS 文件默认以逗号隔开
                int column = strColumTitle.Length;
                for (int i = 0; i < column; i++)
                {

                    dt.Columns.Add(strColumTitle[i]);

                }

                while (!sr.EndOfStream)
                {

                    string strTest = sr.ReadLine();

                    string[] strTestAttribute = strTest.Split(',');

                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < column; i++)
                    {
                        dr[strColumTitle[i]] = strTestAttribute[i];


                    }
                    dt.Rows.Add(dr);



                }
            }
            return dt;

        }
        public  void SendEmail(List<string> mailTo,string subject, string body)
        {

            StringBuilder to = new StringBuilder();
            foreach (string node in mailTo)
            {
                to.AppendFormat("{0};", node);
            }
            string toMail = to.ToString().TrimEnd(';');
            //微软自带的System.Net.Mail不支持QQ邮箱这样的加密的SSL，授权码，
            System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
            try
            {
                mail.To = toMail;//以分号分隔的收件人的地址列表
                mail.From = "kaibin.xu@lead2win.com.cn";
                mail.Subject = subject;
                mail.BodyFormat = System.Web.Mail.MailFormat.Html;
                mail.Body = body;

                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); //身份验证  
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", mail.From); //邮箱登录账号，这里跟前面的发送账号一样就行  
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "1q2w3e4r,."); //这个密码要注意：如果是一般账号，要用授权码；企业账号用登录密码  
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", 465);//端口  
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");//SSL加密  
                System.Web.Mail.SmtpMail.SmtpServer = "smtp.exmail.qq.com";    //企业账号用smtp.exmail.qq.com  
                System.Web.Mail.SmtpMail.Send(mail);

                //邮件发送成功  
            }
            catch (Exception ex)
            {
              //  SendMailForSSL(subject, body, toMail);
                LogUtil.WriteLog(string.Format("465邮件发送失败：{0}", ex.Message));
                //失败，错误信息：ex.Message;  
            }
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