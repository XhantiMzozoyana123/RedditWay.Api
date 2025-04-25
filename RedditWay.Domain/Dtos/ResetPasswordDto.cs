using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Domain.Dtos
{
    public class ResetPasswordDto
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
