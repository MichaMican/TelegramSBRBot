using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.Dapper.Tables;

namespace TelegramFunFactBot.Interfaces
{
    public interface IDapperDB
    {
        void WriteEventLog(string source, string type, string message, string logGroup = null);
        void WriteRequestLog(string jsonString);
        void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn);
        void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn);
        Task<List<FunFactSubscriber>> GetFunFactSubscribers();
    }
}
