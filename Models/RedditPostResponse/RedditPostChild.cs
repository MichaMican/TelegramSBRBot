using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Models.RedditPostResponse
{
    public class RedditPostChild
    {
        public string kind { get; set; }
        public RedditPostChildData data { get; set; }
    }
}
