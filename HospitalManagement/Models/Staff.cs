using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Staff
    {
        public int StaffId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }
        public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
    }
}