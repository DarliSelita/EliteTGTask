using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

namespace EliteTGTask.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDBContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDBContext context,
                              IWebHostEnvironment webHostEnvironment,
                              UserManager<ApplicationUser> userManager)
        {
            _Context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }


        //Krijo 1 postim
        [Authorize(Roles ="Editor")]
        public IActionResult Create()
        {
            ViewBag.Categories = _Context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles ="Editor")]
        public async Task<IActionResult> Create(Post model, List<int> CategoryIds, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            string imagePath = null;
            if(ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                imagePath = "/images/" + fileName;

        }
            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = imagePath,
                CreatedAt = DateTime.UtcNow,
                UserId = _userManager.GetUserId(User),
                PostCategories = CategoryIds.Select(id => new PostCategory { CategoryId = id }).ToList()
            };

            _Context.Posts.Add(post);
            await _Context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Edit Postimet
        [HttpGet]
        [Authorize(Roles ="Editor")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _Context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post Model)
        {
            if (id != Model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _Context.Update(Model);
                    await _Context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_Context.Posts.Any(p => p.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Index", "Home");
            }
            return View(Model);
        }

        //Delete Action

        [HttpGet]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _Context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _Context.Posts.Remove(post);
            await _Context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> LoadMorePosts(int skip)
        {
            var posts = await _Context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    description = p.Description,
                    createdDate = p.CreatedAt.ToString("yyyy-MM-dd"),
                    author = p.User.FullName
                })
                .ToListAsync();

            return Json(posts);
        }

    }
}
