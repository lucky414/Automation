using EntityFramework.Extensions;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ShortUrl.Models
{
  public  class CreateShortUrl
    {
       // private List<LC_Members> list = null;
        DataEntities db = new DataEntities();
        public CreateShortUrl()
        {
           // var mems = from m in db.LC_Members where !db.LC_ShortUrlList.Any(x => x.Short_Flag.Equals(m.Mem_Flag)) select m;
           // list = mems.ToList();
        }
        public void Batch()
        {
            Logs.WriteLog("Start");
            try
            {
                
                db.LC_Members.Where(x => x.Mem_IsDeal == false).Update(u => new LC_Members { Mem_IsDeal = true });
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                Logs.WriteLog("Error:"+ex.Message);
            }

            Logs.WriteLog("End");
        }
        public void Init()
        {
            Logs.WriteLog("Task Start");
            Registry registry = new Registry();
            registry.Schedule(() => Instance()).ToRunNow().AndEvery(30).Minutes();
            JobManager.Initialize(registry);
        }
        public void Instance()
        {
            Logs.WriteLog("Task Run");
            string xmlFile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/Config.xml";
            XmlDocument pdoc = new XmlDocument();
            pdoc.Load(xmlFile);
            XmlNode node = pdoc.SelectSingleNode("root/flag");
            string flag = node.InnerText;
            node.InnerText = "1";
            pdoc.Save(xmlFile);
            if (flag == "0")
            {
                Logs.WriteLog("Has Task");
                List<LC_ShortUrlList> urls = new List<LC_ShortUrlList>();
                List<LC_Members> list = db.LC_Members.Where(x => x.Mem_IsDeal == false).Take(5000).OrderByDescending(x => x.Mem_Id).ToList();
                if (list.Count == 0)
                {
                    node.InnerText = "1";
                    pdoc.Save(xmlFile);
                    Logs.WriteLog("complate");
                }
                else
                {
                    Logs.WriteLog("start task");
                    try
                    {
                        var tasks = new List<Task<int>>();
                        int count = list.Count;
                        int ExeCount = count / 10;
                        int DivCount = count % 10;
                        Func<object, int> action = (object obj) => {
                            int i = (int)obj;
                            int start = i * ExeCount;
                            int end = (i + 1) * ExeCount;
            
                            for (int m = start; m < end; m++)
                            {
                                LC_ShortUrlList model = new LC_ShortUrlList();
                                model.Short_VipNo = list[m].Mem_VipNo;
                                model.Short_Mobile = list[m].Mem_Mobile;
                                model.Short_LongUrl = list[m].Mem_LongUrl;

                                string url = CreateUrl(list[m].Mem_LongUrl, list[m].Mem_Mobile);
                                model.Short_ShortUrl = url;
                                model.Short_CommId = list[m].Mem_CommId;
                                urls.Add(model);
                                Logs.WriteLog("short url:" + url);
                            }
                            if (i == count - 1) {
                                for (int x = 0; i < DivCount; x++)
                                {
                                    int m = (i + 1) * ExeCount + x;
                                    LC_ShortUrlList model = new LC_ShortUrlList();
                                    model.Short_VipNo = list[m].Mem_VipNo;
                                    model.Short_Mobile = list[m].Mem_Mobile;
                                    model.Short_LongUrl = list[m].Mem_LongUrl;

                                    string url = CreateUrl(list[m].Mem_LongUrl, list[m].Mem_Mobile);
                                    model.Short_ShortUrl = url;
                                    model.Short_CommId = list[m].Mem_CommId;
                                    urls.Add(model);
                                    Logs.WriteLog("short url:" + url);

                                }
                            }

                            return 0;
                        };
                        for (int i = 0; i < 10; i++)
                        {
                            int index = i;
                            tasks.Add(Task<int>.Factory.StartNew(action, index));
                        }
                        Task.WaitAll(tasks.ToArray());

                    }
                    catch(Exception ex)
                    {
                        Logs.WriteLog("error:" + ex.Message);
                    }
                    if (urls.Count > 0)
                    {
                        db.BulkInsert<LC_ShortUrlList>(urls);
                        db.BulkSaveChanges();
                    }
                    node.InnerText = "0";
                    pdoc.Save(xmlFile);
                    db.LC_Members.Where(x => x.Mem_IsDeal == false).Take(5000).OrderByDescending(x => x.Mem_Id).Update(u => new LC_Members { Mem_IsDeal = true });
                    Logs.WriteLog("end task");

                }
            }
            else
            {
                Logs.WriteLog("Flag 为1" );
            }
            
        }
        //public void Create()
        //{

        //    var watch = new Stopwatch();
        //    List<LC_Members> list= db.LC_Members.Where(x => x.Mem_IsDeal == false).Take(1000).OrderByDescending(x => x.Mem_Id).ToList();
        //    int count = list.Count;
        //    int coreCount = 10;
        //    int ExeCount = count / coreCount;
        //    var tasks = new Task[coreCount];
        //    for (int i= 0; i < coreCount; i++)
        //    {

        //        tasks[i] = Task.Factory.StartNew(() => { ThreadRun(i, ExeCount, list); });

        //    }
        //    Task.WaitAll(tasks, 1000*5);
        //    watch.Stop();

        //    Logs.WriteLog(string.Format("tasks共使用时间：{0}s={1}ms", watch.ElapsedMilliseconds / 1000, watch.ElapsedMilliseconds));

        //}
        //private void ThreadRun(int threadIndex, int exeCount,List<LC_Members> list)
        //{
        //    try
        //    {
        //        int start = threadIndex * exeCount;
        //        int end = (threadIndex + 1) * exeCount;
        //        for (int i = start; i < end; i++)
        //        {
        //            CreateUrl(list[i].Mem_LongUrl, list[i].Mem_Mobile);

        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Logs.WriteLog(string.Format("Error:{0}", ex.Message));
        //    }

        //}
        private string  CreateUrl(string longurl, string mobile)
        {
            string api_url = "http://l-c.co/yourls-api.php";
            string short_url = string.Empty;
            string param = string.Format("username={0}&password={1}&action={2}&url={3}&title={4}&format={5}", "lead2win", "bu9OtPz9", "shorturl", System.Web.HttpUtility.UrlEncode(longurl.Trim()), "lead2win", "simple");
            short_url = HttpUtil.HttpGet(api_url, param, "application/x-www-form-urlencoded");
            return short_url;
           // FileTools.WriteData(string.Format("{0},{1}", mobile, short_url));
            //Logs.WriteLog(string.Format("{0}:{1}", mobile, short_url));
        }
    }
}
