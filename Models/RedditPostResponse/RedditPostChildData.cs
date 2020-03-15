using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramFunFactBot.Models.RedditPostResponse
{
    public class RedditPostChildData
    {
        public string title { get; set; }
        public string permalink { get; set; }
        public string url { get; set; }
        public RedditPostChildDataPreview preview { get; set; }

    }
}
