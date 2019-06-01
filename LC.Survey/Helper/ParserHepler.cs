using FluentScheduler;
using LC.Survey.Core;
using LC.Survey.DataModel;
using LC.Survey.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using CDO;
using System.Threading.Tasks;
using System.Web;

namespace LC.Survey.Helper
{
    public class ParserHepler
    {
        private readonly IRepository _respostory;
        private CommonHelper helper;
        private FtpHelper ftp;
        public ParserHepler()
        {
            _respostory = new LC.Survey.Repository.Repository(new BFSMSDBEntities());
            helper = new CommonHelper();
            ftp = new FtpHelper();
        }
        public void Init()
        {
            LogHelper.WriteLog("Init:" + DateTime.Now);
            Registry registry = new Registry();
            registry.Schedule(() => ParseDocument()).ToRunNow().AndEvery(1).Days().At(11, 10);
            JobManager.Initialize(registry);
        }
        private void ParseDocument()
        {
            Parse(false);

        }
        private void Parse(bool notice)
        {
            try
            {
                LogHelper.WriteLog("Parse Start");
                var filelist = CheckFile(notice);
                if (filelist == null || filelist.Length == 0)
                    return;
                var existsfile = _respostory.GetAll<POSTSURVEY_LOG>().Select(t => t.FILENAME).ToList();
                existsfile.Add("bak");
                var ls = filelist.Except(existsfile);
                if (ls.Count() > 0)
                {
                    string info = string.Empty;
                    string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "Files/" + DateTime.Now.ToString("yyyy-MM-dd");
                    var fileName = ls.FirstOrDefault();
                    LogHelper.WriteLog("fileName:" + fileName);
                    bool downresult = ftp.Download(filePath, fileName, "", out info);
                    if (downresult)
                    {
                        _respostory.Add<POSTSURVEY_LOG>(new POSTSURVEY_LOG
                        {
                            DOWNLAODTIME = DateTime.Now,
                            FILENAME = fileName

                        });
                        _respostory.UnitOfWork.SaveChanges();
                        Task.Factory.StartNew(() =>
                        {
                            PrepareToSend(filePath, fileName);
                        });

                    }
                }
                else
                {
                    if (notice)
                    {
                        HttpHelper.SendEmailBy465("Post Purchase Survey Auto SMS Notification", "FTP上未发现文件");
                        HttpHelper.SendSMS("FTP上未发现文件");
                    }
                    LogHelper.WriteLog("未上传新文件");
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Parse：" + ex.Message);
            }

        }
        private ResultView FileDataList(string path)
        {
            int i = 0, j = 0, k = 0;
            var filename = Path.GetFileName(path);
            var dt = ParseCSV(path);
            if (dt.Rows.Count == 0)
                return new ResultView { Total = 0, Invalid = 0, ShortUrlNull = 0, TD = 0, Repeat = 0, Data = new List<POSTSURVEY_FILES>() };
            int total = dt.Rows.Count;
            var list = new List<POSTSURVEY_FILES>();
            foreach (DataRow dr in dt.Rows)
            {
                try
                {

                    if (dr.IsNull("phone_no") || string.IsNullOrWhiteSpace(dr["phone_no"].ToString()))
                    {
                        i++;
                        LogHelper.WriteLog(string.Format("{0}，手机号无效", dr["phone_no"]));
                        continue;
                    }
                    var repeat = list.Any(t => t.phone_no == dr["phone_no"].ToString());
                    if (repeat)
                    {
                        k++;
                        LogHelper.WriteLog(string.Format("{0}，重复", dr["phone_no"]));
                        continue;
                    }
                    if (dr.IsNull("customer_name_shortened_chi") || !HttpHelper.IsDateTime(dr["Expired date"].ToString()))
                    {
                        i++;
                        LogHelper.WriteLog(string.Format("{0}，日期无效", dr["phone_no"]));
                        continue;
                    }
                    if (dr.IsNull("customer_name_shortened_chi") || string.IsNullOrWhiteSpace(dr["customer_name_shortened_chi"].ToString()))
                    {
                        i++;
                        LogHelper.WriteLog(string.Format("{0}，名称无效", dr["phone_no"]));
                        continue;
                    }
                    if (dr.IsNull("URL") || string.IsNullOrWhiteSpace(dr["URL"].ToString()))
                    {
                        i++;
                        LogHelper.WriteLog(string.Format("{0}，URL无效", dr["phone_no"]));
                        continue;
                    }

                    var shortUrl = CreateShortUrl(dr["URL"].ToString().Trim());
                    if (string.IsNullOrEmpty(shortUrl))
                    {
                        j++;
                        LogHelper.WriteLog(string.Format("{0}，生成短链接失败", dr["phone_no"]));
                        continue;
                    }
                    var copy = string.Format("【连卡佛】尊敬的{0}，感谢您近期的光顾！诚邀您于{1}前完成购物体验调查 {2} /回TD退订", dr["customer_name_shortened_chi"].ToString(), dr["Expired date"].ToString(), shortUrl);
                    var filedata = new POSTSURVEY_FILES();
                    filedata.user_id = dr.IsNull("user_id") ? string.Empty : dr["user_id"].ToString();
                    filedata.customer_name_shortened_chi = dr["customer_name_shortened_chi"].ToString();
                    filedata.URL = dr["URL"].ToString();
                    filedata.Short_Url = shortUrl;
                    filedata.Copy = copy;
                    filedata.phone_no = dr["phone_no"].ToString();
                    filedata.DownLoadTime = DateTime.Now;
                    filedata.FileName = filename;
                    list.Add(filedata);
                    _respostory.Add<POSTSURVEY_FILES>(filedata);
                    _respostory.UnitOfWork.SaveChanges();
                }
                catch (Exception ex)
                {
                    i++;
                    LogHelper.WriteLog(string.Format("{0}，写入AUTOFILE失败：{1}", dr["phone_no"], ex.Message));

                }
            }
            return new ResultView { Total = total, Invalid = i, ShortUrlNull = j, TD = 0, Repeat = k, Data = list };
        }
        private DataTable ParseCSV(string path)
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
        private string CreateShortUrl(string longurl)
        {
            try
            {
                string api_url = "http://l-c.co/yourls-api.php";
                string short_url = string.Empty;
                string param = string.Format("username={0}&password={1}&action={2}&url={3}&title={4}&format={5}", "lead2win", "bu9OtPz9", "shorturl", HttpUtility.UrlEncode(longurl.Trim()), "lead2win", "simple");
                short_url = HttpHelper.HttpGet(api_url, param, "application/x-www-form-urlencoded");
                return short_url;
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog(string.Format("生成短链接失败：{0}", ex.Message));
                return string.Empty;
            }

        }
        #region 写入发送
        private void PrepareToSend(string filePath, string fileName)
        {
            LogHelper.WriteLog("准备写入发送数据");
            var list = FileDataList(filePath + "/" + fileName);
            if (list.Total == 0)
            {
                HttpHelper.SendEmailBy465("Post Purchase Survey Auto SMS Notification", "CSV文件中没有数据");
                HttpHelper.SendSMS("CSV文件中没有数据");
            }
            else
            {
                var result = list.Data;
                //var tdCo
                if (result.Count > 0)
                {
                    var td = _respostory.GetAll<TD_List>().Select(t => t.Mobile).ToList();
                    var validList = result.Where(t => !td.Contains(t.phone_no)).Select(t => new { t.phone_no, t.Copy }).ToList();
                    list.TD = result.Count - validList.Count;
                    var spendMinutes = System.Configuration.ConfigurationManager.AppSettings["SendSpend"];
                    var sendTime = DateTime.Now.AddMinutes(Int32.Parse(spendMinutes));
                    var smsid = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    var title = "Automation" + DateTime.Now.ToString("yyyyMMdd");
                    foreach (var item in validList)
                    {
                        YM_SendList send = new YM_SendList();
                        send.Send_AccountId = 1;
                        send.Send_Batch = 1;
                        send.Send_CheckId = "Auto";
                        send.Send_CheckStatus = true;
                        send.Send_CheckTime = DateTime.Now;
                        send.Send_Contents = item.Copy;
                        send.Send_CreateId = 1;
                        send.Send_CreateTime = DateTime.Now;
                        send.Send_DataStatus = 1;
                        send.Send_Flag = 1;
                        send.Send_IsOver = true;
                        send.Send_IsPersonalized = true;
                        send.Send_IsTD = false;
                        send.Send_Mobile = item.phone_no;
                        send.Send_NeedReply = false;
                        send.Send_QuestionId = 0;
                        send.Send_ReplyFlag = false;
                        send.Send_ReplyId = 0;
                        send.Send_Result = "";
                        send.Send_SMSID = smsid;
                        send.Send_StartFlag = 0;
                        send.Send_Status = false;
                        send.Send_SubmitResult = "";
                        send.Send_Test = false;
                        send.Send_Time = sendTime;
                        send.Send_Title = title;
                        _respostory.Add<YM_SendList>(send);
                    }
                    _respostory.UnitOfWork.SaveChanges();

                    LogHelper.WriteLog("解析结束");
                    string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />生成短链接失败数量：{3}<br />重复数量：{4}<br />发送数量：{5}<br />发送时间：{6}", list.Total, list.TD, list.Invalid, list.ShortUrlNull, list.Repeat, validList.Count, sendTime);
                    HttpHelper.SendEmailBy465("Post Purchase Survey Auto SMS Notification", body);
                    string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", list.Total, list.TD, list.Invalid, list.ShortUrlNull, list.Repeat, validList.Count, sendTime);
                    HttpHelper.SendSMS(contents);
                }
                else
                {
                    LogHelper.WriteLog("数据无效");
                    //都是无效数据
                }
                //if()
            }
            ftp.Upload(System.AppDomain.CurrentDomain.BaseDirectory + "Files/" + DateTime.Now.ToString("yyyy-MM-dd")+"/"+fileName, "bak/");
          
            ftp.DeleteFile(fileName);
        }
        #endregion
        #region 校验
        public void Check()
        {
            LogHelper.WriteLog("Check:" + DateTime.Now);
            Registry registry = new Registry();
            registry.Schedule(() => CheckRun()).ToRunEvery(1).Days().At(12, 10);
            JobManager.Initialize(registry);
        }
        private void CheckRun()
        {
            Parse(true);
        }
        private string[] CheckFile(bool notice)
        {
            try
            {
                var exists = _respostory.Count<POSTSURVEY_LOG>(t => SqlFunctions.DateDiff("Day", t.DOWNLAODTIME, DateTime.Now) == 0);
                if (exists == 0)
                {
                    var filelist = ftp.GetFileList("");

                    if ((filelist == null || filelist.Length == 0 || (filelist.Length == 1 && filelist[0] == "bak")) && notice)
                    {
                        LogHelper.WriteLog("FTP上未发现文件");
                        //通知
                        HttpHelper.SendEmailBy465("Post Purchase Survey Auto SMS Notification", "FTP上未发现文件");
                        HttpHelper.SendSMS("FTP上未发现文件");
                        return null;

                    }
                      return filelist;
                }
                else
                {
                    LogHelper.WriteLog($"{DateTime.Now}：已下载");
                    return null;
                    //已发送
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"CheckFile：{ex.Message}");
                return null;
            }

        }
        #endregion
    }
}
