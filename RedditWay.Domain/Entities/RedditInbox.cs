using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Entities
{
    public class RedditInbox : BaseEntity
    {
        public string? MessageId { get; set; }

        public string? AccoutId { get; set; }

        public string? Recipient { get; set; }

        public string? Subject { get; set; }

        public string? Body { get; set; }

        public bool IsRead { get; set; }

        public string? Status { get; set; }
    }
}
