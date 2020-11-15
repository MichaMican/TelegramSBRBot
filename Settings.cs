using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot
{
    public class Settings
    {
        public string botToken { get; set; }
        public string dbServer { get; set; }
        public string dbUser { get; set; }
        public string dbPwd { get; set; }
        public string dbInitCat { get; set; }
        public string ideaChatId { get; set; }
        public string devChatId { get; set; }
        public string dcMiddlewareApiKey { get; set; }
        public ulong dcAufAbrufId { get; set; }
        public ulong dcAufAbrufRoleId { get; set; }
        public string tlgrmSbrGroupId { get; set; }
    }
}
