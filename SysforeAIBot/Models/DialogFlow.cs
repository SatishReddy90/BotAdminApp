using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.Models
{
    public class DialogFlow
    {
        public string Answer { get; set; }
        public string Question { get; set; }
        public List<DialogFlow> Branches { get; set; } = new List<DialogFlow>();
    }
}
