using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class MedicalRecord
    {
        [Key] // ✅ FIX
        public int RecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Diagnosis { get; set; }

        public string? Notes { get; set; }
        public string? Vitals { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
    }
}