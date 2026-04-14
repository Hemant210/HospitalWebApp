using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public enum BedStatus { Available, Occupied, Maintenance }
    public class Bed
    {
        public int BedId { get; set; }
        public int WardId { get; set; }

        [StringLength(10)]
        [Display(Name = "Bed Number")]
        public string? BedNumber { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Price Per Day")]
        public decimal PricePerDay { get; set; } = 0; // NEW: Room rates for billing

        public BedStatus Status { get; set; } = BedStatus.Available;

        public Ward? Ward { get; set; }
        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    }
}