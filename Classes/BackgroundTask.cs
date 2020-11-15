using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TelegramFunFactBot.Classes.Dapper.Tables;
using TelegramFunFactBot.Classes.RedditPostsClasses;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class BackgroundTask : IBackgroundTask
    {
        private readonly ITelegramAPICommunicator _telegram;
        private readonly IDapperDB _dapperDB;
        private readonly IHttpHandler _httpHandler;
        private readonly IRedditPostHandler _redditPostHandler;
        private readonly IDiscordAPICommunicator _dc;
        private readonly IReadyToPlayHandler _readyToPlayHandler;
        private readonly Settings _settings;

        public BackgroundTask(IOptions<Settings> settings, IReadyToPlayHandler readyToPlayHandler, IDiscordAPICommunicator dc, ITelegramAPICommunicator telegram, IDapperDB dapperDB, IHttpHandler httpHandler, IRedditPostHandler redditPostHandler)
        {
            _settings = settings.Value;
            _telegram = telegram;
            _dapperDB = dapperDB;
            _httpHandler = httpHandler;
            _redditPostHandler = redditPostHandler;
            _dc = dc;
            _readyToPlayHandler = readyToPlayHandler;
        }

        public async void CheckForSubscribedServices()
        {
            try
            {
                await HandleFunFactSubscriber();
                await HandleMemeSubscriber();
                await HandleDeutscheMemeSubscriber();
                await HandleDuckSubscriber();
                await HandleAlpacaSubscriber();
                await UpdateCountDowns();
                await CheckForCsgoUpdate();
                await _readyToPlayHandler.UpdateReadyToPlay();
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("Init", "Error", "There was an error" + e.Message, "CheckForSubscribedServices");
            }

        }

        

        private async Task CheckForCsgoUpdate()
        {
            //Only check every 5 minutes (So Valve doesnt get mad at me)
            if (DateTime.Now.Minute % 5 == 0)
            {
                try
                {
                    var result = await _httpHandler.Get("https://blog.counter-strike.net/index.php/category/updates/");
                    var resultString = await result.Content.ReadAsStringAsync();
                    HtmlDocument pageDocument = new HtmlDocument();
                    pageDocument.LoadHtml(resultString);

                    var linkElement = pageDocument.DocumentNode.SelectSingleNode("//*[@id=\"post_container\"]/div[1]/h2/a");

                    var newestCsUpdate = linkElement.InnerText;
                    string dateString = Regex.Replace(newestCsUpdate, "[^(0-9/).]", "");
                    string lastCsUpdate = (await _dapperDB.LoadFromDBStorage("lastCsgoUpdate")).value;

                    var link = linkElement.Attributes[0].Value;

                    if (lastCsUpdate != dateString)
                    {
                        await _dapperDB.SaveToDBStorage(new DBStorage(){
                            key = "lastCsgoUpdate", 
                            value = dateString 
                        });

                        var firstUpdateLogs = pageDocument.DocumentNode.SelectSingleNode("//*[@id=\"post_container\"]/div[1]");
                        string releaseNotes = "Release Notes:\n";
                        foreach (var element in firstUpdateLogs.ChildNodes)
                        {
                            if (element.Name == "p")
                            {
                                releaseNotes += element.InnerText + "\n";
                            }
                        }

                        releaseNotes = HttpUtility.HtmlDecode(releaseNotes);

                        var subs = await _dapperDB.GetAllCsgoUpdateSubscriber();
                        foreach (var sub in subs)
                        {
                            await _telegram.SendMessage(sub.chatId, "<b>New CS:GO release for " + dateString + "</b>\n"+ link + "\n\nAll past updates: https://blog.counter-strike.net/index.php/category/updates/ \n\n"+ releaseNotes);
                        }
                    }

                }
                catch (Exception e)
                {
                    _dapperDB.WriteEventLog("Init", "Error", "Could not check for CS updates - Exception: " + e.Message);
                    _telegram.SendErrorMessage("There is something wrong with the CSUpdate checker - FIX IT!");
                    _telegram.SendErrorMessage("Error was: " + e.Message);
                }
            }
        }

        private int CalcDaysToAddOnNextUpdate(DateTime nextUpdate)
        {
            var timeDifference = DateTime.Now - nextUpdate;
            var daysToAdd = (int) Math.Round(timeDifference.TotalDays) + 1;
            //Sanity check
            if(daysToAdd < 1)
            {
                daysToAdd = 1;
            }

            return daysToAdd;
        }

        private async Task HandleDuckSubscriber()
        {
            RedditPostData data = null;
            int maxNumberOfPosts = 5;

            try
            {
                var subscribers = await _dapperDB.GetAllDuckSubscriber();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateDucksNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(CalcDaysToAddOnNextUpdate(subscriber.nextUpdateOn)));

                        if (data == null)
                        {
                            data = await _redditPostHandler.GetRedditTopPostWithImageData("duck", maxNumberOfPosts);
                        }

                        if (data.imageUrl != "")
                        {
                            _telegram.SendImage(subscriber.chatId, data.imageUrl, "<b>" + data.title + "</b> - Source: https://www.reddit.com" + data.permalink, "html");
                        }
                        else
                        {
                            _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", "Reddit didn't provide an image in the top 5 posts :(", "HandleDuckSubscriber");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleDuckSubscriber");
            }
        }
        
        private async Task HandleAlpacaSubscriber()
        {
            RedditPostData data = null;
            int maxNumberOfPosts = 5;

            try
            {
                var subscribers = await _dapperDB.GetAllAlpacaSubscriber();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateAlpacasNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(CalcDaysToAddOnNextUpdate(subscriber.nextUpdateOn)));

                        if (data == null)
                        {
                            data = await _redditPostHandler.GetRedditTopPostWithImageData("alpaca", maxNumberOfPosts);
                        }

                        if (data.imageUrl != "")
                        {
                            _telegram.SendImage(subscriber.chatId, data.imageUrl, "<b>" + data.title + "</b> - Source: https://www.reddit.com" + data.permalink, "html");
                        }
                        else
                        {
                            _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", "Reddit didn't provide an image in the top 5 posts :(", "HandleAlpacaSubscriber");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _dapperDB.WriteEventLog("CheckForSubscribedServices", "Error", e.Message, "HandleAlpacaSubscriber");
            }
        }

        private async Task HandleMemeSubscriber()
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
                        _dapperDB.UpdateMemesNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(CalcDaysToAddOnNextUpdate(subscriber.nextUpdateOn)));

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


        private async Task HandleDeutscheMemeSubscriber()
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
                        _dapperDB.UpdateDeutscheMemesNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(CalcDaysToAddOnNextUpdate(subscriber.nextUpdateOn)));

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

        private async Task HandleFunFactSubscriber()
        {
            try
            {
                var subscribers = await _dapperDB.GetFunFactSubscribers();
                foreach (var subscriber in subscribers)
                {
                    if (DateTime.Now > subscriber.nextUpdateOn)
                    {
                        _dapperDB.UpdateFunFactNextUpdateOn(subscriber.chatId, subscriber.nextUpdateOn.AddDays(CalcDaysToAddOnNextUpdate(subscriber.nextUpdateOn)));

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

        private async Task UpdateCountDowns()
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
