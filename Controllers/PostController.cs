using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EliteTGTask.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDBContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment; // perdorim webhostenviroment qe te bejme retrieve path-in e wwwroot-it per te bere store images, ose per te perdorur nje default image nese useri nuk ben upload.
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDBContext context,
                              IWebHostEnvironment webHostEnvironment,
                              UserManager<ApplicationUser> userManager)
        {
            _Context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Editor")]
        public IActionResult Create()
        {
            ViewBag.Categories = _Context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, List<int> CategoryIds, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            if (CategoryIds == null || !CategoryIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one category.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            string imagePath = "/images/default.jpg";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/" + fileName;
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User not authenticated.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = imagePath,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                PostCategories = CategoryIds.Select(id => new PostCategory { CategoryId = id }).ToList()
            };

            try
            {
                _Context.Posts.Add(post);
                await _Context.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the post...");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _Context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpGet]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _Context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _Context.Update(model);
                    await _Context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!_Context.Posts.Any(p => p.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _Context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

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
                    author = p.User.FullName,
                    imageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Json(posts);
        }
    }
}
