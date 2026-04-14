using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}