using Automation.Models;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Automation
{
    public class AutoJob : IJob
    {
        private IRepository.IRepository repository;
        public AutoJob()
        {
            repository = new Repository.Repository(new ConnectionString());
        }
        public void Execute()
        {

            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\Schedule.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/date");
            List<string> dates = new List<string>();
            foreach (XmlNode node in nodeList)
            {
                dates.Add(node.InnerText);
            }
            string name = DateTime.Now.ToString("yyyy-MM-dd");
            LogUtil.Info("Task", "Task开始执行");
            if (dates.Contains(DateTime.Now.ToString("yyyy-MM-dd")))
            {
                string filename = string.Format("Survey_SMS_CN_Vendor_{0}_B1.csv", DateTime.Now.ToString("MM_yyyy"));
                int count = repository.Count<Models.LC_AUTOFILE>(x => x.FILENAME.ToLower().Equals(filename.ToLower()));
                if (count == 0)
                {
                    GetSendList();
                }

            }

        }
        private string ShortUrl(string longUrl)
        {
            string api_url = "http://l-c.co/yourls-api.php";
            string short_url = string.Empty;
            string param = string.Format("username={0}&password={1}&action={2}&url={3}&title={4}&format={5}", "lead2win", "bu9OtPz9", "shorturl", HttpUtility.UrlEncode(longUrl.Trim()), "lead2win", "simple");

            try
            {
                short_url = HttpUtil.HttpGet(api_url, param, "application/x-www-form-urlencoded");
            }
            catch (Exception ex)
            {
                short_url = string.Empty;
                LogUtil.Error("AutoJob.ShortUrl", string.Format("{0}，生成短链接失败：{1}", longUrl, ex.Message));

            }
            return short_url;
        }
        private void GetSendList()
        {
            FtpTools ftp = new FtpTools();
            string[] file = ftp.GetFileList(FtpConfig.ftpFilePath + DateTime.Now.ToString("yyyyMM") + "/");
            if (file == null)
            {
                HttpUtil.SendByFocusSend("Post Purchase Survey Auto SMS Notification", "FTP上未发现文件");
                HttpUtil.SendSMS("FTP上未发现文件");
            }
            else
            {
                string filename = string.Format("Survey_SMS_CN_Vendor_{0}_B1.csv", DateTime.Now.ToString("MM_yyyy"));

                bool exists = false;
                foreach (string name in file.ToList())
                {
                    if (name.Equals(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    string info = string.Empty;
                    string filePath = GetFilePath("~/Files/" + DateTime.Now.ToString("yyyyMM"));
                    bool downresult = ftp.Download(filePath, filename, FtpConfig.ftpFilePath + DateTime.Now.ToString("yyyyMM") + "/", out info);

                    if (downresult)
                    {
                        DataTable cstData = GetDataFromCSV(filePath + "/" + filename);
                        if (cstData != null && cstData.Rows.Count > 0)
                        {
                            try
                            {
                                SaveData(cstData, filename);
                            }
                            catch (Exception ex)
                            {
                                LogUtil.Error("SaveData", "error:" + ex.Message);
                            }
                            
                        }
                        else
                        {
                            HttpUtil.SendByFocusSend("Post Purchase Survey Auto SMS Notification", "自动化文件数据为空");
                            HttpUtil.SendSMS("自动化文件数据为空");
                        }


                    }
                }

            }
        }
        private void GetDataFromExcel(string path)
        {
            DataTable dt = ExcelHelper.ExcelToTableForXLSX(path);
        }
        private void SaveData(DataTable dt, string filename)
        {
            string smsid = DateTime.Now.ToString("yyyyMMddHHmmss");
             DateTime sendTime = DateTime.Parse(DateTime.Now.AddDays(2).ToShortDateString()).AddHours(10);
            int totalCount = dt.Rows.Count;
            int i = 0;
            int j = 0;
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    if (dr.IsNull("print_name") || dr.IsNull("URL") || dr.IsNull("Expired date"))
                    {
                        i++;
                        //空值不处理
                    }
                    else
                    {
                        if (HttpUtil.IsDateTime(dr["Expired date"].ToString()))
                        {

                            string shorturl = ShortUrl(dr["URL"].ToString());
                            if (!string.IsNullOrEmpty(shorturl))
                            {
                                string copy = string.Format(CopyConfig.SC, dr["print_name"].ToString(), dr["Expired date"].ToString(), shorturl);
                                Models.LC_AUTOFILE auto = new LC_AUTOFILE();
                                auto.print_name = dr["print_name"].ToString();
                                auto.URL = dr["URL"].ToString();
                                auto.Short_Url = shorturl;
                                auto.phone_no = dr["phone_no"].ToString();
                                auto.Expired_date = dr["Expired date"].ToString();
                                auto.FILENAME = filename;
                                auto.CreateDate = DateTime.Now;
                                auto.Copy = copy;
                                repository.Add<Models.LC_AUTOFILE>(auto);
                                repository.UnitOfWork.SaveChanges();
                            }
                            else
                            {
                                j++;
                            }

                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    i++;
                    LogUtil.Error("SaveData", string.Format("{0}，写入数据失败：{1}", dr["phone_no"], ex.Message));
                }

            }
            // 提交发送
            var td = repository.GetAll<Models.TD_List>();
            var sendcount = repository.GetQuery<Models.LC_AUTOFILE>(x => x.FILENAME.Equals(filename, StringComparison.OrdinalIgnoreCase));

            

            var unique = from p in sendcount group p by p.phone_no into g select new { g.Key, Copy = g.Max(p => p.Copy) };
            int m = sendcount.Count() - unique.Count();
            int wx = unique.Count(g => g.Key.Length != 11);
            var sendlist = from s in unique where !td.Any(ss => ss.Mobile.Equals(s.Key))&&s.Key.Length==11  select s;
            i = i + wx;
            int n = sendlist.Count();
            int u = unique.Count()- n - wx;
            DateTime time = DateTime.Now;
            foreach (var item in sendlist)
            {
                Models.YM_SendList send = new YM_SendList();
                send.Send_AccountId = 1;
                send.Send_Batch = 1;
                send.Send_CheckId = "Auto";
                send.Send_CheckStatus = true;
                send.Send_CheckTime = time;
                send.Send_Contents = item.Copy;
                send.Send_CreateId = 1;
                send.Send_CreateTime = time;
                send.Send_DataStatus = 1;
                send.Send_Flag = 1;
                send.Send_IsOver = true;
                send.Send_IsPersonalized = true;
                send.Send_IsTD = false;
                send.Send_Mobile = item.Key;
                send.Send_NeedReply = false;
                send.Send_QuestionId = 0;
                send.Send_ReplyFlag = false;
                send.Send_ReplyId = 0;
                send.Send_Result = "";
                send.Send_SMSID = long.Parse(smsid);
                send.Send_StartFlag = 0;
                send.Send_Status = false;
                send.Send_SubmitResult = "";
                send.Send_Test = false;
                send.Send_Time = sendTime;
                send.Send_Title = "Automation" + DateTime.Now.ToString("yyyyMM");
                repository.Add<Models.YM_SendList>(send);
            }
            repository.UnitOfWork.SaveChanges();
            string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />生成短链接失败数量：{3}<br />重复数量：{4}<br />发送数量：{5}<br />发送时间：{6}", totalCount.ToString(), u.ToString(), i.ToString(), j.ToString(), m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendByFocusSend("Post Purchase Survey Auto SMS Notification", body);
            string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", totalCount.ToString(), u.ToString(), i.ToString(), j.ToString(), m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendSMS(contents);
        }
        private string GetFilePath(string strPath)
        {
            if (strPath.ToLower().StartsWith("http://"))
            {
                return strPath;
            }
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else //非web程序引用
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\") || strPath.StartsWith("~"))
                {
                    strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }

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
        private string ReplaceStr(string str)
        {

            return str.Replace("\"", "");
        }
    }
}