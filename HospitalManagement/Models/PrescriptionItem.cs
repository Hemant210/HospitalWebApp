using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int ItemId { get; set; }
        public int PrescriptionId { get; set; }
        public int MedicineId { get; set; }

        [StringLength(50)]
        public string? Dosage { get; set; }

        [Display(Name = "Duration (Days)")]
        public int DurationDays { get; set; }

        public Prescription? Prescription { get; set; }
        public Medicine? Medicine { get; set; }
    }
}