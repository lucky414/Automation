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
             registry.Schedule(() => Execute()).ToRunNow().AndEvery(1).Days().At(Int32.Parse(hh), Int32.Parse(mm));
            JobManager.Initialize(registry);
        }
        public void Execute()
        {
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
                    if (System.Configuration.ConfigurationManager.AppSettings["IsTesting"] == "1")
                    {
                        dates = "Testing";
                    }
                    string ftpFilePath = FtpConfig.ftpFilePath + "2018 Beauty Loyalty Bi-weekly Automation SMS/" + dates + "/";
                    FtpTools ftp = new FtpTools();

                    string[] file = ftp.GetFileList(ftpFilePath);
                    if (file == null)
                    {
                        HttpUtil.SendEmailBy465("Beauty Loyalty Bi-weekly Automation SMS", "FTP上未发现文件");
                        HttpUtil.SendSMS("FTP上未发现文件");
                    }
                    else if (file.Length > 1)
                    {
                        HttpUtil.SendEmailBy465("Beauty Loyalty Bi-weekly Automation SMS", "文件夹内不止1份发送数据");
                        HttpUtil.SendSMS("文件夹内不止1份发送数据");
                    }
                    else
                    {
                        string filename = file[0];
                        string info = string.Empty;
                        int exists = repository.Count<Models.FTP_FILE>(x => x.NAME.Equals(filename));
                        if (exists == 0)
                        {
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
                                    HttpUtil.SendEmailBy465("Beauty Loyalty Bi-weekly Automation SMS", "自动化文件数据为空");
                                    HttpUtil.SendSMS("自动化文件数据为空");
                                }
                            }
                            else
                            {
                                LogUtil.WriteLog("从FTP下载文件失败");
                            }
                        }
                        else
                        {
                            LogUtil.WriteLog("文件已存在");
                        }

                    }
                }
                else
                {
                    LogUtil.WriteLog("不是发送日");
                }

            }
            catch (Exception ex)
            {
                LogUtil.WriteLog("ParseDocument error：" + ex.Message);
            }
        }
        private DataTable GetDataFromCSV(string path)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.GetEncoding("GB2312")))
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
            DateTime sendTime = Convert.ToDateTime(DateTime.Now.ToShortDateString()).AddHours(16);
       
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
                    if (dr.IsNull("phone_no") || dr.IsNull("Cut Off Date") || dr.IsNull("Current Tier") || dr.IsNull("COS_Spend"))
                    {
                        i++;
                        //空值不处理
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(dr["last_name_eng"].ToString()) && string.IsNullOrWhiteSpace(dr["last_name_chi"].ToString()) && string.IsNullOrWhiteSpace(dr["title_adj"].ToString()))
                        {
                            i++;
                            //尊称空值不处理
                        }
                        else if (!HttpUtil.IsDateTime(dr["Cut Off Date"].ToString()))
                        {
                            //日期格式不对的
                            i++;
                        }
                        else
                        {

                            String copy = dic[dr["Current Tier"].ToString()];
                            string cos_spend = dr["COS_Spend"].ToString().Length <= 3 ? Int32.Parse(dr["COS_Spend"].ToString()).ToString() : Int32.Parse(dr["COS_Spend"].ToString()).ToString("N0");
                            string spend_to_next = dr["Spending to Next Tier"].ToString().Length <= 3 ? Int32.Parse(dr["Spending to Next Tier"].ToString()).ToString() : Int32.Parse(dr["Spending to Next Tier"].ToString()).ToString("N0");
                            if (copy != null)
                            {
                                string title = dr["last_name_chi"].ToString() + dr["title_adj"].ToString();
                                if (string.IsNullOrWhiteSpace(dr["last_name_chi"].ToString()) && !string.IsNullOrWhiteSpace(dr["last_name_eng"].ToString()))
                                {
                                    title = dr["last_name_eng"].ToString() + dr["title_adj"].ToString();
                                }
                                copy = copy.Replace("[Last Name][Title]", title);
                                copy = copy.Replace("[Cut Off Date]", dr["Cut Off Date"].ToString());
                                copy = copy.Replace("[COS Spend]", cos_spend);
                                copy = copy.Replace("[Spending to Next Tier]", spend_to_next);
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
                            else
                            {
                                i++;
                            }
                        }



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
            var _unique = from p in sendcount group p by new { Mobile=p.MOBILE,CommId=p.TIER} into g select new { g.Key.Mobile,g.Key.CommId, Copy = g.Max(p => p.COPY) };
            var unique = _unique.ToList();
            int m = sendcount.Count() - unique.Count;
            LogUtil.WriteLog(string.Format("重复数量：{0}",m.ToString()));
            int wx = unique.Where(x => x.Mobile.Length != 11).Count();

            var _sendlist = from s in unique where !td.Any(ss => ss.Mobile.Equals(s.Mobile)) && s.Mobile.Length == 11 select s;
            var sendlist = _sendlist.ToList();
            i = i + wx;
            LogUtil.WriteLog(string.Format("无效数量：{0}", i.ToString()));
            int n = sendlist.Count;
            LogUtil.WriteLog(string.Format("发送数量：{0}", n.ToString()));
            int u = unique.Count - n - wx;
            LogUtil.WriteLog(string.Format("TD数量：{0}", u.ToString()));
            DateTime time = DateTime.Now.AddHours(1);
            string sendTitle= "Automation" + DateTime.Now.ToString("yyyyMMdd");
            if (sendlist.Count > 0)
            {
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
                    repository.Add<Models.YM_SendList>(send);
                }
                repository.UnitOfWork.SaveChanges();
            }
            LogUtil.WriteLog("Task  解析结束");
            string body = string.Format("你好，本次自动化数据处理结果如下：<br />总数：{0}<br />TD数量：{1}<br />无效数量：{2}<br />重复数量：{3}<br />发送数量：{4}<br />发送时间：{5}", totalCount.ToString(), u.ToString(), i.ToString(),  m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendEmailBy465("Beauty Loyalty Bi-weekly Automation SMS", body);
            string contents = string.Format("你好，本次自动化数据处理结果如下：\n总数：{0}\nTD数量：{1}\n无效数量：{2}\n重复数量：{3}\n发送数量：{4}\n发送时间：{5}", totalCount.ToString(), u.ToString(), i.ToString(),  m.ToString(), n.ToString(), sendTime);
            HttpUtil.SendSMS(contents);
        }
    }
}
