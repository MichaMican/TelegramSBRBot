using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Models.RedditPostResponse
{
    public class RedditPostData
    {
        public string modhash { get; set; }
        public int? dist { get; set; }
        public RedditPostChild[] children { get; set; }
    }
}
