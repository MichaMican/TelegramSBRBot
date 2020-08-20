using Dapper.Contrib.Extensions;
using System;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("DuckSubscriber")]
    public class DuckSubscriber : Subscribeable
    {
        [ExplicitKey]
        public override string chatId { get; set; }
        public override DateTime nextUpdateOn { get; set; }
    }
}
