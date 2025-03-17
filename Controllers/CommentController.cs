using EliteTGTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EliteTGTask.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Shto Koment live me AJAX
        [HttpPost]
       [HttpPost]
public async Task<IActionResult> AddComment(int postId, string text)
{
    if (!User.Identity.IsAuthenticated || !User.IsInRole("Member"))
    {
        return Unauthorized();
    }

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return Unauthorized();
    }

    var comment = new Comment
    {
        postId = postId,
        Text = text,
        UserId = user.Id
    };

    _dbContext.Comments.Add(comment);
    await _dbContext.SaveChangesAsync();

    return Json(new
    {
        username = user.FullName,
        text = comment.Text
    });
}

    }
}
