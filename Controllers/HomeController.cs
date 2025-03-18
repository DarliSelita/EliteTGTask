using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.IO;

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
                .Include(p => p.User)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            // Shtojme kategority tek ViewBag qe te popullohen dhe tek View
            ViewBag.Categories = await _dbContext.Categories.ToListAsync();

            // Keyword
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(SearchKeyword) || p.Description.Contains(SearchKeyword));
            }

            // Category
            if (categoryId > 0)
            {
                postsQuery = postsQuery.Where(p => p.PostCategories.Any(pc => pc.CategoryId == categoryId));
            }

            // Pagination
            int PageSize = 10;
            var posts = await postsQuery.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Ceiling((double)await postsQuery.CountAsync() / PageSize);

            return View(posts);
        }


        // Ketu do krijohen postime, vetem nga editors duke perdorur authorize
        [HttpGet]
        [Authorize(Roles = "Editor")]
        public IActionResult CreatePost()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
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
        public IActionResult ExportPostToExcel(int id)
        {
            var post = _dbContext.Posts
                .Include(p => p.User)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Post Details");

                worksheet.Cell(1, 1).Value = "Nr";
                worksheet.Cell(1, 2).Value = "Id";
                worksheet.Cell(1, 3).Value = "Date";
                worksheet.Cell(1, 4).Value = "Title";
                worksheet.Cell(1, 5).Value = "Description";
                worksheet.Cell(1, 6).Value = "Categories";
                worksheet.Cell(1, 7).Value = "Full Name of Creator";

                worksheet.Cell(2, 1).Value = 1;
                worksheet.Cell(2, 2).Value = post.Id;
                worksheet.Cell(2, 3).Value = post.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(2, 4).Value = post.Title;
                worksheet.Cell(2, 5).Value = post.Description;
                worksheet.Cell(2, 6).Value = string.Join(", ", post.PostCategories.Select(pc => pc.Category.Name));
                worksheet.Cell(2, 7).Value = post.User.FullName;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Post_{post.Id}.xlsx");
                }
            }
        }

    }
}
