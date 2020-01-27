using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TelegramFunFactBot.Controllers
{
    [Route("api/[controller]")]
    public class TelegramController : Controller
    {

        private static readonly HttpClient client = new HttpClient();
        private Settings AppSettings { get; set; }


        public TelegramController(IOptions<Settings> settings)
        {
            AppSettings = settings.Value;
        }


        public class MessageObj
        {
            public string chat_id;
            public string text;
        }

        [HttpPost("new-message")]
        public ActionResult NewMessage([FromBody]dynamic body)
        {
            var answer = new MessageObj();
            try
            {
                answer.chat_id = body.message.chat.id;
                answer.text = body.message.text;
            }
            catch
            {
                return Ok();
            }

            var url = "https://api.telegram.org/bot"+ AppSettings.botToken + "/sendMessage";

            var json = JsonConvert.SerializeObject(answer);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = client.PostAsync(url,content).Result;


            return Ok();
        }
    }
}