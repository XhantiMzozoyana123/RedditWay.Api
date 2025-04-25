using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;

namespace RedditWay.Mvc.Controllers
{
    public class RedditMailController : Controller
    {
        private readonly IRedditMessagingService _mailService;
        private readonly IRedditApiExtractService _apiService;
        private readonly ApplicationDbContext _context;

        public RedditMailController(IRedditMessagingService mailService, IRedditApiExtractService apiService, ApplicationDbContext context)
        {
            _mailService = mailService;
            _apiService = apiService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessages(MailDto mailDto)
        {
            var accessToken = await _apiService.GetAccessTokenAsync(_context.RedditAccounts.FirstOrDefault());
            var result = await _mailService.PrepareMail(mailDto, accessToken);

            if (result)
                return Ok("Messages sent successfully.");
            else
                return StatusCode(500, "An error occurred while sending messages.");
        }

    }
}
