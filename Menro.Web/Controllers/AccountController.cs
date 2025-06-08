using Menro.Application.SD;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using Menro.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Menro.Web.ViewModels;

namespace Menro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(IUnitOfWork uow, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            LoginVM loginVM = new()
            {
                RedirectUrl = returnUrl
            };

            return View(loginVM);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(vm.Email);
                    if (user != null)
                    {
                        TempData["success"] = "ورود با موفقیت انجام شد";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["error"] = "ایمیل در دیتابیس پیدا نشد";
                        return View(vm);
                    }
                }
                else
                {
                    TempData["error"] = "ورود با موفقیت انجام نشد";
                    //ModelState.AddModelError("", "invalid login attempt");
                }
            }
            TempData["error"] = "مدل نامعتبر";
            return View(vm);
        }
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            RegisterVM registerVM = new()
            {
                RoleList = _roleManager.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                }),
                RedirectUrl = returnUrl
            };
            return View(registerVM);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            User user = new()
            {
                FullName = vm.FullName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                NormalizedEmail = vm.Email.ToUpper(),
                EmailConfirmed = true,
                UserName = vm.Email
            };

            var result = await _userManager.CreateAsync(user, vm.ConfirmPassword);

            if (result.Succeeded)
            {

                if (!string.IsNullOrEmpty(vm.Role))
                {
                    await _userManager.AddToRoleAsync(user, vm.Role);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                }
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["success"] = "registration was successful!";
                if (string.IsNullOrEmpty(vm.RedirectUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return LocalRedirect(vm.RedirectUrl);
                }

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            vm.RoleList = _roleManager.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Name
            });
            return View(vm);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
