using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.DTO
{
    public class LuisAppInput
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
