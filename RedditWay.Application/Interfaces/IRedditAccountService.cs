using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditAccountService
    {
        public Task<bool> AddRedditAccount(RedditAccounts redditAccounts);

        public Task<List<RedditAccounts>> GetRedditAccount(string UserId);

        public Task<RedditAccounts> GetRedditAccountById(int Id);

        public Task<bool> UpdateRedditAccount(RedditAccounts redditAccounts);

        public Task<bool> DeleteRedditAccount(int Id);
    }
}
