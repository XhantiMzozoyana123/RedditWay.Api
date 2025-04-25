using OpenQA.Selenium.DevTools.V133.FedCm;
using RedditWay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Dtos
{
    public class MailDto
    {
        public string UserId { get; set; }

        public MailContent Content { get; set; } = new MailContent();

        public List<MailContent> Variation { get; set; } = new List<MailContent>();
        
        public RedditAccounts RedditAccount { get; set; } = new RedditAccounts();
        
        public List<RedditAccounts> RedditAccountList { get; set; } = new List<RedditAccounts>();

        public RedditUsers Lead { get; set; } = new RedditUsers();

        public bool AbTest { get; set; }

        public bool AccountRotation { get; set; }

        public int Delay { get; set; }
    }

    public class MailContent 
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
