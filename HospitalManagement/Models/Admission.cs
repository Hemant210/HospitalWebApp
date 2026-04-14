using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Admission
    {
        public int AdmissionId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int BedId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Admit Date")]
        public DateTime AdmitDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Discharge Date")]
        public DateTime? DischargeDate { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Bed? Bed { get; set; }
    }
}