using Azure.Core;
using Newtonsoft.Json.Linq;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YoutubeExplode.Channels;
using HtmlAgilityPack;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace RedditWay.Infrastructure.Services
{
    public class RedditApiExtractService : IRedditApiExtractService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _client;

        public RedditApiExtractService(ApplicationDbContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }

        public async Task<string> GetAccessTokenAsync(RedditAccounts account)
        {
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{account.ClientId}:{account.ClientSecret}"));

            var req = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            req.Headers.Add("User-Agent", "RedditWarmUpBot/0.1 by u/" + account.UserName);

            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = account.UserName,
                ["password"] = account.Password
            });

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get token: {res.StatusCode}\n{json}");
            }

            var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString();

            return accessToken;
        }


        public async Task<(int, int, DateTime)> GetUserKarmaInfo(SearchDto searchDto, string userName, string accessToken)
        {
            try
            {
                // Set up the headers for Reddit API authentication
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Add("User-Agent", "YourAppName/1.0");

                // Reddit API endpoint to get user information
                string url = $"https://oauth.reddit.com/user/{userName}/about";

                // Send request to Reddit API
                var response = await _client.GetStringAsync(url);

                // Parse the JSON response
                JObject userInfo = JObject.Parse(response);

                // Extract postKarma and commentKarma from the response
                int postKarma = (int)userInfo["data"]["link_karma"];
                int commentKarma = (int)userInfo["data"]["comment_karma"];
                DateTime cakeDay = DateTime.Now;

                return (postKarma, commentKarma, cakeDay);

            }
            catch (Exception ex)
            {
                return (0, 0, DateTime.Now);
            }
        }

        public async Task SearchPostAsync(SearchDto searchDto, string accessToken)
        {
            var url = $"https://oauth.reddit.com/r/{searchDto.Group}/search?q={Uri.EscapeDataString(searchDto.Keyword)}&limit={searchDto.Pages}&sort=new&restrict_sr=false";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            request.Headers.UserAgent.ParseAdd("RedditWayBot/1.0");

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            foreach (var post in doc.RootElement.GetProperty("data").GetProperty("children").EnumerateArray())
            {
                var data = post.GetProperty("data");
                var userName = data.GetProperty("author").GetString();
                var postCount = GetUserKarmaInfo(searchDto, userName, accessToken).Result.Item1;
                var commentCount = GetUserKarmaInfo(searchDto, userName, accessToken).Result.Item2;
                var cakeDate = GetUserKarmaInfo(searchDto, userName, accessToken).Result.Item3;

                if (postCount > searchDto.PostKarma.Min && postCount < searchDto.PostKarma.Max)
                {
                    if (commentCount > searchDto.CommentKarma.Min && commentCount < searchDto.CommentKarma.Max)
                    {
                        if (searchDto.HasYouTubeChannel)
                        {
                            var channelUrl = await ExtractYouTubeUrl(userName);

                            if (channelUrl != "")
                            {
                                var user = new RedditUsers
                                {
                                    UserName = userName,
                                    PostKarma = postCount,
                                    CommentKarma = commentCount,
                                    CakeDate = cakeDate,
                                    ExternalLink = await ExtractYouTubeUrl(userName),
                                    Group = searchDto.Keyword,
                                    CreatedDate = DateTime.Now
                                };

                                _context.RedditUsers.Add(user);
                                _context.SaveChanges();

                                await GetRandomYouTubeVideo(user);
                            }
                        }
                        else
                        {
                            var user = new RedditUsers
                            {
                                UserName = userName,
                                PostKarma = postCount,
                                CommentKarma = commentCount,
                                CakeDate = cakeDate,
                                ExternalLink = await ExtractYouTubeUrl(userName),
                                Group = searchDto.Keyword,
                                CreatedDate = DateTime.Now
                            };

                            _context.RedditUsers.Add(user);
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }

        public async Task<string> ExtractYouTubeUrl(string userName)
        {
            string youtubeLink = string.Empty;

            try
            {
                var url = $"https://www.reddit.com/user/{userName}/";
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Find the div with class="mb-sm"
                var node = doc.DocumentNode
                              .Descendants("div")
                              .FirstOrDefault(n => n.GetAttributeValue("class", "").Contains("mb-sm"));

                if (node != null)
                {
                    var innerDoc = new HtmlDocument();
                    innerDoc.LoadHtml(node.InnerHtml);

                    youtubeLink = innerDoc.DocumentNode
                        .Descendants("a")
                        .Select(a => a.GetAttributeValue("href", ""))
                        .FirstOrDefault(href => href.Contains("youtube.com") || href.Contains("youtu.be"));

                }
            }
            catch (Exception)
            {
                return null;
            }

            return youtubeLink;
        }

        public async Task<RedditYouTubeChannels> GetRandomYouTubeVideo(RedditUsers redditUsers)
        {
            var youtube = new YoutubeClient();
            RedditYouTubeChannels redditYouTubeChannels = new RedditYouTubeChannels();

            try
            {
                var uploads = await youtube.Channels.GetUploadsAsync(redditUsers.ExternalLink);
                var videos = await youtube.Playlists.GetVideosAsync(uploads.FirstOrDefault().Id.Value);
                // Get available caption tracks for this video
                var tracks = await youtube.Videos.ClosedCaptions.GetManifestAsync(videos.FirstOrDefault().Id);

                // Try get the English auto-generated or manual captions
                var trackInfo = tracks.GetByLanguage("en") ?? tracks.Tracks.FirstOrDefault();
                var videoList = videos.ToList();

                // Pick a random video
                var random = new Random();
                var selectedVideo = videoList[random.Next(videoList.Take(10).Count())];

                redditYouTubeChannels.RedditUserId = redditUsers.Id;
                redditYouTubeChannels.Title = selectedVideo.Title;
                redditYouTubeChannels.Duration = selectedVideo.Duration.ToString();
                redditYouTubeChannels.Captions = "";
                redditYouTubeChannels.Url = $"https://www.youtube.com/watch?v={selectedVideo.Id}";
                redditYouTubeChannels.CreatedDate = DateTime.Now;

                _context.RedditYouTubeChannels.Add(redditYouTubeChannels);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return null;
            }

            return redditYouTubeChannels;
        }
    }
}
