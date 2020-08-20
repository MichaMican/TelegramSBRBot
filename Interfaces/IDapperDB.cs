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
        void SaveToDBStorage(string key, string value);
        string LoadFromDBStorage(string key);
        Task<string> GetCurrentVersion();
        void UpdateVersion(string newVersion);
        void SubscribeToFunFacts(string chatId, DateTime nextUpdateOn);
        void SubscribeToDeutscheMemes(string chatId, DateTime nextUpdateOn);
        void SubscribeToMemes(string chatId, DateTime nextUpdateOn);
        void SubscribeToCsgoUpdates(string chatId);
        void UnsubscribeFromFunFacts(string chatId);
        void UnsubscribeFromDeutscheMemes(string chatId);
        void UnsubscribeFromMemes(string chatId);
        void UnsubscribeFromCsgoUpdates(string chatId);
        void UpdateFunFactNextUpdateOn(string chatId, DateTime nextUpdateOn);
        void UpdateMemesNextUpdateOn(string chatId, DateTime nextUpdateOn);
        void UpdateDeutscheMemesNextUpdateOn(string chatId, DateTime nextUpdateOn);
        Task<List<FunFactSubscriber>> GetFunFactSubscribers();
        Task<List<MemeSubscriber>> GetMemesSubscribers();
        Task<List<DeutscheMemeSubscriber>> GetDeutscheMemesSubscribers();
        Task<List<UpdateLogSubscriber>> GetAllUpdateSubscriber();
        Task<List<CSGOUpdatesSubscriber>> GetAllCsgoUpdateSubscriber();
        Task<List<DuckSubscriber>> GetAllDuckSubscriber();
        void SubscribeToUpdateLog(string chatId);
        void UnsubscribeFromUpdateLog(string chatId);
        void SetCountdown(string chatId, string title, DateTime countdownEnd, int messageId);
        Task<List<Countdown>> GetAllCountdowns();
        void StopCountdown(int messageId);
        void SubscribeToDucks(string chatId, DateTime timeToUpdate);
        void UnsubscribeToDucks(string chatId);
        void UpdateDucksNextUpdateOn(string chatId, DateTime nextUpdateOn);

    }
}
