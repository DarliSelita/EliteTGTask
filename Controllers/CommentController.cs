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

        // Add Comment via AJAX
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string text)
        {
            _logger.LogInformation("Received request to add comment. Post ID: {PostId}, Text: {Text}", postId, text);

            // Validate input
            if (string.IsNullOrEmpty(text))
            {
                _logger.LogWarning("Comment text is null or empty.");
                return BadRequest("Comment text cannot be empty.");
            }

            // Check if the user is authenticated and authorized
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Member"))
            {
                _logger.LogWarning("Unauthorized attempt to add a comment.");
                return Unauthorized();
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Authenticated user not found in database.");
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
                _logger.LogInformation("Comment added successfully. Comment ID: {CommentId}", comment.Id);

                // Return the response
                return Json(new
                {
                    username = user.FullName,
                    text = comment.Text,
                    commentId = comment.Id
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while adding comment.");
                return StatusCode(500, "Internal Server Error.");
            }
        }

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
            comment.UpdatedAt = DateTime.Now; // Optional: Track when the comment was updated

            try
            {
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment.");
                return StatusCode(500, "Internal Server Error.");
            }
        }
    }
}