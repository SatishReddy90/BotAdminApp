using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.Models
{
    public class AppSettings
    {
        public List<QnaService> qnaServices { get; set; }
        public LuisApp luisApp { get; set; }
        public List<string> luisIntents { get; set; }
    }

    public class QnaService
    {
        public string type { get; set; }
        public string endpointKey { get; set; }
        public string hostname { get; set; }
        public string id { get; set; }
        public string kbId { get; set; }
        public string name { get; set; }
        public string subscriptionKey { get; set; }
    }

    public class LuisApp
    {
        public string type { get; set; }
        public string appId { get; set; }
        public string authoringKey { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string region { get; set; }
        public List<string> serviceIds { get; set; }
        public string subscriptionKey { get; set; }
        public string version { get; set; }
    }
}
