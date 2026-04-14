using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(5)]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [StringLength(50)]
        [Display(Name = "Insurance No")]
        public string? InsuranceNo { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    }
}