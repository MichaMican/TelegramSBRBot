using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("DBStorage")]
    public class DBStorage
    {
        [ExplicitKey]
        public string key { get; set; }
        public string value { get; set; }
        public DateTime dateTimeValue { get; set; }
    }
}
