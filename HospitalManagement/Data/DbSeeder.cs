using HospitalManagement.Auth;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Create roles
            string[] roles = {
                HospitalRoles.Admin, HospitalRoles.Doctor, HospitalRoles.Nurse,
                HospitalRoles.Receptionist, HospitalRoles.Pharmacist,
                HospitalRoles.LabTechnician, HospitalRoles.Patient
            };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Seed users
            await Seed(userManager, "admin@hospital.com", "Admin@123", "System Administrator", HospitalRoles.Admin);
            await Seed(userManager, "doctor@hospital.com", "Doctor@123", "Dr. Priya Sharma", HospitalRoles.Doctor);
            await Seed(userManager, "nurse@hospital.com", "Nurse@123", "Nurse Rekha Patel", HospitalRoles.Nurse);
            await Seed(userManager, "reception@hospital.com", "Reception@123", "Ravi Kumar", HospitalRoles.Receptionist);
            await Seed(userManager, "pharma@hospital.com", "Pharma@123", "Sita Devi", HospitalRoles.Pharmacist);
            await Seed(userManager, "lab@hospital.com", "Lab@123", "Mohan Das", HospitalRoles.LabTechnician);
            await Seed(userManager, "patient@hospital.com", "Patient@123", "Raj Kumar", HospitalRoles.Patient);
        }

        private static async Task Seed(UserManager<AppUser> um,
            string email, string password, string fullName, string role)
        {
            if (await um.FindByEmailAsync(email) == null)
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    IsActive = true,
                    EmailConfirmed = true
                };
                var r = await um.CreateAsync(user, password);
                if (r.Succeeded) await um.AddToRoleAsync(user, role);
            }
        }
    }
}