using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Dtos
{
    public class SearchDto
    {
        public string? UserId { get; set; }

        public string? Keyword { get; set; }

        public string? Type { get; set; }

        public string? Group { get; set; }

        public int Pages { get; set; }

        public string? SortBy { get; set; }

        public bool HasYouTubeChannel { get; set; }

        public PostKarma? PostKarma { get; set; }

        public CommentKarma? CommentKarma { get; set; }
    }

    public class PostKarma 
    {
        public int Min { get; set; }

        public int Max { get; set; }

    }

    public class CommentKarma
    {
        public int Min { get; set; }

        public int Max { get; set; }
    }
}
