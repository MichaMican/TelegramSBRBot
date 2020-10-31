using Dapper.Contrib.Extensions;
using System;

namespace TelegramFunFactBot.Classes.Dapper.Tables
{
    [Table("AlpacaSubscriber")]
    public class AlpacaSubscriber : Subscribeable
    {
        [ExplicitKey]
        public override string chatId { get; set; }
        public override DateTime nextUpdateOn { get; set; }
    }
}
