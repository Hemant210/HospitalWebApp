using HospitalManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Data
{
    public class HospitalDbContext : IdentityDbContext<AppUser>
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Rename Identity tables
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor).WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);

            // MedicalRecord
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Doctor).WithMany(d => d.MedicalRecords)
                .HasForeignKey(m => m.DoctorId).OnDelete(DeleteBehavior.Restrict);


            // LAB TEST RELATION
            modelBuilder.Entity<LabTest>()
                .HasKey(l => l.TestId);

            modelBuilder.Entity<LabTest>()
                .HasOne(l => l.MedicalRecord)
                .WithMany(m => m.LabTests)
                .HasForeignKey(l => l.RecordId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LabTest>()
                .HasOne(l => l.Patient)
                .WithMany()
                .HasForeignKey(l => l.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LabTest>()
                .HasOne(l => l.Technician)
                .WithMany(s => s.LabTests)
                .HasForeignKey(l => l.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Admission
            modelBuilder.Entity<Admission>()
                .HasOne(a => a.Patient).WithMany(p => p.Admissions)
                .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Admission>()
                .HasOne(a => a.Doctor).WithMany()
                .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);

            // Bill — one-to-one with Appointment
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Appointment).WithOne(a => a.Bill)
                .HasForeignKey<Bill>(b => b.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Patient).WithMany(p => p.Bills)
                .HasForeignKey(b => b.PatientId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}