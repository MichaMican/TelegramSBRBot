using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.HelperClasses;
using TelegramFunFactBot.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace TelegramFunFactBot.Classes
{
    public class TelegramAPICommunicator : ITelegramAPICommunicator
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly Settings _settings;
        public TelegramAPICommunicator(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public void SendImage(string chatId, string imageURL, string caption = "", string parse_mode = "")
        {
            var answer = new TelegramAPIImageMessage();
            answer.chat_id = chatId;
            answer.photo = imageURL;
            answer.caption = caption;
            answer.parse_mode = parse_mode;


            var url = "https://api.telegram.org/bot" + _settings.botToken + "/sendPhoto";

            var json = JsonConvert.SerializeObject(answer);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = _client.PostAsync(url, content).Result;
        }

        public void SendMessage(string chatId, string message, string parse_mode = "html")
        {
            var answer = new TelegramAPIMessage();
            answer.chat_id = chatId;
            answer.text = message;
            answer.parse_mode = parse_mode;

            var url = "https://api.telegram.org/bot" + _settings.botToken + "/sendMessage";

            var json = JsonConvert.SerializeObject(answer);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = _client.PostAsync(url, content).Result;
        }
    }
}
