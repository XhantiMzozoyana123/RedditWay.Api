using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditInboxService
    {
        Task<List<RedditInbox>> GetInboxMessages();

        Task<bool> ReplyToMessage(string messageId, string replyText, string accessToken, string userAgent);

    }
}
