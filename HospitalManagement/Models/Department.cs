using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Department Name")]
        public string DeptName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        public int? HeadDoctorId { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}