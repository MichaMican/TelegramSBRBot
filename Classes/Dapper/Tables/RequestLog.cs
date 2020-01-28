using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("RequestLog")]
    public class RequestLog
    {
        [Key]
        public int id { get; set; }
        public DateTime timestamp { get; set; }
        public string requestJson { get; set; }
    }
}
