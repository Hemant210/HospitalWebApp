using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public enum LabTestStatus { Pending, SampleCollected, InProgress, Completed, Cancelled }

public class LabTest
    {
        [Key]
        public int TestId { get; set; }

        [Required]
        public int RecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public int? TechnicianId { get; set; } // Nullable because it might not be assigned immediately

        [Required, StringLength(100)]
        [Display(Name = "Test Name")]
        public string TestName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Test Cost")]
        public decimal Cost { get; set; } = 0; // NEW: Crucial for billing

        public string? Result { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Test Date")]
        public DateTime TestDate { get; set; } = DateTime.Now;

        [Required]
        public LabTestStatus Status { get; set; } = LabTestStatus.Pending; // Updated to use Enum

        // Navigation
        [ForeignKey(nameof(RecordId))]
        public MedicalRecord? MedicalRecord { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public Staff? Technician { get; set; }
    }
}