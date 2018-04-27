using AutoSMS.com.focussend.app;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;

namespace AutoSMS.Code
{
   public class HttpUtil
    {
        LogUtil log = new LogUtil();
        private  string sContentType = "application/x-www-form-urlencoded";
        private  string sUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        #region http get
        public  string HttpGet(string url)
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
        public  string HttpGet(string url, string parm)
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
        public  string HttpGet(string url, string parm, string contentType)
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
            catch(Exception ex)
            {
                log.WriteLog("HttpGet失败：" + ex.Message);
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
            //using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            //{
            //    if (response != null && response.StatusCode == HttpStatusCode.OK)
            //    {

            //        Stream stream = response.GetResponseStream();
            //        StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
            //        result = reader.ReadToEnd();

            //        reader.Close();
            //        reader.Dispose();

            //        /**************************************************************
            //         * 如果是图片类型，可以直接下载
            //         * 图片类型ContentType="application/octet-stream"
            //         * Tools.DownLoadPic(response.ResponseUri.AbsoluteUri);
            //         ************************************************************/

            //    }
            //}

        }
        #endregion
        public  bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }
        public  bool IsDateTime(string date)
        {
            return Regex.IsMatch(date, "^((?<year>\\d{2,4})年)?(?<month>\\d{1,2})月((?<day>\\d{1,2})日)?$");
        }
        /// <summary>
        /// 处理http GET请求，返回数据
        /// </summary>
        /// <param name="url">请求的url地址</param>
        /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
        public  string Get(string url)
        {
            System.GC.Collect();
            string result = "";
            LogUtil log = new LogUtil();
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            //请求url以获取数据
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";

                //设置代理
                //WebProxy proxy = new WebProxy();
                //proxy.Address = new Uri(WxPayConfig.PROXY_URL);
                //request.Proxy = proxy;

                //获取服务器返回
                response = (HttpWebResponse)request.GetResponse();

                //获取HTTP返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (Exception ex)
            {
                log.WriteLog("HTTP 调用失败："+ex.Message);
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }
        public  string HttpsPost(string data, string url)
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
        public  HttpWebResponse CreatePostHttpResponse(string data, string url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
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
        public  string HttpsGet(string url)
        {
            HttpWebResponse response = CreateGetHttpResponse(url);
            if (response == null)
            {
                return string.Empty;
            }
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string html = sr.ReadToEnd();
            return html;
        }
        public  HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            HttpWebResponse response = null;
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
        #region SMS
        public  void SendSMS(string contents)
        {
            contents = "【连卡佛】" + contents;
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

            string result = HttpsPost(param, sendUrl);
            log.WriteLog(string.Format("sms发送结果：{0}", result));
        }
        #endregion
        #region sendemail
        public  void SendByFocusSend(string subject, string body)
        {
            FocusSendWebService webService = new FocusSendWebService();
            FocusUser user = new FocusUser();
            user.Email = "kaibin.xu@lead2win.com.cn";

            string pwd = "lead2win";
            string encodedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, FormsAuthPasswordFormat.SHA1.ToString("G"));
            user.Password = encodedPwd;

            FocusEmail email = new FocusEmail();
            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = true;
            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\EmailList.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/email");
            //收件人地址

            FocusTask task = new FocusTask();
            task.TaskName = "Auto" + DateTime.Now.ToString("yyyyMMddhhmmss");
            task.SenderEmail = "Service@lead2win.com.cn";
            task.SenderName = "Service";
            task.ReplyName = "Service";
            task.ReplyEmail = "Service@lead2win.com.cn";
            task.SendDate = DateTime.Now;
            task.Subject = subject;
            List<FocusReceiver> receiverList = new List<FocusReceiver>();
            foreach (XmlNode node in nodeList)
            {
                FocusReceiver receiver = new FocusReceiver();
                receiver.Email = node.InnerText;
                receiverList.Add(receiver);

            }
            try
            {
                string result = webService.BatchSend(user, email, task, receiverList.ToArray());
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("邮件发送失败：{0}", ex.Message));

            }

        }
        #endregion
    }
}
