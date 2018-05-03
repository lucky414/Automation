using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Auto_BL.Models
{
    public class SendTask : IJob
    {
        private IRepository.IRepository repository;
        public SendTask()
        {
            repository = new Repository.Repository(new Auto_Bl_Entities());
        }
        public void Init()
        {
            LogUtil.WriteLog("Task Init");
            string hh = System.Configuration.ConfigurationManager.AppSettings["StartHour"];
            string mm = System.Configuration.ConfigurationManager.AppSettings["StartMinute"];
            Registry registry = new Registry();
            registry.Schedule(() => Execute()).ToRunEvery(1).Days().At(Int32.Parse(hh), Int32.Parse(mm));
            // registry.Schedule(() => Execute()).ToRunNow().AndEvery(1).Days().At(Int32.Parse(hh), Int32.Parse(mm));
            JobManager.Initialize(registry);
        }
        public void Execute()
        {

           // string weekstr = DateTime.Now.DayOfWeek.ToString();
            LogUtil.WriteLog("Task Execute");
            try
            {
                XmlDocument pdoc = new XmlDocument();
                string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\Schedule.xml";
                pdoc.Load(xmlFile);

                XmlNodeList nodeList = pdoc.SelectNodes("root/date");
                List<string> sendDates = new List<string>();
                foreach (XmlNode node in nodeList)
                {
                    sendDates.Add(node.InnerText);
                }
                string name = DateTime.Now.ToString("yyyy-MM-dd");
                if (sendDates.Contains(DateTime.Now.ToString("yyyy-MM-dd")))
                {
                    string dates = DateTime.Now.ToString("yyyyMMdd");
                    string ftpFilePath = FtpConfig.ftpFilePath + "SS18 Beauty Loyalty " + dates + "/";
                    FtpTools ftp = new FtpTools();

                    string[] file = ftp.GetFileList(ftpFilePath);
                    if (file == null)
                    {
                        HttpUtil.SendByFocusSend("Beauty Loyalty Auto SMS Notification", "FTP上未发现文件");
                        HttpUtil.SendSMS("FTP上未发现文件");
                    }
                    else if (file.Length > 1)
                    {
                        HttpUtil.SendByFocusSend("Beauty Loyalty Auto SMS Notification", "文件夹内不止1份发送数据");
                        HttpUtil.SendSMS("文件夹内不止1份发送数据");
                    }
                    else
                    {
                        // string filename = string.Format("CN_Beauty_Loyalty_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd"));
                        string filename = file[0];
                        string info = string.Empty;
                        string localPath = System.AppDomain.CurrentDomain.BaseDirectory + "Files\\" + dates;
                        bool downresult = ftp.Download(localPath, filename, ftpFilePath, out info);
                        if (downresult)
                        {
                            DataTable cstData = GetDataFromCSV(localPath + "/" + filename);
                            if (cstData != null && cstData.Rows.Count > 0)
                            {
                                try
                                {
                                    SaveData(cstData, filename);
                                }
                                catch (Exception ex)
                                {
                                    LogUtil.WriteLog("error:" + ex.Message);
                                }

                            }
                            else
                            {
                                HttpUtil.SendByFocusSend("Beauty Loyalty Auto SMS Notification", "自动化文件数据为空");
                                HttpUtil.SendSMS("自动化文件数据为空");
                            }
                        }
                        else
                        {
                            LogUtil.WriteLog("从FTP下载文件失败");
                        }
                    }
                    //string filename = string.Format("Survey_SMS_CN_Vendor_{0}_B1.csv", DateTime.Now.ToString("MM_yyyy"));
                    //int count = repository.Count<DataModel.LC_AUTOFILE>(x => x.FILENAME.ToLower().Equals(filename.ToLower()));
                    //if (count == 0)
                    //{
                    //    ParseData();
                    //}

                }

            }
            catch (Exception ex)
            {
                LogUtil.WriteLog("ParseDocument error：" + ex.Message);
            }
            //if (weekstr.Equals("Thursday", StringComparison.OrdinalIgnoreCase))
            //{
            //    //每周四发
            //    //SS18 Beauty Loyalty 20180419
                
            //}
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
            string smsid = DateTime.Now.ToString("yyyyMMddHHmmss");
            DateTime sendTime = DateTime.Now.AddHours(3);
            int totalCount = dt.Rows.Count;
            int i = 0;
            XmlDocument pdoc = new XmlDocument();
            string xmlFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Files\Copy.xml";
            pdoc.Load(xmlFile);

            XmlNodeList nodeList = pdoc.SelectNodes("root/tier");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (XmlNode node in nodeList)
            {
                dic.Add(node.Attributes["name"].Value, node.InnerText.Trim());
            }
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    if (dr.IsNull("phone_no") || dr.IsNull("Cut Off Date")||dr.IsNull("Current Tier") || dr.IsNull("COS_Spend"))
                    {
                        i++;
                        //空值不处理
                    }
                    else
                    {
                        String copy = dic[dr["Current Tier"].ToString()];
                        string cos_spend= dr["COS_Spend"].ToString().Length <= 3 ? dr["COS_Spend"].ToString() : Int32.Parse(dr["COS_Spend"].ToString()).ToString("N0");
                        string spend_to_next= dr["Spending to Next Tier"].ToString().Length <= 3 ? dr["Spending to Next Tier"].ToString() : Int32.Parse(dr["Spending to Next Tier"].ToString()).ToString("N0");
                        if (copy != null)
                        {
                            copy = copy.Replace("[Last Name][Title]", dr["last_name_eng"].ToString()+ dr["title_adj"].ToString());
                            copy = copy.Replace("[Cut Off Date]", dr["Cut Off Date"].ToString());
                            copy = copy.Replace("[COS Spend]", cos_spend);
                            copy = copy.Replace("[Spending to Next Tier]", spend_to_next);
                      
                        }
                        Models.FTP_FILE auto = new FTP_FILE();
                        auto.COSSPEND = cos_spend;
                        auto.CUTOFFDATE = dr["Cut Off Date"].ToString();
                        auto.DATES = DateTime.Now;
                        auto.LASTNAME = dr["last_name_eng"].ToString();
                        auto.NAME = filename;
                        auto.MOBILE = dr["phone_no"].ToString();
                        auto.SPENDTONEXTTIER = spend_to_next;
                        auto.TIER = dr["Current Tier"].ToString();
                        auto.TITLE = dr["title_adj"].ToString();
                        auto.COPY = copy;
                        repository.Add<Models.FTP_FILE>(auto);
                        repository.UnitOfWork.SaveChanges();

                    }
                }
                catch (Exception ex)
                {
                    i++;
                    LogUtil.WriteLog(string.Format("{0}，写入数据失败：{1}", dr["phone_no"], ex.Message));
                }

            }
            // 提交发送
            var td = repository.GetAll<Models.TD_List>();
            var sendcount = repository.GetQuery<Models.FTP_FILE>(x => x.NAME.Equals(filename, StringComparison.OrdinalIgnoreCase));
            var unique = from p in sendcount group p by p.MOBILE into g select new { g.Key, Copy = g.Max(p => p.COPY) };
            int m = sendcount.Count() - unique.Count();
            int wx = unique.Count(g => g.Key.Length != 11);
            var sendlist = from s in unique where !td.Any(ss => ss.Mobile.Equals(s.Key)) && s.Key.Length == 11 select s;
            i = i + wx;
            int n = sendlist.Count();
            int u = unique.Count() - n - wx;
            DateTime time = DateTime.Now;
            foreach (var item in sendlist)
            {
                Models.YM_SendList send = new YM_SendList();
                send.Send_AccountId = 1;
                send.Send_Batch = 1;
                send.Send_CheckId = "Auto_BL";
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
            string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />重复数量：{3}<br />发送数量：{4}<br />发送时间：{5}", totalCount.ToString(), u.ToString(), i.ToString(),  m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendByFocusSend("Post Purchase Survey Auto SMS Notification", body);
            string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n重复数量：{3}\n发送数量：{4}\n发送时间：{5}", totalCount.ToString(), u.ToString(), i.ToString(),  m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendSMS(contents);
        }
    }
}
