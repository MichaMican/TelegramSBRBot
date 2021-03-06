﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("CSGOUpdatesSubscriber")]
    public class CSGOUpdatesSubscriber : Subscribeable
    {
        [ExplicitKey]
        public override string chatId { get; set; }
        public override DateTime nextUpdateOn { get; set; }
    }
}
