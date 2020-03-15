using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;
using TelegramFunFactBot.Models.RedditPostResponse;

namespace TelegramFunFactBot.Classes
{
    public class RedditPostHandler : IRedditPostHandler
    {
        private readonly IHttpHandler _httpHandler;

        public RedditPostHandler(IHttpHandler httpHandler)
        {
            _httpHandler = httpHandler;
        }

        public async Task<List<RedditPostChildData>> GetRedditTopPostsData(string subreddit, int maxNumberOfPosts)
        {
            var returnList = new List<RedditPostChildData>();

            var response = await _httpHandler.Get("https://www.reddit.com/r/" + subreddit + "/top.json?limit=" + maxNumberOfPosts + "&raw_json=1");
            string responseBody = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<RedditPostAPIResponse>(responseBody);

            foreach(var redditPost in data.data.children)
            {
                returnList.Add(redditPost.data);
            }

            return returnList;
        }


        public RedditPostChildData GetPostWithImageData(List<RedditPostChildData> data)
        {
            foreach (var post in data)
            {
                if (!String.IsNullOrEmpty(post.url))
                {
                    return post;
                }
            }
            //if there are no topPosts of the day or if there is no top post with image 
            return null;
        }

        public async Task<List<RedditPostChildData>> GetRedditRandomPostsData(string subreddit, int maxNumberOfPosts)
        {
            var returnList = new List<RedditPostChildData>();

            var response = await _httpHandler.Get("https://www.reddit.com/r/" + subreddit + "/random.json?raw_json=1");
            string responseBody = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<RedditPostAPIResponse[]>(responseBody);

            foreach(var post in data)
            {
                foreach (var redditPost in post.data.children)
                {
                    returnList.Add(redditPost.data);
                }
            }

            return returnList;
        }
    }
}
