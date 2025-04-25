using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditMessagingService
    {
        Task<bool> PrepareMail(MailDto messenger, string accessToken);

        Task<bool> SendMail(MailDto messenger, string accessToken);

        Task<string> MessageBuilder(string text, RedditUsers user, RedditAccounts account);

        string ParseSpintax(string text);

        void UpdateList(MailDto messenger);
    }
}
