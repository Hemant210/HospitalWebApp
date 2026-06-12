using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.MedicalTeam)] // Admin, Doctor, Nurse, Lab Tech
    public class LabTestsController : Controller
    {
        private readonly HospitalDbContext _context;

        public LabTestsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: LabTests (With Search Filter & Smart Sorting)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var labTestsQuery = _context.LabTests
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .Include(l => l.Technician)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                labTestsQuery = labTestsQuery.Where(l => l.TestName.Contains(searchString)
                                                      || l.Patient.FirstName.Contains(searchString)
                                                      || l.Patient.LastName.Contains(searchString));
            }

            // SMART FIX: Show ALL records, but order them by Status first, then Date.
            // Because Enums are backed by integers (Pending = 0, Completed = 3, Cancelled = 4),
            // OrderBy(l => l.Status) naturally pushes active tasks to the top and finished tasks to the bottom!
            var sortedLabs = await labTestsQuery
                .OrderBy(l => l.Status)
                .ThenByDescending(l => l.TestDate)
                .AsNoTracking()
                .ToListAsync();

            return View(sortedLabs);
        }

        // GET: LabTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var labTest = await _context.LabTests
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .Include(l => l.Technician)
                .FirstOrDefaultAsync(m => m.TestId == id);

            if (labTest == null) return NotFound();

            return View(labTest);
        }

        // GET: LabTests/Create (SMART FLOW ADDED)
        public async Task<IActionResult> Create(int? recordId)
        {
            PopulateSmartLabDropdowns(recordId);
            return View();
        }

        // POST: LabTests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,RecordId,PatientId,TechnicianId,TestName,Cost,Result,TestDate,Status")] LabTest labTest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labTest);
                await _context.SaveChangesAsync();

                // SMART FLOW FIX: Return Doctor directly back to the Patient's Medical Record!
                return RedirectToAction("Details", "MedicalRecords", new { id = labTest.RecordId });
            }

            PopulateSmartLabDropdowns(labTest.RecordId, labTest.PatientId, labTest.TechnicianId);
            return View(labTest);
        }

        // GET: LabTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var labTest = await _context.LabTests.FindAsync(id);
            if (labTest == null) return NotFound();

            PopulateSmartLabDropdowns(labTest.RecordId, labTest.PatientId, labTest.TechnicianId, includeAll: true);
            return View(labTest);
        }

        // POST: LabTests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TestId,RecordId,PatientId,TechnicianId,TestName,Cost,Result,TestDate,Status")] LabTest labTest)
        {
            if (id != labTest.TestId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabTestExists(labTest.TestId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSmartLabDropdowns(labTest.RecordId, labTest.PatientId, labTest.TechnicianId, includeAll: true);
            return View(labTest);
        }

        // GET: LabTests/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var labTest = await _context.LabTests
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .Include(l => l.Technician)
                .FirstOrDefaultAsync(m => m.TestId == id);

            if (labTest == null) return NotFound();

            return View(labTest);
        }

        // POST: LabTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labTest = await _context.LabTests.FindAsync(id);
            if (labTest != null)
            {
                _context.LabTests.Remove(labTest);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- SMART FILTER LOGIC FOR LABS ---
        private void PopulateSmartLabDropdowns(int? selectedRecordId = null, int? selectedPatientId = null, int? selectedTechId = null, bool includeAll = false)
        {
            var today = DateTime.Today;

            var recordsQuery = _context.MedicalRecords.Include(m => m.Patient).AsQueryable();
            var patientsQuery = _context.Patients.AsQueryable();

            if (!includeAll)
            {
                recordsQuery = recordsQuery.Where(m => m.Patient.Admissions.Any(a => a.DischargeDate == null) || m.VisitDate.Date >= today);
                patientsQuery = patientsQuery.Where(p => p.Admissions.Any(a => a.DischargeDate == null) || p.Appointments.Any(a => a.AppointmentDate.Date >= today));
            }

            var recordList = recordsQuery.Select(r => new {
                RecordId = r.RecordId,
                Display = $"Record #{r.RecordId} - {r.Patient.FirstName} {r.Patient.LastName}"
            }).ToList();

            var patientList = patientsQuery.Select(p => new {
                PatientId = p.PatientId,
                FullName = $"{p.FirstName} {p.LastName}"
            }).ToList();

            ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", selectedRecordId);
            ViewData["PatientId"] = new SelectList(patientList, "PatientId", "FullName", selectedPatientId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", selectedTechId);
        }

        private bool LabTestExists(int id) => _context.LabTests.Any(e => e.TestId == id);
    }
}