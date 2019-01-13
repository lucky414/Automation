using AutoSMS.DataModel;
using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace AutoSMS.Code
{
  public  class DataParser
    {
        LogUtil log = new LogUtil();
        HttpUtil httper = new HttpUtil();
        private IRepository.IRepository repository;
        public DataParser()
        {
            repository = new Repository.Repository(new DBEntities());
        }
        public void Init()
        {
            log.WriteLog("Task Start");
            var isManual= System.Configuration.ConfigurationManager.AppSettings["IsManual"];
            if (isManual == "1")
            {
                SendByManual();
            }
            else
            {
                string hh = System.Configuration.ConfigurationManager.AppSettings["StartHour"];
                string mm = System.Configuration.ConfigurationManager.AppSettings["StartMinute"];
                Registry registry = new Registry();
                registry.Schedule(() => ParseDocument()).ToRunNow().AndEvery(1).Days().At(Int32.Parse(hh), Int32.Parse(mm));
                JobManager.Initialize(registry);
            }
        }
        public void SendByManual()
        {
            log.WriteLog("Manual Start:"+DateTime.Now);
            // 生成URL
            try
            {
                var filename = System.Configuration.ConfigurationManager.AppSettings["FileName"];
                var shortUrl = repository.GetQuery<LC_SHORTURL>(t => t.URL_FILENAME.Equals(filename, StringComparison.OrdinalIgnoreCase)).Select(t =>new { Mobile=t.URL_MOBILE, Copy=t.URL_COPY} ).ToList();

                var list = repository.GetQuery<LC_AUTOFILE>(t => t.FILENAME.Equals(filename, StringComparison.OrdinalIgnoreCase)&&!shortUrl.Any(c=>c.Mobile == t.phone_no)).ToList();
                var sendTime = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["SendTime"]);
                //var datatable = GetDataFromCSV(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Manual\\" + filename);
                var i = 0;
                var j = 0;
               // var list=  InsertToAutoFile(datatable, filename,ref i);
                var urls=  InsertToShortUrl(list, sendTime,ref j);
                try
                {
                    var td = repository.GetAll<TD_List>().Select(t=>t.Mobile).ToList();
                    var sendlist = urls.Where(t => !td.Contains(t.URL_MOBILE)).GroupBy(t => t.URL_MOBILE).Select(t => new { Mobile=t.Key, Copy = t.Max(c => c.URL_COPY) }).ToList();
                    shortUrl.AddRange(sendlist);
                    //shortUrl = shortUrl.Distinct().ToList();
                    var smsid = DateTime.Now.ToString("yyyyMMddHHmmss");
                    DateTime time = DateTime.Now;
                    string sendTitle = "Automation" + DateTime.Now.ToString("yyyyMM");
                    foreach (var item in shortUrl)
                    {
                        YM_SendList send = new YM_SendList();
                        send.Send_AccountId = 1;
                        send.Send_Batch = 1;
                        send.Send_CheckId = "Auto";
                        send.Send_CheckStatus = false;
                        send.Send_CheckTime = time;
                        send.Send_Contents = item.Copy;
                        send.Send_CreateId = 1;
                        send.Send_CreateTime = time;
                        send.Send_DataStatus = 1;
                        send.Send_Flag = 1;
                        send.Send_IsOver = true;
                        send.Send_IsPersonalized = true;
                        send.Send_IsTD = false;
                        send.Send_Mobile = item.Mobile;
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
                        send.Send_Title = sendTitle;
                        repository.Add<YM_SendList>(send);
                    }
                    repository.UnitOfWork.SaveChanges();
                    log.WriteLog("Manual  End:" + DateTime.Now);
                }
                catch (Exception ex)
                {
                    log.WriteLog("保存失败:" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                log.WriteLog("Manual失败:" + ex.InnerException.Message);
            }
           
           
           
        }
        public void ParseDocument()
        {
            log.WriteLog("Task Run");
            try
            {

                string name = DateTime.Now.ToString("yyyy-MM-dd");
                string thisFolder = DateTime.Now.ToString("yyyyMM") + "/";
                string nextFolder= DateTime.Now.AddMonths(1).ToString("yyyyMM") + "/";
                string thisFileName = string.Format("Survey_SMS_CN_Vendor_{0}_B1.csv", DateTime.Now.ToString("MM_yyyy"));
                string nextFileName= string.Format("Survey_SMS_CN_Vendor_{0}_B1.csv", DateTime.Now.AddMonths(1).ToString("MM_yyyy"));

                int count = repository.Count<DataModel.LC_AUTOFILE>(x => x.FILENAME.ToLower().Equals(thisFileName.ToLower()));
                if (count == 0)
                {
                    ParseData(thisFolder, thisFileName);
                }
                else
                {
                    count = repository.Count<DataModel.LC_AUTOFILE>(x => x.FILENAME.ToLower().Equals(nextFileName.ToLower()));
                    if (count == 0)
                    {
                        ParseData(nextFolder, nextFileName);
                    }
                   
                }

            }
            catch (Exception ex)
            {
                log.WriteLog("ParseDocument error：" + ex.Message);
            }
           
        }
        private void ParseData(string folder, string filename)
        {
            log.WriteLog(string.Format("Parse Data，Folder:{0}，FileName:{1}", folder, filename));
            FtpTools ftp = new FtpTools();
          
            string[] file = ftp.GetFileList(folder);
            if (file != null)
            {
                string ftpFile = file[0];
                if (ftpFile.Equals(filename, StringComparison.OrdinalIgnoreCase))
                {

                    string info = string.Empty;
                    string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "Files/" + folder.TrimEnd('/');
                    bool downresult = ftp.Download(filePath, filename, folder, out info);
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
                                log.WriteLog("数据有误：" + ex.Message);

                            }

                        }
                        else
                        {
                            httper.SendByFocusSend("Post Purchase Survey Auto SMS Notification", "自动化文件数据为空");
                            httper.SendSMS("自动化文件数据为空");
                        }
                    }
                }
            }
            else
            {
                log.WriteLog("没有文件");
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
        private void SaveData(DataTable dt, string filename)
        {
            log.WriteLog("Task  开始解析");
            string smsid = DateTime.Now.ToString("yyyyMMddHHmmss");
            //int Spend_days = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Spend_days"]);
            //DateTime sendTime = DateTime.Parse(DateTime.Now.AddDays(Spend_days).ToShortDateString()).AddHours(10);
            DateTime now = DateTime.Parse(DateTime.Now.ToShortDateString());
            DateTime startWeek = now.AddDays(1 - Convert.ToInt32(now.DayOfWeek.ToString("d")));  //本周周一  
            DateTime sendTime = startWeek.AddDays(5).AddHours(10);  //本周六10点 

            int totalCount = dt.Rows.Count;
            int i = 0; // 无效数据
            int j = 0;
            var files = InsertToAutoFile(dt, filename,ref i);
            var urls = InsertToShortUrl(files, sendTime,ref j);
           
            log.WriteLog(string.Format("生成短链接失败数量：{0}", j.ToString()));
           // int errormobile = totalCount - datalist.Count;
            //i += errormobile;
            log.WriteLog(string.Format("无效数量：{0}",i.ToString()));
            // 提交发送
            var td = repository.GetAll<TD_List>().Select(t=>t.Mobile).ToList();
            var sendcount =urls.FindAll(t=>!td.Contains(t.URL_MOBILE));
            // td 名单中数据
            var inTds = urls.Count - sendcount.Count;
            log.WriteLog(string.Format("td 名单中数据：{0}", inTds.ToString()));
            // 发送列表
            var sendList = sendcount.GroupBy(t => t.URL_MOBILE).Select(t => new { t.Key, Copy = t.Max(p => p.URL_COPY) }).ToList();
            //重复数据
            var repeats = sendcount.Count - sendList.Count;
            log.WriteLog(string.Format("重复数量：{0}", repeats.ToString()));
            // var _uni log.WriteLog(string.Format("重复数量：{0}", m.ToString()));que = from p in sendcount group p by p.URL_MOBILE into g select new { g.Key, Copy = g.Max(p => p.URL_COPY) };
            //  var unique = _unique.ToList();

            //int m = sendcount.Count - unique.Count;
            //log.WriteLog(string.Format("重复数量：{0}", m.ToString()));
            //var _sendlist = from s in unique where !td.Any(ss => ss.Mobile.Equals(s.Key)) select s;
            //var sendlist = _sendlist.ToList();

            //int n = 0, u = 0;
            //n = sendlist.Count();
            log.WriteLog(string.Format("发送数量：{0}", sendList.Count.ToString()));
            //u = unique.Count - n;
           // log.WriteLog(string.Format("TD数量：{0}", u.ToString()));
            DateTime time = DateTime.Now;
            string sendTitle= "Automation" + DateTime.Now.ToString("yyyyMM");
            foreach (var item in sendList)
            {
                YM_SendList send = new YM_SendList();
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
                send.Send_Title = sendTitle;
                repository.Add<YM_SendList>(send);
            }
            repository.UnitOfWork.SaveChanges();
            log.WriteLog("Task  解析结束");
            string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />生成短链接失败数量：{3}<br />重复数量：{4}<br />发送数量：{5}<br />发送时间：{6}", totalCount.ToString(),inTds.ToString(), i.ToString(), j.ToString(), repeats.ToString(), sendList.Count.ToString(), sendTime);
            httper.SendByFocusSend("Post Purchase Survey Auto SMS Notification", body);
            string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n生成短链接失败数量：{3}\n重复数量：{4}\n发送数量：{5}\n发送时间：{6}", totalCount.ToString(), inTds.ToString(), i.ToString(), j.ToString(), repeats.ToString(), sendList.Count.ToString(), sendTime);
            httper.SendSMS(contents);

        }
        private List<LC_AUTOFILE> InsertToAutoFile(DataTable dt,string filename,ref int i)
        {
            var list = new List<LC_AUTOFILE>();
            if (dt == null || dt.Rows.Count == 0) return list;
            foreach (DataRow dr in dt.Rows)
            {
                
                try
                {
                    
                    if (httper.IsDateTime(dr["Expired date"].ToString()))
                    {
                        LC_AUTOFILE auto = new LC_AUTOFILE();
                        auto.print_name = dr.IsNull("print_name") ? string.Empty : dr["print_name"].ToString();
                        auto.URL = dr.IsNull("URL") ? string.Empty : dr["URL"].ToString(); dr["URL"].ToString();
                        auto.phone_no = dr.IsNull("phone_no") ? string.Empty : dr["phone_no"].ToString();
                        auto.Expired_date = dr.IsNull("Expired date") ? string.Empty : dr["Expired date"].ToString();
                        auto.FILENAME = filename;
                        auto.CreateDate = DateTime.Now;
                        list.Add(auto);
                        repository.Add<LC_AUTOFILE>(auto);
                        repository.UnitOfWork.SaveChanges();
                    }
                    else
                    {
                        i++;
                        log.WriteLog(string.Format("{0}，日期无效", dr["phone_no"]));
                    }
                   

                }
                catch (Exception ex)
                {
                    i++;
                    log.WriteLog(string.Format("{0}，写入AUTOFILE失败：{1}", dr["phone_no"], ex.Message));
                    return new List<LC_AUTOFILE>();
                }
               
            }
              return list;
        }
        private List<LC_SHORTURL> InsertToShortUrl(List<LC_AUTOFILE> list,DateTime sendTime,ref int j)
        {
            var urls = new List<LC_SHORTURL>();
            if (list.Count == 0) return urls;
            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.Expired_date) || string.IsNullOrWhiteSpace(item.URL) || string.IsNullOrWhiteSpace(item.print_name))
                {
                    j++;
   
                    log.WriteLog(string.Format("{0}，日期/URL/PRINTNAME为空", item.phone_no));
                }
                else
                {

                    string shorturl = CreateShortUrl(item.URL);
                    if (string.IsNullOrEmpty(shorturl))
                    {
                        j++;
                        log.WriteLog(string.Format("{0}，shorturl为空", item.phone_no));
                    }
                    else
                    {
                        try
                        {
                            LC_SHORTURL urlmodel = new LC_SHORTURL();
                            string copy = string.Format("【连卡佛】尊敬的{0}，感谢您近期的光顾！诚邀您于{1}前完成购物体验调查 {2} /回TD退订", item.print_name, item.Expired_date, shorturl);
                            urlmodel.URL_CREATETIME = DateTime.Now;
                            urlmodel.URL_COPY = copy;
                            urlmodel.URL_FILENAME = item.FILENAME;
                            urlmodel.URL_DATE = item.Expired_date;
                            urlmodel.URL_LONG = item.URL;
                            urlmodel.URL_MOBILE = item.phone_no;
                            urlmodel.URL_PRINTNAME = item.print_name;
                            urlmodel.URL_SENDTIME = sendTime;
                            urlmodel.URL_SHORT = shorturl;
                            urlmodel.URL_VIPNO = item.vip_no;
                            urls.Add(urlmodel);
                            repository.Add<LC_SHORTURL>(urlmodel);
                            repository.UnitOfWork.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            j++;
                            log.WriteLog(string.Format("{0}，写入SHORTURL失败：{1}", item.phone_no, ex.Message));
                        }

                    }
                }
            }
            return urls;
        }
        private string CreateShortUrl(string longurl)
        {
            try
            {
                string api_url = "http://l-c.co/yourls-api.php";
                string short_url = string.Empty;
                string param = string.Format("username={0}&password={1}&action={2}&url={3}&title={4}&format={5}", "lead2win", "bu9OtPz9", "shorturl", HttpUtility.UrlEncode(longurl.Trim()), "lead2win", "simple");
                short_url = httper.HttpGet(api_url, param, "application/x-www-form-urlencoded");
                return short_url;
            }
            catch(Exception ex)
            {
                log.WriteLog(string.Format("生成短链接失败：{0}", ex.Message));
                return string.Empty;
            }
 
        }
    }
}
