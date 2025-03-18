using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;

namespace EliteTGTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDBContext _dbContext;

        public HomeController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Controller per faqen kryesore te blog-ut
        public async Task<IActionResult> Index(string SearchKeyword, int categoryId = 0, int page = 1)
        {
            var postsQuery = _dbContext.Posts
                .Include(p => p.User) // Include the user who created the post
                .Include(p => p.PostCategories) // Include post categories
                .ThenInclude(pc => pc.Category) // Include categories
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            // Search by keyword
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(SearchKeyword) || p.Description.Contains(SearchKeyword));
            }

            // Filter by category
            if (categoryId > 0)
            {
                postsQuery = postsQuery.Where(p => p.PostCategories.Any(pc => pc.CategoryId == categoryId));
            }

            // Pagination
            int PageSize = 10;
            var posts = await postsQuery.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Ceiling((double)await postsQuery.CountAsync() / PageSize);

            // Debugging: Log the number of posts
            Debug.WriteLine($"Number of posts fetched: {posts.Count}");

            return View(posts);
        }

        // Ketu do krijohen postime, vetem per rolin e editors
        [HttpGet]
        [Authorize(Roles ="Editor")]
        public IActionResult CreatePost()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles ="Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Post Model)
        {
            if (ModelState.IsValid)
            {
                Model.CreatedAt = DateTime.Now;
                _dbContext.Add(Model);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(Model);
        }
    }
}
