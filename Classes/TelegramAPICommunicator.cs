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

        public void SendMessage(string message, string chatId)
        {
            var answer = new TelegramAPIMessage();
            answer.chat_id = chatId;
            answer.text = message;

            var url = "https://api.telegram.org/bot" + _settings.botToken + "/sendMessage";

            var json = JsonConvert.SerializeObject(answer);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = _client.PostAsync(url, content).Result;
        }
    }
}
