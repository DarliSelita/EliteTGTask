using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EliteTGTask.Models;
using System.Linq.Expressions;
//using EliteTGTask.ViewModels;

namespace EliteTGTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        //Ketu eshte Sign Up-i, vetem bejme retrieve View
        [HttpGet]

        public IActionResult Signup()
        {
            return View();
        }

        // Ketu bejme POST ose dergojme te dhenat per krijimin e nje acc te ri (breakpoint tek line i krijimit te user per te pare nese vjen gje apo jo)
        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel Model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Model.Username, FullName = Model.FullName };
                var result = await _userManager.CreateAsync(user, Model.Password());
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }
            return View(Model);

        }

        // 


        public IActionResult Index()
        {
            return View();
        }
    }
}
