using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditApiExtractService
    {
        Task<string> GetAccessTokenAsync(RedditAccounts account);

        Task SearchPostAsync(SearchDto searchDto, string accessToken);

        Task<(int, int, DateTime)> GetUserKarmaInfo(SearchDto searchDto, string userName, string accessToken);

        public Task<string> ExtractYouTubeUrl(string userName);

        public Task<RedditYouTubeChannels> GetRandomYouTubeVideo(RedditUsers redditUsers);

    }
}
