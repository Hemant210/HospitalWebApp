using HospitalManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController] // This tells ASP.NET to expect and return JSON
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // A lightweight model specifically for the iOS API
        public class ApiLoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiLoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && user.IsActive)
            {
                // Verify the password without setting a browser cookie
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (result.Succeeded)
                {
                    // 🚨 ADD THIS LINE: This gives the iPhone a session cookie so it stays logged in!
                    await _signInManager.SignInAsync(user, isPersistent: true);

                    return Ok(new
                    {
                        success = true,
                        userId = user.Id,
                        fullName = user.FullName,
                        role = user.Role,
                        message = "Login Successful"
                    });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid email or password." });
        }
    }
}