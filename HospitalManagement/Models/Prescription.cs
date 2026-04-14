using System.ComponentModel.DataAnnotations;

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

        public MedicalRecord? MedicalRecord { get; set; }
        public Doctor? Doctor { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}