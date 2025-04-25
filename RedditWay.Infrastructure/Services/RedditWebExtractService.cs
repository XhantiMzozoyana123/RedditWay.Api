using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using RedditWay.Application.Interfaces;
using RedditWay.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditWay.Domain;
using RedditWay.Domain.Entities;
using HtmlAgilityPack;
using YoutubeExplode;
using AngleSharp.Common;
using YoutubeExplode.Common;

namespace RedditWay.Infrastructure.Services
{
    public class RedditWebExtractService : IRedditWebExtractService
    {
        private ApplicationDbContext _context;
        
        public RedditWebExtractService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Extraction(SearchDto searchDto)
        {
            try 
            {
                var options = new EdgeOptions();
                //options.AddArgument("headless"); // enable headless mode
                //options.AddArgument("disable-gpu"); // disable GPU for better compatibility

                using var driver = new EdgeDriver(options);
                var baseUrl = $"https://www.reddit.com/r/{searchDto.Keyword}/{searchDto.SortBy}/";


                driver.Navigate().GoToUrl(baseUrl);
                Thread.Sleep(3000); // initial load

                HashSet<string> collectedTexts = new HashSet<string>();
                int scrolls = 0;

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                while (scrolls < searchDto.Pages)
                {
                    var spans = driver.FindElements(By.CssSelector("span.whitespace-nowrap"));
                    int foundThisScroll = 0;

                    foreach (var span in spans)
                    {
                        string text = span.Text.Trim();
                        if (!string.IsNullOrEmpty(text) && collectedTexts.Add(text))
                        {
                            foundThisScroll++;
                        }
                    }

                    // Scroll to bottom
                    long currentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight"));
                    js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    Thread.Sleep(2500); // wait for more to load
                    long newHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight"));

                    if (newHeight == currentHeight)
                    {
                        break;
                    }

                    scrolls++;
                }

                foreach (var userName in collectedTexts)
                {
                    RedditUsers redditUsers = await ExtractProfileDetails(searchDto, userName.Split('/')[1]);

                    if (redditUsers.UserName != null)
                    {
                        redditUsers.UserId = "";
                        _context.RedditUsers.Add(redditUsers);
                        await _context.SaveChangesAsync();
                    }
                }

                driver.Quit();
            }
            catch(Exception)
            {
                return false;
            }

            return true;
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
            catch(Exception)
            {
                return null;
            }

            return youtubeLink;
        }

        public async Task<RedditUsers> ExtractProfileDetails(SearchDto searchDto, string userName)
        {
            RedditUsers redditUsers = new RedditUsers();

            try
            {
                var url = $"https://www.reddit.com/user/{userName}/";
                var httpClient = new HttpClient();

                // Reddit requires a User-Agent header
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; HtmlAgilityPackBot/1.0)");

                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var parentNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'flex') and contains(@class, 'justify-between') and contains(@class, 'gap-x-sm') and contains(@class, 'my-md')]");
                // Find all nested elements with data-testid="karma-number" inside the parent
                var karmaNodes = parentNode.SelectNodes(".//*[@data-testid='karma-number']");
                var cakeDateNodes = parentNode.SelectNodes(".//*[@data-testid='cake-day']");

                int postCount = int.Parse(karmaNodes[0].InnerText.Trim().Replace(",", ""));
                int commentCount = int.Parse(karmaNodes[0].InnerText.Trim().Replace(",", ""));

                if(postCount > searchDto.PostKarma.Min && postCount < searchDto.PostKarma.Max)
                {
                    if (commentCount > searchDto.CommentKarma.Min && commentCount < searchDto.CommentKarma.Max)
                    {
                        if(searchDto.HasYouTubeChannel)
                        {
                            var channelUrl = await ExtractYouTubeUrl(userName);

                            if (channelUrl != "")
                            {
                                redditUsers.UserName = userName;
                                redditUsers.PostKarma = postCount;
                                redditUsers.CommentKarma = commentCount;
                                redditUsers.Group = "";
                                redditUsers.ExternalLink = await ExtractYouTubeUrl(userName);
                                redditUsers.CakeDate = DateTime.Parse(cakeDateNodes[0].InnerText.Trim().Replace(",", ""));
                            }
                        }
                        else
                        {
                            redditUsers.UserName = userName;
                            redditUsers.PostKarma = postCount;
                            redditUsers.CommentKarma = commentCount;
                            redditUsers.Group = "";
                            redditUsers.ExternalLink = await ExtractYouTubeUrl(userName);
                            redditUsers.CakeDate = DateTime.Parse(cakeDateNodes[0].InnerText.Trim().Replace(",", ""));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return redditUsers;
        }

        public async Task<DateTime> ExtractCakeDate(string userName)
        {
            DateTime result = new DateTime();

            try
            {
                var url = $"https://www.reddit.com/user/{userName}/";
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Find the <time> tag
                var timeTag = doc.DocumentNode
                                 .Descendants("time")
                                 .FirstOrDefault();

                if (timeTag != null)
                {
                    var datetime = timeTag.GetAttributeValue("datetime", "not found");

                    result = DateTime.Parse(datetime);
                }
            }
            catch(Exception)
            {
                return DateTime.Now;
            }

            return result;
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
