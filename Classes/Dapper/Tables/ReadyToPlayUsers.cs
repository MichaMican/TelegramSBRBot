using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("ReadyToPlayUsers")]
    public class ReadyToPlayUsers
    {
        [ExplicitKey]
        public string tlgrmId { get; set; }
        public string dcId { get; set; }
        public DateTime readyStartDate { get; set; }
        public DateTime readyEndDate { get; set; }
    }
}
