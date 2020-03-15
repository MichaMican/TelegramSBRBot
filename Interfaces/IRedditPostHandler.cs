using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Models.RedditPostResponse;

namespace TelegramFunFactBot.Interfaces
{
    public interface IRedditPostHandler
    {
        Task<List<RedditPostChildData>> GetRedditTopPostsData(string subreddit, int maxNumberOfPosts);
        RedditPostChildData GetPostWithImageData(List<RedditPostChildData> data);
        Task<List<RedditPostChildData>> GetRedditRandomPostsData(string subreddit, int maxNumberOfPosts);
    }
}
