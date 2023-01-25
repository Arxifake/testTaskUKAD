using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTaskUKAD
{
    internal class Result
    {
        public Result() { }
        public Dictionary<string, double> linkTime { get; set; }
         public List<HtmlNode> allLinks { get; set; }



    }
}
