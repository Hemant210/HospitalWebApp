using HospitalManagement.Auth;
using HospitalManagement.Models;
using HospitalManagement.ViewModels; // Ensuing access to RegisterViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin can manage logins!
    public class UserManagementController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Inject RoleManager alongside UserManager
        public UserManagementController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagement
        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            // 🚨 UI CLEANUP: Fetch all users EXCEPT those with the Patient role
            var staffUsers = await _userManager.Users
                .Where(u => u.Role != HospitalRoles.Patient)
                .ToListAsync();

            return View(staffUsers);
        }

        // GET: UserManagement/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Gather all available roles from the system database to fill the dropdown menu
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles);

            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,      // Saves the string role name directly to AppUser profile
                    IsActive = true,
                    EmailConfirmed = true   // Auto-confirm since Admin is manually creating it
                };

                // Create the user with the chosen password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the structural Identity system Role to this user account
                    await _userManager.AddToRoleAsync(user, model.Role);

                    return RedirectToAction(nameof(Index));
                }

                // If creation fails (e.g., password too weak), add errors to display to Admin
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we reach here, something failed; reload the roles dropdown list
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles, model.Role);
            return View(model);
        }

        // POST: UserManagement/ToggleActiveStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}