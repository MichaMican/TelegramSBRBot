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
using TelegramFunFactBot.Models;

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

        private class MessageResponse
        {
            public bool ok { get; set; }
            public Message result { get; set; }
        }


        public async Task<Message> SendMessage(string chatId, string message, string parse_mode = "html")
        {
            var answer = new TelegramAPIMessage();
            answer.chat_id = chatId;
            answer.text = message;
            answer.parse_mode = parse_mode;

            var url = "https://api.telegram.org/bot" + _settings.botToken + "/sendMessage";

            var json = JsonConvert.SerializeObject(answer);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(url, content);

            var resString = await res.Content.ReadAsStringAsync();

            return (JsonConvert.DeserializeObject<MessageResponse>(resString)).result;
        }

        private class UpdateMessageBody
        {
            public string chat_id { get; set; }
            public int message_id { get; set; }
            public string text { get; set; }
            public string parse_mode { get; set; }
        }

        public async void UpdateMessage(string chatId, int messageId, string text, string parse_mode = "html")
        {
            var body = new UpdateMessageBody() { chat_id = chatId, message_id = messageId, text = text, parse_mode=parse_mode };

            var url = "https://api.telegram.org/bot" + _settings.botToken + "/editMessageText";

            var json = JsonConvert.SerializeObject(body);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(url, content);
        }
    }
}
