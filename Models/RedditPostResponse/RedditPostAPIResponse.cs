using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Models.RedditPostResponse
{
    public class RedditPostAPIResponse
    {
        public string kind { get; set; }
        public RedditPostData data { get; set; }
    }
}
