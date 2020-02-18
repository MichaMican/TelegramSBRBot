using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot.Classes.RedditPostsClasses
{
    public class RedditPostHandler : IRedditPostHandler
    {
        private readonly IHttpHandler _httpHandler;

        public RedditPostHandler(IHttpHandler httpHandler)
        {
            _httpHandler = httpHandler;
        }

        public async Task<List<RedditPostData>> GetRedditTopPostsData(string subreddit, int maxNumberOfPosts)
        {
            var returnList = new List<RedditPostData>();

            var response = await _httpHandler.Get("https://www.reddit.com/r/" + subreddit + "/top.json?limit=" + maxNumberOfPosts + "&raw_json=1");
            string responseBody = await response.Content.ReadAsStringAsync();

            dynamic data = JsonConvert.DeserializeObject(responseBody);

            for (int i = 0; i < maxNumberOfPosts; i++)
            {
                try
                {
                    var post = new RedditPostData()
                    {
                        title = data.data.children[i].data.title,
                        permalink = data.data.children[i].data.permalink
                    };

                    
                    try
                    {
                        post.imageUrl = data.data.children[i].data.preview.images[0].source.url; //imageUrl needn't be provided - thats why this try catch is necessary
                    }
                    catch
                    {
                        /* Fall through */
                    }

                    returnList.Add(post);
                    
                }
                catch
                {
                    //if this fails it means, that there were less then the through maxNumberOfPosts provided posts
                    break;
                }
                
            }

            return returnList;
        }


        public async Task<RedditPostData> GetRedditTopPostWithImageData(string subreddit, int maxNumberOfPosts)
        {
            var topPosts = await GetRedditTopPostsData(subreddit, maxNumberOfPosts);
   

            foreach (RedditPostData post in topPosts)
            {
                if (!String.IsNullOrEmpty(post.imageUrl))
                {
                    return post;
                }
            }
            //if there are no topPosts of the day or if there is no top post with image 
            return null;
        }
    }
}
