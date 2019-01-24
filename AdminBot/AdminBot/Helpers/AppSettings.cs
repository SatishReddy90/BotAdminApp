using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminBot.Helpers
{
    public class AppSettings
    {
        public QnAMaker QnAMaker { get; set; }
    }

    public class QnAMaker
    {
        public string Host { get; set; }
        public string Service { get; set; }
        public string SubscriptionKey { get; set; }
        public QnAMethods Methods { get; set; }
    }

    public class QnAMethods
    {
        public string Create { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string Get { get; set; }
        public string EndPointKeys { get; set; }
    }
}
