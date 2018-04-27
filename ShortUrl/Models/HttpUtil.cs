using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShortUrl.Models
{
   public class HttpUtil
    {
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
    }
}
