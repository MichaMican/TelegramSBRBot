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
        Task<string> GetCurrentVersion();
        void UpdateVersion(string newVersion);
        void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn);
        void SubscribeToDeutscheMemes(string chatId, DateTime nextUpdateOn);
        void SubscribeToMemes(string chatId, DateTime nextUpdateOn);
        void UnsubscribeFromFunFacts(string chatId);
        void UnsubscribeFromDeutscheMemes(string chatId);
        void UnsubscribeFromMemes(string chatId);
        void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn);
        void UpdateMemesNextUpdateOn(string chatId, DateTime nextUpdateOn);
        void UpdateDeutscheMemesNextUpdateOn(string chatId, DateTime nextUpdateOn);
        Task<List<FunFactSubscriber>> GetFunFactSubscribers();
        Task<List<MemeSubscriber>> GetMemesSubscribers();
        Task<List<DeutscheMemeSubscriber>> GetDeutscheMemesSubscribers();
        Task<List<UpdateLogSubscriber>> GetAllUpdateSubscriber();
        void SubscribeToUpdateLog(string chatId);
        void UnsubscribeFromUpdateLog(string chatId);

    }
}
