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
            HandleMemeSubscriber();
        }


        private async void HandleMemeSubscriber()
        {

            dynamic data = null;
            int maxNumberOfPosts = 5;

            try
            {
                var subscribers = await _dapperDB.GetMemesSubscribers();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateMemesNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(1));

                        if(data == null)
                        {
                            var response = await _httpHandler.Get("https://www.reddit.com/r/memes/top.json?limit=" + maxNumberOfPosts + "&raw_json=1");
                            string responseBody = await response.Content.ReadAsStringAsync();

                            data = JsonConvert.DeserializeObject(responseBody);
                        }

                        string imageUrl = "";
                        string title = "";
                        string permalink = "";
                        int i = 0;
                        
                        while(imageUrl == "" && i < maxNumberOfPosts)
                        {
                            

                            imageUrl = data.data.children[i].data.preview.images[0].source.url;
                            title = data.data.children[i].data.title;
                            permalink = data.data.children[i].data.permalink;

                            i++;
                        }

                        if (imageUrl != "")
                        {
                            _telegram.SendImage(subscriber.chatId, imageUrl,"<b>" + title + "</b> - Source: https://www.reddit.com" + permalink, "html");
                        }
                        else
                        {
                            _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", "Reddit didn't provide an image in the top 5 posts :(", "HandleMemeSubscriber");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleMemeSubscriber");
            }
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
