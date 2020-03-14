using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes
{
    public class HttpHandler : IHttpHandler
    {
        private readonly HttpClient _client = new HttpClient();

        public HttpHandler()
        {
            _client.Timeout = new TimeSpan(0, 0, 30);
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await _client.GetAsync(url);
        }
    }
}
