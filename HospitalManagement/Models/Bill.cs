using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public enum PaymentStatus { Unpaid, Partial, Paid, Refunded }
    public enum PaymentMode { Cash, CreditCard, UPI, Insurance }
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        public int PatientId { get; set; }

        // Either AppointmentId OR AdmissionId will have a value
        public int? AppointmentId { get; set; }
        public int? AdmissionId { get; set; }

        // --- BILLING BREAKDOWN ---
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Consultation Fee")]
        public decimal DoctorFee { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Room/Bed Charges")]
        public decimal RoomCharges { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Lab Test Charges")]
        public decimal LabCharges { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Medicine Charges")]
        public decimal MedicineCharges { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Discount Amount")]
        public decimal Discount { get; set; } = 0;

        // --- TOTALS ---
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Net Total")]
        public decimal TotalAmount { get; set; } // (DoctorFee + Room + Lab + Medicine) - Discount

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Paid Amount")]
        public decimal PaidAmount { get; set; } = 0;

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid; // Uses Enum

        [Display(Name = "Payment Mode")]
        public PaymentMode? PaymentMode { get; set; } // Uses Enum

        [DataType(DataType.DateTime)]
        [Display(Name = "Bill Date")]
        public DateTime BillDate { get; set; } = DateTime.Now;

        // Navigation
        public Patient? Patient { get; set; }
        public Appointment? Appointment { get; set; }
        public Admission? Admission { get; set; }
    }
}