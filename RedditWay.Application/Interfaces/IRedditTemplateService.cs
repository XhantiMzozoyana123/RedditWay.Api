using OpenQA.Selenium.BiDi.Modules.Script;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditTemplateService
    {
        public Task<bool> AddRedditTemplate(RedditTemplates redditTemplates);

        public Task<List<RedditTemplates>> GetRedditTemplates(string UserId);

        public Task<RedditTemplates> GetRedditTemplateById(int Id);

        public Task<bool> UpdateRedditTemplate(RedditTemplates redditTemplates);

        public Task<bool> DeleteRedditTemplate(int Id);
    }
}
