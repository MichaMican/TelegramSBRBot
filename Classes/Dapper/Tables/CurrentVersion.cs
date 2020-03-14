using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("Countdown")]
    public class Countdown
    {
        [ExplicitKey]
        public int messageId { get; set; }
        public DateTime countdownEnd { get; set; }
        public string title { get; set; }
        public string chatId { get; set; }
    }
}
