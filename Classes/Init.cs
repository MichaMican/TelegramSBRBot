using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class Init : IInit
    {
        private readonly ITelegramAPICommunicator _telegram;
        private readonly IDapperDB _dapperDB;
        private readonly IHttpHandler _httpHandler;

        public Init(ITelegramAPICommunicator telegram, IDapperDB dapperDB, IHttpHandler httpHandler)
        {
            _telegram = telegram;
            _dapperDB = dapperDB;
            _httpHandler = httpHandler;
        }

        public void CheckForSubscribedServices()
        {
            HandleFunFactSubscriber();
        }

        private class FunFact
        {
            public string id { get; set; }
            public string text { get; set; }
            public string source { get; set; }
            public string source_url { get; set; }
            public string language { get; set; }
            public string permalink { get; set; }
        }

        private async void HandleFunFactSubscriber()
        {
            try
            {
                var subscribers = await _dapperDB.GetFunFactSubscribers();
                foreach(var subscriber in subscribers)
                {
                    if(DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateFunFactNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(1));

                        var response = await _httpHandler.Get("https://uselessfacts.jsph.pl/today.json?language=de");
                        string responseBody = await response.Content.ReadAsStringAsync();

                        FunFact funFact = JsonConvert.DeserializeObject<FunFact>(responseBody);

                        string message = funFact.text + "\n" + "Quelle: " + funFact.source_url;
                        _telegram.SendMessage(subscriber.chatId, message);
                    }
                }
            } 
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleFunFactSubscriber");
            }
        }

    }
}
