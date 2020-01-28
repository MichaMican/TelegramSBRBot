using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface IDapperDB
    {
        void WriteEventLog(string source, string type, string logGroup, string message);
        void WriteRequestLog(string jsonString);
    }
}
