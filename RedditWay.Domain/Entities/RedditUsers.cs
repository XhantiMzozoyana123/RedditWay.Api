using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Entities
{
    public class RedditUsers : BaseEntity
    {
        public string? UserName { get; set; }

        public int PostKarma { get; set; }

        public int CommentKarma { get; set; }

        public string? Group { get; set; }

        public string? ExternalLink { get; set; }

        public bool Sent { get; set; }

        public DateTime CakeDate { get; set; }
    }
}
