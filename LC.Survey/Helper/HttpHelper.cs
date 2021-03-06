﻿using CDO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;

namespace LC.Survey.Helper
{
  public  class HttpHelper
    {
        private const string sContentType = "application/x-www-form-urlencoded";
        private const string sUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        public static string Send(string data, string url)
        {
            return Send(Encoding.GetEncoding("UTF-8").GetBytes(data), url);
        }

        public static string Send(byte[] data, string url)
        {
            Stream responseStream;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }
            // request.UserAgent = sUserAgent;  
            request.ContentType = sContentType;
            request.Method = "POST";
            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            try
            {
                responseStream = request.GetResponse().GetResponseStream();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            string str = string.Empty;
            using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
            {
                str = reader.ReadToEnd();
            }
            responseStream.Close();
            return str;
        }
        public static string Send(string data, string url, string contentType)
        {
            return Send(Encoding.GetEncoding("UTF-8").GetBytes(data), url, contentType);
        }
        public static string Send(byte[] data, string url, string contentType)
        {
            Stream responseStream;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }
            // request.UserAgent = sUserAgent;  
            request.ContentType = contentType;
            request.Method = "POST";
            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            try
            {
                responseStream = request.GetResponse().GetResponseStream();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            string str = string.Empty;
            using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
            {
                str = reader.ReadToEnd();
            }
            responseStream.Close();
            return str;
        }
        #region GET/POST调用方法
        public static string HttpGet(string url)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ContentType = "application/json";
            webRequest.Timeout = 5000;
            string result = string.Empty;
            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
                    result = reader.ReadToEnd();

