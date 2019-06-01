using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Helper
{
   public class LogHelper
    {
        private static readonly object writeFile = new object();
        public static void WriteLog(string msg)
        {
            lock (writeFile)
            {
                FileStream fs = null;
                StreamWriter sw = null;

                try
                {
                    String path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    path = string.Format(@"{0}Log\{1}\", path, DateTime.Now.ToString("yyyy-MM-dd"));//按时间格式保存日志文件
                    string filename = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    //服务器中日志目录
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    fs = new FileStream(path + "/" + filename, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine(DateTime.Now.ToString() + "     " + msg + "\r\n");
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Flush();
                        sw.Dispose();
                        sw = null;
                    }
                    if (fs != null)
                    {
                        //     fs.Flush();
                        fs.Dispose();
                        fs = null;
                    }
                }
            }
        }
    }
}
