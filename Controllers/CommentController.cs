using EliteTGTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EliteTGTask.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ApplicationDBContext context, UserManager<ApplicationUser> userManager, ILogger<CommentController> logger)
        {
            _dbContext = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Add comment via POST
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string text)
        {
            // Validate input
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest("Comment text cannot be empty.");
            }

            // Check if the user is authenticated and authorized
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Member"))
            {
                return Unauthorized();
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Create the comment
            var comment = new Comment
            {
                postId = postId,
                Text = text,
                UserId = user.Id
            };

            try
            {
                // Add the comment to the database
                _dbContext.Comments.Add(comment);
                await _dbContext.SaveChangesAsync();

                // Return the response with comment details
                return Json(new
                {
                    username = user.FullName,
                    text = comment.Text,
                    commentId = comment.Id
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error.");
            }
        }

        // Edit comment via POST
        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string text)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var comment = await _dbContext.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if the current user is the owner of the comment
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (comment.UserId != currentUserId)
            {
                return Unauthorized();
            }

            // Update the comment text
            comment.Text = text;
            comment.UpdatedAt = DateTime.Now;

            try
            {
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error.");
            }
        }

        // Delete comment via POST
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _dbContext.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
