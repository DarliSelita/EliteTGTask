using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;

namespace EliteTGTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDBContext _dbContext;

        public HomeController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Blog Index
        public async Task<IActionResult> Index(string SearchKeyword, int categoryId = 0, int page = 1)
        {
            var postsQuery = _dbContext.Posts
                .Include(p => p.User)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(SearchKeyword) || p.Description.Contains(SearchKeyword));
            }

            if (categoryId > 0)
            {
                postsQuery = postsQuery.Where(p => p.PostCategories.Any(pc => pc.CategoryId == categoryId));
            }


            return View();
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
