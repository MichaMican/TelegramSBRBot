using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("EventLog")]
    public class EventLog
    {
        [Key]
        public int id { get; set; }
        public string source { get; set; }
        public string type { get; set; }
        public string logGroup { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
    }
}
