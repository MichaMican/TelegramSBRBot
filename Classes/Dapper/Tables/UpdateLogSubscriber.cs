﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("UpdateLogSubscriber")]
    public class UpdateLogSubscriber
    {
        [ExplicitKey]
        public string chatId { get; set; }

    }
}
