using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.MedicalTeam)]
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

            return View(await recordsQuery.OrderByDescending(r => r.VisitDate).AsNoTracking().ToListAsync());
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
        [Authorize(Roles = HospitalRoles.ClinicalStaff)]
        public IActionResult Create(int? patientId)
        {
            PopulateSmartDropdowns(patientId);
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
            PopulateSmartDropdowns(medicalRecord.PatientId, medicalRecord.DoctorId);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Edit/5
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null) return NotFound();

            // When editing, we include all patients (even discharged) so the dropdown doesn't break on historic records
            PopulateSmartDropdowns(medicalRecord.PatientId, medicalRecord.DoctorId, includeAllPatients: true);
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
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
            PopulateSmartDropdowns(medicalRecord.PatientId, medicalRecord.DoctorId, includeAllPatients: true);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)]
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
        [Authorize(Roles = HospitalRoles.Admin)]
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

        // --- HELPER METHOD FOR SMART DROPDOWNS ---
        private void PopulateSmartDropdowns(int? selectedPatientId = null, int? selectedDoctorId = null, bool includeAllPatients = false)
        {
            var doctors = _context.Doctors.Select(d => new {
                DoctorId = d.DoctorId,
                FullName = $"Dr. {d.FirstName} {d.LastName}"
            }).ToList();

            ViewData["DoctorId"] = new SelectList(doctors, "DoctorId", "FullName", selectedDoctorId);

            var today = DateTime.Today;
            var patientsQuery = _context.Patients.AsQueryable();

            // The Core Fix: Filter out discharged patients unless explicitly told to include them
            if (!includeAllPatients)
            {
                patientsQuery = patientsQuery.Where(p =>
                    p.Admissions.Any(a => a.DischargeDate == null) || // Currently admitted in a bed
                    p.Appointments.Any(a => a.AppointmentDate.Date >= today) // Or has an appointment today/future
                );
            }

            var activePatients = patientsQuery.Select(p => new {
                PatientId = p.PatientId,
                FullName = $"{p.FirstName} {p.LastName} (ID: #{p.PatientId})"
            }).ToList();

            ViewData["PatientId"] = new SelectList(activePatients, "PatientId", "FullName", selectedPatientId);
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecords.Any(e => e.RecordId == id);
        }
        // ---------------------------------------------------
        // 📱 iOS API ENDPOINTS (FULL CRUD)
        // ---------------------------------------------------

        public class ApiMedicalRecordDto
        {
            public int RecordId { get; set; }
            public int PatientId { get; set; }
            public string? PatientName { get; set; }
            public int DoctorId { get; set; }
            public string? DoctorName { get; set; }
            public DateTime VisitDate { get; set; }
            public string? Diagnosis { get; set; }
            public string? Vitals { get; set; }
            public string? Notes { get; set; }
        }

        [HttpGet("/api/medicalrecords")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedicalRecordsApi()
        {
            var records = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Select(m => new ApiMedicalRecordDto
                {
                    RecordId = m.RecordId,
                    PatientId = m.PatientId,
                    PatientName = m.Patient.FirstName + " " + m.Patient.LastName,
                    DoctorId = m.DoctorId,
                    DoctorName = "Dr. " + m.Doctor.FirstName + " " + m.Doctor.LastName,
                    VisitDate = m.VisitDate,
                    Diagnosis = m.Diagnosis,
                    Vitals = m.Vitals,
                    Notes = m.Notes
                })
                .OrderByDescending(m => m.VisitDate)
                .AsNoTracking().ToListAsync();
            return Ok(records);
        }

        // 🧠 SMART DROPDOWN API FOR iOS
        [HttpGet("/api/medicalrecords/options")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOptionsApi()
        {
            var doctors = await _context.Doctors.Select(d => new { id = d.DoctorId, name = $"Dr. {d.FirstName} {d.LastName}" }).ToListAsync();

            var today = DateTime.Today;
            var activePatients = await _context.Patients
                .Where(p => p.Admissions.Any(a => a.DischargeDate == null) || p.Appointments.Any(a => a.AppointmentDate.Date >= today))
                .Select(p => new { id = p.PatientId, name = $"{p.FirstName} {p.LastName} (ID: #{p.PatientId})" })
                .ToListAsync();

            return Ok(new { doctors, patients = activePatients });
        }

        [HttpPost("/api/medicalrecords")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRecordApi([FromBody] ApiMedicalRecordDto dto)
        {
            var record = new MedicalRecord
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitDate = dto.VisitDate,
                Diagnosis = dto.Diagnosis,
                Vitals = dto.Vitals,
                Notes = dto.Notes
            };
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("/api/medicalrecords/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateRecordApi(int id, [FromBody] ApiMedicalRecordDto dto)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            record.PatientId = dto.PatientId;
            record.DoctorId = dto.DoctorId;
            record.VisitDate = dto.VisitDate;
            record.Diagnosis = dto.Diagnosis;
            record.Vitals = dto.Vitals;
            record.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("/api/medicalrecords/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteRecordApi(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}