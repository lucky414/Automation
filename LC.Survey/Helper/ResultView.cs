using LC.Survey.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Helper
{
  public  class ResultView
    {
        public List<POSTSURVEY_FILES> Data { get; set; }
        public int Total { get; set; }
        public int Invalid { get; set; }
        public int ShortUrlNull { get; set; }
        public int TD { get; set; }
        public int Repeat { get; set; }
    }
}
