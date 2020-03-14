using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("CurrentVersion")]
    public class CurrentVersion
    {
        [ExplicitKey]
        public string version { get; set; }

    }
}
