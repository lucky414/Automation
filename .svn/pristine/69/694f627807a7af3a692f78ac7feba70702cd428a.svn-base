using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Automation.Models
{
    public class FtpTools
    {
        private IRepository.IRepository repository;
        FtpWebRequest reqFTP;
        private void Connect(string url)
        {
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
            // 指定数据传输类型  
            reqFTP.UseBinary = true;
            reqFTP.UsePassive = true;
            // ftp用户名和密码  
            reqFTP.Credentials = new NetworkCredential(FtpConfig.ftpUserName, FtpConfig.ftpPassword);
        }
        public FtpTools()
        {
            repository = new Repository.Repository(new ConnectionString());
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

                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr=ex.Message,Log_Name= "获取FTP文件列表",Log_Time=DateTime.Now,Log_Type="FTP",Log_Url= "FtpTools.GetFileList" });
                repository.UnitOfWork.SaveChanges();
                return null;
            }
        }
        //上面的代码示例了如何从ftp服务器上获得文件列表  
        public string[] GetFileList(string path)
        {
            return GetFileList("ftp://" + FtpConfig.ftpUrl + "/" + path, WebRequestMethods.Ftp.ListDirectory);
        }
        //上面的代码示例了如何从ftp服务器上获得文件列表  
        public string[] GetFileList()
        {
            return GetFileList("ftp://" + FtpConfig.ftpUrl + "/", WebRequestMethods.Ftp.ListDirectory);
        }
        //上面的代码实现了从ftp服务器上载文件的功能  
        public void Upload(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + FtpConfig.ftpUrl + "/" + fileInf.Name;
            Connect(uri);//连接               
            // 默认为true，连接不会被关闭 
            // 在一个命令之后被执行  
            reqFTP.KeepAlive = false;
            // 指定执行什么命令  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            // 上传文件时通知服务器文件的大小  
            reqFTP.ContentLength = fileInf.Length;
            // 缓冲大小设置为kb   
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            // 打开一个文件流(System.IO.FileStream) 去读上传的文件  
            FileStream fs = fileInf.OpenRead();
            try
            {
                // 把上传的文件写入流  
                Stream strm = reqFTP.GetRequestStream();
                // 每次读文件流的kb   
                contentLen = fs.Read(buff, 0, buffLength);
                // 流内容没有结束  
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入upload stream   
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                // 关闭两个流  
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "上传文件到FTP", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.Upload" });
                repository.UnitOfWork.SaveChanges();
            }
        }
        /**/
        ////上面的代码实现了从ftp服务器下载文件的功能  
        public bool Download(string filePath, string fileName, string ftpPath, out string errorinfo)
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
                string url = "ftp://" + FtpConfig.ftpUrl + "/" + ftpPath + fileName;

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
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "下载FTP文件", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.Download" });
                errorinfo = string.Format("因{0},无法下载", ex.Message);
                repository.UnitOfWork.SaveChanges();

                return false;
            }

        }
        //删除文件  
        public void DeleteFileName(string fileName)
        {
            try
            {
                FileInfo fileInf = new FileInfo(fileName);
                string uri = "ftp://" + FtpConfig.ftpUrl + "/" + fileInf.Name;
                Connect(uri);//连接              
                // 默认为true，连接不会被关闭  
                // 在一个命令之后被执行  
                reqFTP.KeepAlive = false;
                // 指定执行什么命令  
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "删除FTP文件", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.DeleteFileName" });
                repository.UnitOfWork.SaveChanges();
            }
        }
        //创建目录  
        public void MakeDir(string dirName)
        {
            try
            {
                string uri = "ftp://" + FtpConfig.ftpUrl + "/" + dirName;
                Connect(uri);//连接         
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "创建FTP目录", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.dirName" });
                repository.UnitOfWork.SaveChanges();
            }
        }
        //删除目录  
        public void delDir(string dirName)
        {
            try
            {
                string uri = "ftp://" + FtpConfig.ftpUrl + "/" + dirName;
                Connect(uri);//连接        
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "删除FTP目录", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.delDir" });
                repository.UnitOfWork.SaveChanges();
            }
        }
        //获得文件大小  
        public long GetFileSize(string filename)
        {
            long fileSize = 0;
            try
            {
                FileInfo fileInf = new FileInfo(filename);
                string uri = "ftp://" + FtpConfig.ftpUrl + "/" + fileInf.Name;
                Connect(uri);//连接         
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                fileSize = response.ContentLength;
                response.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "获取FTP文件大小", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.GetFileSize" });
                repository.UnitOfWork.SaveChanges();
            }
            return fileSize;
        }
        //文件改名  
        public void Rename(string currentFilename, string newFilename)
        {
            try
            {
                FileInfo fileInf = new FileInfo(currentFilename);
                string uri = "ftp://" + FtpConfig.ftpUrl + "/" + fileInf.Name;
                Connect(uri);//连接  
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                //Stream ftpStream = response.GetResponseStream(); 
                //ftpStream.Close();  
                response.Close();
            }
            catch (Exception ex)
            {
                repository.Add<Models.LC_ExceptionLog>(new Models.LC_ExceptionLog() { Log_Descr = ex.Message, Log_Name = "更改FTP文件名称", Log_Time = DateTime.Now, Log_Type = "FTP", Log_Url = "FtpTools.Rename" });
                repository.UnitOfWork.SaveChanges();
            }
        }
        //获得文件明细  
        public string[] GetFilesDetailList()
        {
            return GetFileList("ftp://" + FtpConfig.ftpUrl + "/",
WebRequestMethods.Ftp.ListDirectoryDetails);
        }
        //获得文件明细 
        public string[] GetFilesDetailList(string path)
        {
            return GetFileList("ftp://" + FtpConfig.ftpUrl + "/" + path,
WebRequestMethods.Ftp.ListDirectoryDetails);
        }
    }
}