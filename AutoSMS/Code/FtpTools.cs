using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoSMS.Code
{
    public class FtpTools
    {
        FtpWebRequest reqFTP;
        private string ftpusername = System.Configuration.ConfigurationManager.AppSettings["FtpUserName"];
        private string ftppassword = System.Configuration.ConfigurationManager.AppSettings["FtpPassword"];
        private string ftppath = System.Configuration.ConfigurationManager.AppSettings["FtpPath"];
        private string ftpip = System.Configuration.ConfigurationManager.AppSettings["FtpIP"];
        LogUtil log = new LogUtil();
        public FtpTools()
        {

        }
        private void Connect(string url)
        {
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
            // 指定数据传输类型  
            reqFTP.UseBinary = true;
            reqFTP.UsePassive = true;
            // ftp用户名和密码  
            reqFTP.Credentials = new NetworkCredential(ftpusername, ftppassword);
        }
        private string[] GetFileList(string path, string WRMethods)
        {
            // string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            try
            {
                Connect(path);
                reqFTP.Method = WRMethods;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);//中文文件名  
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    return null;
                }
                else
                {

                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    // to remove the trailing '' ''   
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                    reader.Close();
                    response.Close();
                    return result.ToString().Split('\n');
                }
            }
            catch (Exception ex)
            {

                log.WriteLog("获取FTP文件失败：" + ex.Message);
                return null;
            }
        }
        //上面的代码示例了如何从ftp服务器上获得文件列表  
        public string[] GetFileList(string path)
        {
            return GetFileList("ftp://" + ftpip + "/" + ftppath+ path, WebRequestMethods.Ftp.ListDirectory);
        }
        public bool Download(string filePath, string fileName, string folder, out string errorinfo)
        {

            try
            {
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                String onlyFileName = Path.GetFileName(fileName);
                string newFileName = filePath + "\\" + onlyFileName;
                if (File.Exists(newFileName))
                {
                    File.Delete(newFileName);
                    // errorinfo = string.Format("本地文件{0}已存在,无法下载", newFileName);
                    // return false;
                }
                string url = "ftp://" + ftpip + "/" + ftppath+ folder + fileName;

                Connect(url);//连接 
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;

                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                FileStream outputStream = new FileStream(newFileName, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                errorinfo = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorinfo = string.Format("因{0},无法下载", ex.Message);
                log.WriteLog(errorinfo);
                return false;
            }

        }
    }
}
