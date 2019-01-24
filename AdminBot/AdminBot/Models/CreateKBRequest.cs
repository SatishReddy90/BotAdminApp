using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminBot.Models
{
    public class CreateKBRequest
    {
        public string name { get; set; }
        public List<QnaList> qnaList { get; set; }
        public List<string> urls { get; set; }
        public List<object> files { get; set; }
    }

    public class Metadata
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class QnaList
    {
        public int id { get; set; }
        public string answer { get; set; }
        public string source { get; set; }
        public List<string> questions { get; set; }
        public List<Metadata> metadata { get; set; }
    }
}
