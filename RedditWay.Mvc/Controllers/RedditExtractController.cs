using Microsoft.AspNetCore.Mvc;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using System.Threading.Tasks;

namespace RedditWay.Mvc.Controllers
{
    public class RedditExtractController : Controller
    {
        private readonly IRedditWebExtractService _webService;
        private readonly IRedditApiExtractService _apiService;
        private readonly ApplicationDbContext _context;

        public RedditExtractController(IRedditWebExtractService webService, IRedditApiExtractService apiService, ApplicationDbContext context)
        {
            _webService = webService;
            _apiService = apiService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExtractUsers(SearchDto dto)
        {
            if (!ModelState.IsValid)
                return View("Index");

            if(dto.Type == "API")
            {
                var accessToken = await _apiService.GetAccessTokenAsync(_context.RedditAccounts.FirstOrDefault());
                await _apiService.SearchPostAsync(dto, accessToken);
            }
            else
            {
                await _webService.Extraction(dto);
            }

            return View("Index");
        }
    }
}
