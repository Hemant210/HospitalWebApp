using Microsoft.AspNetCore.Identity;

namespace HospitalManagement.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? LinkedId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfilePhoto { get; set; }
    }
}