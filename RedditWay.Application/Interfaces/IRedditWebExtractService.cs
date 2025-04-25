using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditWebExtractService
    {
        public Task<bool> Extraction(SearchDto searchDto);

        public Task<string> ExtractYouTubeUrl(string userName);

        public Task<DateTime> ExtractCakeDate(string userName);

        public Task<RedditUsers> ExtractProfileDetails(SearchDto searchDto, string userName);

        Task<RedditYouTubeChannels> GetRandomYouTubeVideo(RedditUsers redditUsers);
    }
}
