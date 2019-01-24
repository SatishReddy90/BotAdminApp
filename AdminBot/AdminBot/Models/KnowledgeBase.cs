using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdminBot.Models
{
    public class Knowledgebase
    {
        public string Id { get; set; }
        [Display(Name = "Host name")]
        public string HostName { get; set; }
        public string LastAccessedTimestamp { get; set; }
        [Display(Name = "Last modified")]
        public string LastChangedTimestamp { get; set; }
        [Display(Name = "Last published")]
        public string LastPublishedTimestamp { get; set; }
        [Display(Name = "Knowledge base name")]
        public string Name { get; set; }
        public string UserId { get; set; }
        public List<string> Urls { get; set; }
        public List<string> Sources { get; set; }

    }
    public class RootObject
    {
        public List<Knowledgebase> knowledgebases { get; set; }
    }
}
