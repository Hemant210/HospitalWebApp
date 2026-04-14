using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(255)]
        public string? Reason { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Bill? Bill { get; set; }
    }
}