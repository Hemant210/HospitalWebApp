using HospitalManagement.Auth;
using HospitalManagement.Models;
using HospitalManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Pass the returnUrl to the view so the form knows where to send them
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;

        //    if (!ModelState.IsValid) return View(model);

        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user != null && user.IsActive)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        //        if (result.Succeeded)
        //        {
        //            // Smart Routing: Send them to their requested page, or default to Dashboard
        //            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        //            {
        //                return LocalRedirect(returnUrl);
        //            }
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }

        //    // Security best practice: Keep failure messages generic
        //    ModelState.AddModelError(string.Empty, "Invalid authentication attempt or account disabled.");
        //    return View(model);
        //}

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            // 1. Find the user by Email (or Phone, if they typed their phone in the top box)
            var user = await _userManager.FindByEmailAsync(model.EmailOrPhone) ??
                       await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone);

            if (user != null && user.IsActive)
            {
                // Check if the user is a patient
                bool isPatient = await _userManager.IsInRoleAsync(user, HospitalRoles.Patient);
                bool signInSuccess = false;

                if (isPatient)
                {
                    // 🚨 NEW LOGIC: For Patients, check if the entered password exactly matches their Phone Number
                    if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber == model.Password)
                    {
                        // Match found! Manually sign them in without checking hashed passwords
                        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                        signInSuccess = true;
                    }
                    else
                    {
                        // Fallback: Check the standard hashed password just in case they changed it manually later
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                        signInSuccess = result.Succeeded;
                    }
                }
                else
                {
                    // 🔒 STRICT SECURITY: For Admins, Doctors, and Staff, ONLY use secure hashed passwords
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    signInSuccess = result.Succeeded;
                }

                // 2. If Sign-In was successful, route them correctly
                if (signInSuccess)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    // 3. ROLE ROUTING: Send Patients to their Portal
                    if (isPatient)
                    {
                        return RedirectToAction("Dashboard", "PatientPortal");
                    }

                    // Default to Main Dashboard for Staff
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}