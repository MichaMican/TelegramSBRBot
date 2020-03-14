using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Models
{
    public class Message
    {
        public int message_id { get; set; }
        public int date { get; set; }
        public Chat chat { get; set; }
    }
}
