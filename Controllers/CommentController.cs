using EliteTGTask.Models;
using Microsoft.AspNetCore.Mvc;

namespace EliteTGTask.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDBContext _dbContext;

        public CommentController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Shto Koment live me AJAX
        [HttpPost]
        public async Task<IActionResult> AddComment(int PostId, String CommentText)
        {
            if (string.IsNullOrEmpty(CommentText)) return BadRequest("Please write something!!!");

            var post = await _dbContext.Posts.FindAsync(PostId);
            if (post == null) return NotFound();

            var comment = new Comment
            {
                Text = CommentText,
                postId = PostId,
                CreatedAt = DateTime.Now,
                UserId = User.Identity.Name
            };
            _dbContext.Add(comment);
            await _dbContext.SaveChangesAsync();

            return Json(new { comment.User.FullName, comment.Text, comment.CreatedAt });
        }
    }
}
