using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    public abstract class Subscribeable
    {

        public abstract string chatId { get; set; }
        public abstract DateTime nextUpdateOn { get; set; }
    }
}
