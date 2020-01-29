using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Interfaces
{
    public interface IHttpHandler
    {
        Task<HttpResponseMessage> Get(string url);
    }
}
