using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

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

        // Create Post
        [HttpGet]
        [Authorize(Roles = "Editor")]
        public IActionResult Create()
        {
            Debug.WriteLine("Create GET action called.");
            ViewBag.Categories = _Context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, List<int> CategoryIds, IFormFile? ImageFile)
        {
            Debug.WriteLine("Create POST action called.");

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState is invalid.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            Debug.WriteLine("ModelState is valid.");

            // Check if CategoryIds is null or empty
            if (CategoryIds == null || !CategoryIds.Any())
            {
                Debug.WriteLine("No categories selected.");
                ModelState.AddModelError("", "Please select at least one category.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            Debug.WriteLine("Categories selected: " + string.Join(", ", CategoryIds));

            string imagePath = "/images/default.jpg"; // Set default image path

            if (ImageFile != null && ImageFile.Length > 0)
            {
                Debug.WriteLine("Image file uploaded.");

                var imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Debug.WriteLine("Creating images directory.");
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);

                Debug.WriteLine("Saving image to: " + filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/" + fileName; // Use uploaded image
                Debug.WriteLine("Image saved successfully. Path: " + imagePath);
            }
            else
            {
                Debug.WriteLine("No image file uploaded. Using default image.");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("User ID is null or empty.");
                ModelState.AddModelError("", "User not authenticated.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            Debug.WriteLine("User ID: " + userId);

            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = imagePath, // Either uploaded image or default image
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                PostCategories = CategoryIds.Select(id => new PostCategory { CategoryId = id }).ToList()
            };

            Debug.WriteLine("Post object created.");

            try
            {
                _Context.Posts.Add(post);
                await _Context.SaveChangesAsync();
                Debug.WriteLine("Post saved to database successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving post to database: " + ex.Message);
                ModelState.AddModelError("", "An error occurred while saving the post.");
                ViewBag.Categories = _Context.Categories.ToList();
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        // In PostController.cs

        [HttpGet]
        // In PostController.cs

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _Context.Posts
                .Include(p => p.Comments) // Include related comments
                .Include(p => p.User) // Include user information (if not loaded already)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post); // Return the single post to the Details view
        }



        // Edit Post
        [HttpGet]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(int id)
        {
            Debug.WriteLine("Edit GET action called for post ID: " + id);
            var post = await _Context.Posts.FindAsync(id);
            if (post == null)
            {
                Debug.WriteLine("Post not found.");
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post Model)
        {
            Debug.WriteLine("Edit POST action called for post ID: " + id);

            if (id != Model.Id)
            {
                Debug.WriteLine("ID mismatch.");
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _Context.Update(Model);
                    await _Context.SaveChangesAsync();
                    Debug.WriteLine("Post updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_Context.Posts.Any(p => p.Id == id))
                    {
                        Debug.WriteLine("Post not found during update.");
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index", "Home");
            }

            Debug.WriteLine("ModelState is invalid.");
            return View(Model);
        }

        // Delete Post
        [HttpGet]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(int id)
        {
            Debug.WriteLine("Delete GET action called for post ID: " + id);
            var post = await _Context.Posts.FindAsync(id);
            if (post == null)
            {
                Debug.WriteLine("Post not found.");
                return NotFound();
            }

            _Context.Posts.Remove(post);
            await _Context.SaveChangesAsync();
            Debug.WriteLine("Post deleted successfully.");
            return RedirectToAction("Index", "Home");
        }

        // Load More Posts
        [HttpGet]
        public async Task<IActionResult> LoadMorePosts(int skip)
        {
            Debug.WriteLine("LoadMorePosts action called. Skip: " + skip);
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

            Debug.WriteLine("Posts loaded: " + posts.Count);
            return Json(posts);
        }
    }
}
