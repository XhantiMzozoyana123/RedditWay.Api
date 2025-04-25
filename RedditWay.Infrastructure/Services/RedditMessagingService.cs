using Azure.Core;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Channels;

namespace RedditWay.Infrastructure.Services
{
    public class RedditMessagingService : IRedditMessagingService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _client;
        public RedditMessagingService(ApplicationDbContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }

        public async Task<string> MessageBuilder(string text, RedditUsers user, RedditAccounts account)
        {
            return text.Replace("[name]", user.UserName);
        }

        public string ParseSpintax(string text)
        {
            Random random = new Random();

            string pattern = @"\{([^{}]*)\}";
            while (Regex.IsMatch(text, pattern))
            {
                text = Regex.Replace(text, pattern, match =>
                {
                    string[] options = match.Groups[1].Value.Split('|');
                    return options[random.Next(options.Length)];
                });
            }
            return text;
        }

        public async Task<bool> PrepareMail(MailDto messenger, string accessToken)
        {
            try
            {
                List<RedditUsers> redditUserList = new List<RedditUsers>();

                //Get all the reddit users that you've scrapped and put them into a list, group by their userName, so there is no duplicates...
                redditUserList = _context.RedditUsers.Where(x => x.Sent == false)
                        .GroupBy(g => g.UserName)
                        .Select(s => s.First())
                        .ToList();

                //Get me all of reddit account that I created for outreach...
                var redditAccountList = await _context.RedditAccounts.ToListAsync();
                
                //Use this index pointer to rotate around the account that I am using to send my messages
                int accountIndex = 0;

                foreach (var redditUser in redditUserList)
                {
                    MailDto messengerModel = new MailDto();
                    Random random = new Random();

                    //Create a integer to select any email message (content) in your a/b test of message...
                    int emailVariationIndex = messenger.AbTest ? random.Next(messenger.Variation.Count) : 1;

                    //Use the emailVariationIndex to select the content of the email...
                    var content = messenger.AbTest ? messenger.Variation[emailVariationIndex] : messenger.Content;

                    var subject = await MessageBuilder(content.Subject, redditUser, redditAccountList[accountIndex]);
                    var body = await MessageBuilder(content.Body, redditUser, redditAccountList[accountIndex]);

                    messengerModel.Content.Subject = subject;
                    messengerModel.Content.Body = body;
                    messengerModel.Lead = redditUser;
                    messengerModel.RedditAccount = redditAccountList[accountIndex];
                    messengerModel.RedditAccountList = redditAccountList;

                    await SendMail(messengerModel, accessToken);
                    accountIndex = (accountIndex + 1) % redditAccountList.Count;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> SendMail(MailDto messenger, string accessToken)
        {
            try 
            {
                var messageClient = new HttpClient();
                messageClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                messageClient.DefaultRequestHeaders.Add("User-Agent", "MyRedditBot/0.1 by " + messenger.RedditAccount.UserName);

                var messageContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("to", messenger.Lead.UserName),
                    new KeyValuePair<string, string>("subject", messenger.Content.Subject),
                    new KeyValuePair<string, string>("text", messenger.Content.Body)
                });

                var response = await messageClient.PostAsync("https://oauth.reddit.com/api/compose", messageContent);
                var result = await response.Content.ReadAsStringAsync();

                UpdateList(messenger);
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }

        public void UpdateList(MailDto messenger)
        {
            var query = _context.RedditUsers
                .Where(x => x.UserName == messenger.Lead.UserName)
                .FirstOrDefault();

            query.Sent = true;
            query.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
        }
    }
}
