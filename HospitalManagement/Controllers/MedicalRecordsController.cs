using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.MedicalTeam)] // Admin, Doctor, Nurse, LabTechnician can view lists/details
    public class MedicalRecordsController : Controller
    {
        private readonly HospitalDbContext _context;

        public MedicalRecordsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: MedicalRecords (With Multi-Field Search)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var recordsQuery = _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                recordsQuery = recordsQuery.Where(m => m.Patient.FirstName.Contains(searchString)
                                                    || m.Patient.LastName.Contains(searchString)
                                                    || m.Diagnosis.Contains(searchString));
            }

            return View(await recordsQuery.AsNoTracking().ToListAsync());
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

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
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");
            ViewData["PatientId"] = patientId.HasValue
                ? new SelectList(_context.Patients, "PatientId", "FirstName", patientId)
                : new SelectList(_context.Patients, "PatientId", "FirstName");

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
                return RedirectToAction(nameof(Details), new { id = medicalRecord.RecordId });
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Edit/5
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)] // Admin and Doctor have edit authority
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null) return NotFound();

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)] // Admin and Doctor have edit authority
        public async Task<IActionResult> Edit(int id, MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.RecordId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalRecordExists(medicalRecord.RecordId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id = medicalRecord.RecordId });
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin has delete authority
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.RecordId == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin has delete authority
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
            {
                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecords.Any(e => e.RecordId == id);
        }
    }
}