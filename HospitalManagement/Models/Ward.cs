using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Ward
    {
        public int WardId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Ward Name")]
        public string WardName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Ward Type")]
        public string? WardType { get; set; }

        [Display(Name = "Total Beds")]
        public int TotalBeds { get; set; }

        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
    }
}