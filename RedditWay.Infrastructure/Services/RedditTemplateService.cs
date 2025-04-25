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
    public class RedditTemplateService : IRedditTemplateService
    {
        private ApplicationDbContext _context;
        public RedditTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRedditTemplate(RedditTemplates redditTemplates)
        {
            try 
            {
                _context.RedditTemplates.Add(redditTemplates);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRedditTemplate(int Id)
        {
            try
            {
                var query = await GetRedditTemplateById(Id);

                _context.RedditTemplates.Remove(query);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<RedditTemplates> GetRedditTemplateById(int Id)
        {
            return await _context.RedditTemplates.FirstAsync(x => x.Id == Id);
        }

        public async Task<List<RedditTemplates>> GetRedditTemplates(string UserId)
        {
            return await _context.RedditTemplates
                .Where(x => x.UserId == UserId)
                .ToListAsync();
        }

        public async Task<bool> UpdateRedditTemplate(RedditTemplates redditTemplates)
        {
            try
            {
                var query = await GetRedditTemplateById(redditTemplates.Id);

                query = redditTemplates;
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
