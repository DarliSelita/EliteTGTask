using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;

namespace EliteTGTask.Controllers
{
    public class PostController : Controller
    {

        private readonly ApplicationDBContext _Context;

        public PostController (ApplicationDBContext _Context)
        {
            _Context = _Context;
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

    }
}
