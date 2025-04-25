using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Entities
{
    public class AppInfo
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string DomainName { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string ImapHost { get; set; }

        public int ImapPort { get; set; }
    }
}
