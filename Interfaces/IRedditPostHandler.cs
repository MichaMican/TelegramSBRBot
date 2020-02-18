using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Classes.RedditPostsClasses;

namespace TelegramFunFactBot.Interfaces
{
    public interface IRedditPostHandler
    {
        Task<List<RedditPostData>> GetRedditTopPostsData(string subreddit, int maxNumberOfPosts);
        Task<RedditPostData> GetRedditTopPostWithImageData(string subreddit, int maxNumberOfPosts);
    }
}
