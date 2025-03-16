using EliteTGTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EliteTGTask.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Dashboard i Adminit
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string UserId, string role)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var v in roles)
            {
                await _userManager.RemoveFromRoleAsync(user, v);
            }
            await _userManager.AddToRoleAsync(user, role);
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");

        }
    }
}
