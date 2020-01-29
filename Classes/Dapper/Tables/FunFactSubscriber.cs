using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("FunFactSubscriber")]
    public class FunFactSubscriber
    {
        [ExplicitKey]
        public string chatId { get; set; }
        public DateTime nextUpdateOn { get; set; }

    }
}
