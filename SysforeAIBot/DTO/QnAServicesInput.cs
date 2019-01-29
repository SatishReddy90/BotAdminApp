using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.DTO
{
    public class QnAServicesInput
    {
        public List<QnaService> qnaServices { get; set; }
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
}
