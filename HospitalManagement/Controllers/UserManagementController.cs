using HospitalManagement.Auth;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin can manage logins!
    public class UserManagementController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UserManagementController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // View all users in the system
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Deactivate a user account (e.g., if a staff member quits)
        [HttpPost]
        public async Task<IActionResult> ToggleActiveStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive; // Flip the status
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}