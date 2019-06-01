using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Helper
{
 public   class FtpClient
    {
        public static string ftpRemotePath;

        #region 变量属性
        /// <summary>
        /// Ftp服务器ip
        /// </summary>
        public static string FtpServerIP = System.Configuration.ConfigurationManager.AppSettings["FtpIP"];
        /// <summary>
        /// Ftp 指定用户名
        /// </summary>
        public static string FtpUserID = System.Configuration.ConfigurationManager.AppSettings["FtpUserName"];
        /// <summary>
        /// Ftp 指定用户密码
        /// </summary>
        public static string FtpPassword = System.Configuration.ConfigurationManager.AppSettings["FtpPassword"];

        public static string ftpURI = "ftp://" + FtpServerIP + "/"+ System.Configuration.ConfigurationManager.AppSettings["FtpPath"];

        #endregion
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="urlpath"></param>
        /// <param name="localpath"></param>
        public static void Download(string urlpath, string localpath)
        {
            FtpWebRequest reqFTP;
            try
            {
                FileStream outputStream = new FileStream(localpath, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + urlpath));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP下载失败：" + ex.Message);
                //MessageBox.Show(ex.ToString());
            }
            
        }
        // 删除文件
        public static void Delete(string urlpath)
        {
            try
            {
                string uri = ftpURI + urlpath;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP删除文件：" + ex.Message);
                // MessageBox.Show(ex.ToString());
            }
        }


        // 删除文件夹
        public static void RemoveDirectory(string urlpath)
        {
            try
            {
                string uri = ftpURI + urlpath;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP删除文件夹：" + ex.Message);
                // MessageBox.Show(ex.ToString());
            }
        }

        //获取指定目录下明细(包含文件和文件夹)
        public static string[] GetFilesDetailList(string urlpath)
        {
            string[] downloadFiles;
            try
            {
                bool getin = false;
                string uri = ftpURI + urlpath;
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftp.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                string line = reader.ReadLine();
                while (line != null)
                {
                    getin = true;
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                if (getin)
                    result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP获取指定目录下明细：" + ex.Message);
                downloadFiles = null;
               // MessageBox.Show(ex.ToString());
                return downloadFiles;
            }
        }

        // 获取指定目录下文件列表(仅文件)
        public static string[] GetFileList(string urlpath, string mask)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                string uri = ftpURI + urlpath;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                    {
                        string mask_ = mask.Substring(0, mask.IndexOf("*"));
                        if (line.Substring(0, mask_.Length) == mask_)
                        {
                            result.Append(line);
                            result.Append("\n");
                        }
                    }
                    else
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP获取指定目录下文件列表(仅文件)：" + ex.Message);
                downloadFiles = null;
                //if (ex.Message.Trim() != "远程服务器返回错误: (550) 文件不可用(例如，未找到文件，无法访问文件)。")
                //{
                //  //  MessageBox.Show(ex.ToString());
                //}
                return downloadFiles;
            }
        }


        // 获取指定目录下所有的文件夹列表(仅文件夹)
        public static string[] GetDirectoryList(string urlpath)
        {
            string[] drectory = GetFilesDetailList(urlpath);
            string m = string.Empty;
            foreach (string str in drectory)
            {
                if (str == "")
                    continue;
                int dirPos = str.IndexOf("<DIR>");
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                {
                    /*判断 Unix 风格*/
                    string dir = str.Substring(54).Trim();
                    if (dir != "." && dir != "..")
                    {
                        m += dir + "\n";
                    }
                }
            }
            if (m[m.Length - 1] == '\n')
                m.Remove(m.Length - 1);
            char[] n = new char[] { '\n' };
            return m.Split(n);   //这样最后一个始终是空格了
        }

        /// 判断指定目录下是否存在指定的子目录
        // RemoteDirectoryName指定的目录名
        public static bool DirectoryExist(string urlpath, string RemoteDirectoryName)
        {
            string[] dirList = GetDirectoryList(urlpath);
            foreach (string str in dirList)
            {
                if (str.Trim() == RemoteDirectoryName.Trim())
                {
                    return true;
                }
            }
            return false;
        }


        // 判断指定目录下是否存在指定的文件
        //远程文件名
        public static bool FileExist(string urlpath, string RemoteFileName)
        {
            string[] fileList = GetFileList(urlpath, "*.*");
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        // 创建文件夹
        public static void MakeDir(string urlpath)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + urlpath));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP创建文件夹：" + ex.Message);
                // MessageBox.Show(ex.ToString());
            }
        }

        // 获取指定文件大小
        public static long GetFileSize(string urlpath)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + urlpath));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP获取指定文件大小：" + ex.Message);
                //  MessageBox.Show(ex.ToString());
            }
            return fileSize;
        }

        // 改名
        public static void ReName(string urlpath, string newname)
        {
            FtpWebRequest reqFTP;
            try
            {
                LogHelper.WriteLog("old:" + ftpURI + urlpath);
                LogHelper.WriteLog("new:" + newname);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + urlpath));  //源路径
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newname; //新名称
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FTP改名：" + ex.Message);
                //   MessageBox.Show(ex.ToString());
            }
        }


        // 移动文件
        public static void MovieFile(string urlpath, string newname)
        {
            ReName(urlpath, newname);
        }

        // 切换当前目录
        /// <param name="IsRoot">true 绝对路径   false 相对路径</param>
        public static void GotoDirectory(string DirectoryName, bool IsRoot)
        {
            if (IsRoot)
            {
                ftpRemotePath = DirectoryName;
            }
            else
            {
                ftpRemotePath += DirectoryName + "/";
            }
            ftpURI = "ftp://" + FtpServerIP + "/" + ftpRemotePath + "/";
        }


    }
}
