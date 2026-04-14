using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    // Only medical staff should see medical records! Receptionists are excluded.
    [Authorize(Roles = HospitalRoles.MedicalTeam)]
    public class MedicalRecordsController : Controller
    {
        private readonly HospitalDbContext _context;

        public MedicalRecordsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index()
        {
            var records = _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient);
            return View(await records.ToListAsync());
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // This massive query grabs the Record, the Doctor, the Patient, 
            // AND all attached Prescriptions, Medicines, and Lab Tests!
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                        .ThenInclude(pi => pi.Medicine)
                .Include(m => m.LabTests)
                .FirstOrDefaultAsync(m => m.RecordId == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        [Authorize(Roles = HospitalRoles.ClinicalStaff)] // Admin, Doctor, Nurse can create
        public IActionResult Create(int? patientId)
        {
            // Dropdowns for creating the record
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");

            // Smart feature: If we pass a patientId in the URL, pre-select them in the dropdown
            if (patientId.HasValue)
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", patientId);
            }
            else
            {
                ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName");
            }

            return View();
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.ClinicalStaff)]
        public async Task<IActionResult> Create(MedicalRecord medicalRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicalRecord);
                await _context.SaveChangesAsync();
                // Redirect straight to the details page so the doctor can add prescriptions!
                return RedirectToAction(nameof(Details), new { id = medicalRecord.RecordId });
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", medicalRecord.PatientId);
            return View(medicalRecord);
        }
    }
}