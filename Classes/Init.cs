using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.RedditPostsClasses;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class Init : IInit
    {
        private readonly ITelegramAPICommunicator _telegram;
        private readonly IDapperDB _dapperDB;
        private readonly IHttpHandler _httpHandler;
        private readonly IRedditPostHandler _redditPostHandler;

        public Init(ITelegramAPICommunicator telegram, IDapperDB dapperDB, IHttpHandler httpHandler, IRedditPostHandler redditPostHandler)
        {
            _telegram = telegram;
            _dapperDB = dapperDB;
            _httpHandler = httpHandler;
            _redditPostHandler = redditPostHandler;
        }

        public void CheckForSubscribedServices()
        {
            try
            {
                HandleFunFactSubscriber();
                HandleMemeSubscriber();
                HandleDeutscheMemeSubscriber();
                UpdateCountDowns();
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("Init", "Error", "There was an error" + e.Message, "CheckForSubscribedServices");
            }

        }

        private async void HandleMemeSubscriber()
        {
            RedditPostData data = null;
            int maxNumberOfPosts = 5;

            try
            {
                var subscribers = await _dapperDB.GetMemesSubscribers();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateMemesNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(1));

                        if (data == null)
                        {
                            data = await _redditPostHandler.GetRedditTopPostWithImageData("memes", maxNumberOfPosts);
                        }

                        if (data.imageUrl != "")
                        {
                            _telegram.SendImage(subscriber.chatId, data.imageUrl, "<b>" + data.title + "</b> - Source: https://www.reddit.com" + data.permalink, "html");
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


        private async void HandleDeutscheMemeSubscriber()
        {

            RedditPostData data = null;
            int maxNumberOfPosts = 5;

            try
            {
                var subscribers = await _dapperDB.GetDeutscheMemesSubscribers();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateDeutscheMemesNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(1));

                        if (data == null)
                        {
                            data = await _redditPostHandler.GetRedditTopPostWithImageData("ich_iel", maxNumberOfPosts);
                        }

                        if (data.imageUrl != "")
                        {
                            _telegram.SendImage(subscriber.chatId, data.imageUrl, "<b>" + data.title + "</b> - Source: https://www.reddit.com" + data.permalink, "html");
                        }
                        else
                        {
                            _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", "Reddit didn't provide an image in the top 5 posts :(", "HandleDeutscheMemeSubscriber");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleDeutscheMemeSubscriber");
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
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateFunFactNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(1));

                        var response = await _httpHandler.Get("https://uselessfacts.jsph.pl/today.json?language=de");
                        string responseBody = await response.Content.ReadAsStringAsync();

                        FunFact funFact = JsonConvert.DeserializeObject<FunFact>(responseBody);

                        string message = funFact.text + "\n" + "Quelle: " + funFact.source_url;
                        await _telegram.SendMessage(subscriber.chatId, message);
                    }
                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleFunFactSubscriber");
            }
        }

        private async void UpdateCountDowns()
        {
            try
            {
                var allCountdowns = await _dapperDB.GetAllCountdowns();

                foreach (var countdown in allCountdowns)
                {
                    try
                    {
                        if (countdown.countdownEnd > DateTime.UtcNow)
                        {
                            TimeSpan timeSpan = countdown.countdownEnd - DateTime.UtcNow;

                            int days = ((int)Math.Floor(timeSpan.TotalDays));
                            int hours = ((int)Math.Floor(timeSpan.TotalHours)) % 24;
                            int minutes = ((int)Math.Floor(timeSpan.TotalMinutes)) % 60;

                            var message = "<b>" + countdown.title + "</b>| Days: " + days + " Hours: " + hours + " Minutes: " + minutes;

                            _telegram.UpdateMessage(countdown.chatId, countdown.messageId, message);
                        }
                        else
                        {
                            _dapperDB.StopCountdown(countdown.messageId);
                        }
                    }
                    catch (Exception e)
                    {
                        _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", "There was an error processing the countdown with messageId " + countdown.messageId + " Error: " + e.Message, "UpdateCountDowns");
                    }

                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "UpdateCountDowns");
            }
        }

    }
}