                    reader.Close();
                    reader.Dispose();

                }
            }
            return result;
        }
        /// <summary>
        /// HTTP GET调用方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parm">参数</param>
        /// <returns></returns>
        public static string HttpGet(string url, string parm)
        {
            if (!string.IsNullOrEmpty(parm)) url = url + "?" + parm;
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ContentType = "application/json";
            webRequest.Timeout = 5000;
            string result = string.Empty;
            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
                    result = reader.ReadToEnd();

                    reader.Close();
                    reader.Dispose();

                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parm"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string HttpGet(string url, string parm, string contentType)
        {
            System.GC.Collect();
            if (!string.IsNullOrEmpty(parm)) url = url + "?" + parm;
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ContentType = contentType;
            webRequest.KeepAlive = false;
            string result = string.Empty;
            HttpWebResponse response = null;
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            try
            {
                response = webRequest.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
                result = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

            }
            catch (Exception ex)
            {
                result = ex.Message;
                // LogUtil.WriteLog("HttpGet失败：" + ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (webRequest != null)
                {
                    webRequest.Abort();
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpPost(string data, string url)
        {
            return HttpPost(Encoding.GetEncoding("UTF-8").GetBytes(data), url, sContentType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string HttpPost(string data, string url, string contentType)
        {
            return HttpPost(Encoding.GetEncoding("UTF-8").GetBytes(data), url, contentType);
        }
        public static string HttpPost(string url, string data, string encoding, string contentType)
        {
            return HttpPost(Encoding.GetEncoding(encoding).GetBytes(data), url, contentType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string HttpPost(byte[] data, string url, string contentType)
        {
            Stream responseStream;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }
            // request.UserAgent = sUserAgent;  
            request.ContentType = contentType;
            request.Method = "POST";
            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            try
            {
                responseStream = request.GetResponse().GetResponseStream();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            string str = string.Empty;
            using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
            {
                str = reader.ReadToEnd();
            }
            responseStream.Close();
            return str;
        }
        /// <summary>
        /// 带HEADER的POST
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <param name="dit"></param>
        /// <returns></returns>
        public static string HttpPost(string data, string url, string contentType, Dictionary<string, string> dit)
        {
            return HttpPost(Encoding.GetEncoding("UTF-8").GetBytes(data), url, contentType, dit);
        }
        public static string HttpPost(byte[] data, string url, string contentType, Dictionary<string, string> dit)
        {
            Stream responseStream;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }
            // request.UserAgent = sUserAgent;  
            if (dit != null)
            {
                foreach (KeyValuePair<string, string> kvp in dit)
                {
                    request.Headers.Add(kvp.Key, kvp.Value);
                }
            }
            request.ContentType = contentType;
            request.Method = "POST";
            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            try
            {
                responseStream = request.GetResponse().GetResponseStream();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            string str = string.Empty;
            using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
            {
                str = reader.ReadToEnd();
            }
            responseStream.Close();
            return str;
        }
        #endregion
        #region 发邮件
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailTo">要发送的邮箱</param>
        /// <param name="mailSubject">邮箱主题</param>
        /// <param name="mailContent">邮箱内容</param>
        /// <returns>返回发送邮箱的结果</returns>
        public static bool SendEmail(string mailTo, string mailSubject, string mailContent)
        {

            // 设置发送方的邮件信息,例如使用网易的smtp
            string mailFrom = "kaibin.xu@lead2win.com.cn"; //登陆用户名
            string userPassword = "1q2w3e4r,.";//登陆密码
            string[] ArrMail = mailTo.Split(';');
            //声明一个Mail对象
            MailMessage mymail = new MailMessage();
            //发件人地址
            //如是自己，在此输入自己的邮箱
            mymail.From = new MailAddress(mailFrom);
            //收件人地址
            for (int i = 0; i < ArrMail.Length; i++)
            {
                mymail.To.Add(new MailAddress(ArrMail[i]));
            }
            //邮件主题
            mymail.Subject = mailSubject;
            //邮件标题编码
            mymail.SubjectEncoding = System.Text.Encoding.UTF8;
            //发送邮件的内容
            mymail.Body = mailContent;
            //邮件内容编码
            mymail.BodyEncoding = System.Text.Encoding.UTF8;

            //是否是HTML邮件
            mymail.IsBodyHtml = true;
            //邮件优先级
            mymail.Priority = MailPriority.High;
            //创建一个邮件服务器类
            SmtpClient myclient = new SmtpClient();
            myclient.Host = "smtp.163.com";
            //SMTP服务端口
            myclient.Port = 465;
            //验证登录
            myclient.Credentials = new NetworkCredential(mailFrom, userPassword);//"@"输入有效的邮件名, "*"输入有效的密码

            try
            {
                myclient.Send(mymail); // 发送邮件
                return true;
            }
            catch (SmtpException ex)
            {
                LogHelper.WriteLog(string.Format("邮件发送失败：{0}", ex.Message));
                return false;
            }
        }
        public static bool SendEmail(string mailSubject, string mailContent)
        {

            // 设置发送方的邮件信息,例如使用网易的smtp
            string mailFrom = "kaibin.xu@lead2win.com.cn"; //登陆用户名
            string userPassword = "1q2w3e4r,.";//登陆密码
                                               // string[] ArrMail = mailTo.Split(';');
                                               //声明一个Mail对象
            MailMessage mymail = new MailMessage();
            //发件人地址
            //如是自己，在此输入自己的邮箱
            mymail.From = new MailAddress(mailFrom);
            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\EmailList.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/email");
            //收件人地址
            foreach (XmlNode node in nodeList)
            {
                mymail.To.Add(new MailAddress(node.InnerText));
            }
            //邮件主题
            mymail.Subject = mailSubject;
            //邮件标题编码
            mymail.SubjectEncoding = System.Text.Encoding.UTF8;
            //发送邮件的内容
            mymail.Body = mailContent;
            //邮件内容编码
            mymail.BodyEncoding = System.Text.Encoding.UTF8;

            //是否是HTML邮件
            mymail.IsBodyHtml = true;
            //邮件优先级
            mymail.Priority = MailPriority.High;

            //创建一个邮件服务器类
            SmtpClient myclient = new SmtpClient();
            myclient.DeliveryMethod = SmtpDeliveryMethod.Network;
            myclient.EnableSsl = true;
            myclient.Host = "smtp.exmail.qq.com";
            //SMTP服务端口
            myclient.Port = 465;
            myclient.UseDefaultCredentials = true;
            //验证登录
            myclient.Credentials = new NetworkCredential(mailFrom, userPassword);//"@"输入有效的邮件名, "*"输入有效的密码

            try
            {
                myclient.Send(mymail); // 发送邮件
                return true;
            }
            catch (SmtpException ex)
            {
                LogHelper.WriteLog(string.Format("邮件发送失败：{0}", ex.Message));
                return false;
            }
        }
       
        public static void SendEmailBy465(string subject, string body)
        {
            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\EmailList.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/email");
            StringBuilder to = new StringBuilder();
            foreach (XmlNode node in nodeList)
            {
                to.AppendFormat("{0};", node.InnerText);
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
                //SendMailForSSL(subject, body, toMail);
                LogHelper.WriteLog(string.Format("465邮件发送失败：{0}", ex.Message));
                //失败，错误信息：ex.Message;  
            }
        }
        //public static void SendMailForSSL(string subject, string body, string to)
        //{
        //    try
        //    {
        //        CDO.Message oMsg = new CDO.Message();
        //        Configuration conf = new ConfigurationClass();
        //        conf.Fields[CdoConfiguration.cdoSendUsingMethod].Value = CdoSendUsing.cdoSendUsingPort;
        //        conf.Fields[CdoConfiguration.cdoSMTPAuthenticate].Value = CdoProtocolsAuthentication.cdoBasic;
        //        conf.Fields[CdoConfiguration.cdoSMTPUseSSL].Value = true;
        //        conf.Fields[CdoConfiguration.cdoSMTPServer].Value = "smtp.exmail.qq.com";//必填，而且要真实可用     
        //        conf.Fields[CdoConfiguration.cdoSMTPServerPort].Value = 465;//465特有  
        //        conf.Fields[CdoConfiguration.cdoSendEmailAddress].Value = "kaibin.xu@lead2win.com.cn";
        //        conf.Fields[CdoConfiguration.cdoSendUserName].Value = "kaibin.xu@lead2win.com.cn";//真实的邮件地址     
        //        conf.Fields[CdoConfiguration.cdoSendPassword].Value = "1q2w3e4r,.";   //为邮箱密码，必须真实     


        //        conf.Fields.Update();

        //        oMsg.Configuration = conf;

        //        oMsg.TextBody = body;

        //        oMsg.Subject = subject;
        //        oMsg.From = "kaibin.xu@lead2win.com.cn";
        //        oMsg.To = to;
        //        //ADD attachment.  
        //        //TODO: Change the path to the file that you want to attach.  
        //        //oMsg.AddAttachment("C:\Hello.txt", "", "");  
        //        //oMsg.AddAttachment("C:\Test.doc", "", "");  
        //        oMsg.Send();
        //    }
        //    catch (System.Net.Mail.SmtpException ex)
        //    {

        //        LogHelper.WriteLog("SendMailForSSL error:" + ex.Message);
        //        //throw ex;
        //    }
        //}
        #endregion
        #region SMS
        public static void SendSMS(string contents)
        {
            contents = "【连卡佛】" + contents + "\n/回TD退订";
            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\EmailList.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/mobile");
            StringBuilder mobiles = new StringBuilder();
            foreach (XmlNode node in nodeList)
            {
                mobiles.AppendFormat("{0},", node.InnerText);
            }
            string sendUrl = "http://sdk4rptws.eucp.b2m.cn:8080/sdkproxy/sendsms.action";
            string sendKey = "6SDK-EMY-6688-KCZLS";
            string sendPwd = "304304";
            string param = "cdkey={0}&password={1}&phone={2}&message={3}&addserial=&smspriority=3";
            param = string.Format(param, sendKey, sendPwd, mobiles.ToString().TrimEnd(','), contents);

            string result = HttpPost(param, sendUrl, "application/x-www-form-urlencoded");
            LogHelper.WriteLog(string.Format("sms发送结果：{0}", result));
        }
        #endregion
        #region 校验是否未yyyy年MM月dd日
        public static bool IsDateTime(string date)
        {
            return Regex.IsMatch(date, "^((?<year>\\d{2,4})年)?(?<month>\\d{1,2})月((?<day>\\d{1,2})日)?$");
        }
        #endregion
    }
}
