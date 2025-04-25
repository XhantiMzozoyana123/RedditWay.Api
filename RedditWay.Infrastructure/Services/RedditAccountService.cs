using Microsoft.EntityFrameworkCore;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Infrastructure.Services
{
    public class RedditAccountService : IRedditAccountService
    {
        private ApplicationDbContext _context;
        
        public RedditAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRedditAccount(RedditAccounts redditAccounts)
        {
            try
            {
                _context.RedditAccounts.Add(redditAccounts);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRedditAccount(int Id)
        {
            try
            {
                var query = await GetRedditAccountById(Id);

                _context.RedditAccounts.Remove(query);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<List<RedditAccounts>> GetRedditAccount(string UserId)
        {
            return await _context.RedditAccounts
                            .Where(x => x.UserId == UserId)
                            .ToListAsync();
        }

        public async Task<RedditAccounts> GetRedditAccountById(int Id)
        {
            return await _context.RedditAccounts.FirstAsync(x => x.Id == Id);
        }

        public async Task<bool> UpdateRedditAccount(RedditAccounts redditAccounts)
        {
            try
            {
                var query = await GetRedditAccountById(redditAccounts.Id);

                query = redditAccounts;
                query.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
