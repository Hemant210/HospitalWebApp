//using System.ComponentModel.DataAnnotations;

//namespace HospitalManagement.Models
//{
//    public class Prescription
//    {
//        public int PrescriptionId { get; set; }
//        public int RecordId { get; set; }
//        public int DoctorId { get; set; }

//        [DataType(DataType.Date)]
//        public DateTime PrescribedDate { get; set; } = DateTime.Today;

//        [StringLength(255)]
//        public string? Notes { get; set; }

//        public MedicalRecord? MedicalRecord { get; set; }
//        public Doctor? Doctor { get; set; }
//        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
//    }
//}



using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 1. ADD THIS FOR FOREIGN KEYS

namespace HospitalManagement.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int RecordId { get; set; }
        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        public DateTime PrescribedDate { get; set; } = DateTime.Today;

        [StringLength(255)]
        public string? Notes { get; set; }

        // 2. ADD THIS FIX: Tell Entity Framework exactly which ID to use!
        [ForeignKey("RecordId")]
        public MedicalRecord? MedicalRecord { get; set; }

        public Doctor? Doctor { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}