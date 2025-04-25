using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Entities
{
    public class RedditYouTubeChannels : BaseEntity
    {
        public int RedditUserId { get; set; }

        public string? Title { get; set; }

        public string? Duration { get; set; }

        public string? Captions { get; set; }

        public string? Url { get; set; }
    }
}
