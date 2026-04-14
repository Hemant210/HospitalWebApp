using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; }

        [Display(Name = "Stock Qty")]
        public int StockQty { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}