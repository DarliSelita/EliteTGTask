using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EliteTGTask.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Server.HttpSys;
using Amazon.S3;
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

        public ViewResult Signup()
        {
            return View();
        }

        // Ketu behet POST ose dergojm te dhenat per krijimin e nje acc te ri (breakpoint tek line i krijimit te user per te pare nese vjen gje apo jo)
        [HttpPost]
        [ActionName("Signup")]
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

        [HttpGet]

        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel Model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Model.Username, Model.Password, _userManager);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid Loign Attempt");
            return View(Model);
        }

        // Log Out

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
