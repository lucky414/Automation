using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Helper
{
   public class FtpHelper
    {
        FtpWebRequest reqFTP;
        private string ftpusername = System.Configuration.ConfigurationManager.AppSettings["FtpUserName"];
        private string ftppassword = System.Configuration.ConfigurationManager.AppSettings["FtpPassword"];
        private string ftppath = System.Configuration.ConfigurationManager.AppSettings["FtpPath"];
        private string ftpip = System.Configuration.ConfigurationManager.AppSettings["FtpIP"];
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
                       // LogHelper.WriteLog("file:" + line);
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

                LogHelper.WriteLog("获取FTP文件失败：" + ex.Message);
                return null;
            }
        }
        //上面的代码示例了如何从ftp服务器上获得文件列表  
        public string[] GetFileList(string path)
        {
            return GetFileList("ftp://" + ftpip + "/" + ftppath + path, WebRequestMethods.Ftp.ListDirectory);
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
                string url = "ftp://" + ftpip + "/" + ftppath + folder + fileName;

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

                LogHelper.WriteLog(errorinfo);
                return false;
            }

        }
        public bool DeleteFile(string fileName)
        {
            try
            {
                string url = "ftp://" + ftpip + "/" + ftppath + fileName;
                Connect(url);//连接     
                             //  FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFTP.Credentials = new NetworkCredential(ftpusername, ftppassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除失败：" + ex.Message);
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }
        }
        public void Upload(string filename, string targetPath)
         {
             FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpip + "/" + ftppath+ targetPath+ fileInf.Name;
           
             Connect(uri);//连接        

            // 默认为true，连接不会被关闭

            // 在一个命令之后被执行

            reqFTP.KeepAlive = false;

            // 指定执行什么命令

            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // 上传文件时通知服务器文件的大小

            reqFTP.ContentLength = fileInf.Length;
             // 缓冲大小设置为kb 
             int buffLength = 2048;
             byte[] buff = new byte[buffLength];
 
             int contentLen;
 
             // 打开一个文件流(System.IO.FileStream) 去读上传的文件
 
             FileStream fs = fileInf.OpenRead();
 
             try
 
             {
 
                 // 把上传的文件写入流
                 Stream strm = reqFTP.GetRequestStream();
 
                 // 每次读文件流的kb
                 contentLen = fs.Read(buff, 0, buffLength);
 
                 // 流内容没有结束
                 while (contentLen != 0)
                 {
                     // 把内容从file stream 写入upload stream 
                     strm.Write(buff, 0, contentLen);
                     contentLen = fs.Read(buff, 0, buffLength);
 
                 }
 
                 // 关闭两个流
                 strm.Close();
                 fs.Close();
                reqFTP = null;
 
             }
             catch (Exception ex)
             {
                LogHelper.WriteLog("上传失败：" + ex.Message);
                // MessageBox.Show(ex.Message, "Upload Error");

            }
 
         }
    }
}
