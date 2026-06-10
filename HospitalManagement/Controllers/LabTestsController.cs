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

        // GET: LabTests (With Search Filter)
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

            return View(await labTestsQuery.AsNoTracking().ToListAsync());
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
            // Format the Record dropdown to show Patient Names instead of just numbers
            var records = _context.MedicalRecords.Include(m => m.Patient).ToList();
            var recordList = records.Select(r => new {
                RecordId = r.RecordId,
                Display = $"Record #{r.RecordId} - {r.Patient?.FirstName} {r.Patient?.LastName}"
            });

            // If we came from the Medical Record page, auto-select the dropdowns!
            if (recordId.HasValue)
            {
                var medicalRecord = await _context.MedicalRecords.FindAsync(recordId);
                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", recordId);
                ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", medicalRecord?.PatientId);
            }
            else
            {
                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display");
                ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName");
            }

            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName");
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

            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
            return View(labTest);
        }

        // GET: LabTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var labTest = await _context.LabTests.FindAsync(id);
            if (labTest == null) return NotFound();

            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
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
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
            return View(labTest);
        }

        // GET: LabTests/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // Only Admin should delete lab tests
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

        private bool LabTestExists(int id)
        {
            return _context.LabTests.Any(e => e.TestId == id);
        }
    }
}