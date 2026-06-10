//using HospitalManagement.Auth;
//using HospitalManagement.Data;
//using HospitalManagement.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace HospitalManagement.Controllers
//{
//    [Authorize(Roles = HospitalRoles.MedicalTeam)]
//    public class PrescriptionsController : Controller
//    {
//        private readonly HospitalDbContext _context;

//        public PrescriptionsController(HospitalDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Prescriptions
//        public async Task<IActionResult> Index()
//        {
//            var hospitalDbContext = _context.Prescriptions.Include(p => p.Doctor);
//            return View(await hospitalDbContext.ToListAsync());
//        }

//        // GET: Prescriptions/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var prescription = await _context.Prescriptions
//                .Include(p => p.Doctor)
//                .FirstOrDefaultAsync(m => m.PrescriptionId == id);
//            if (prescription == null)
//            {
//                return NotFound();
//            }

//            return View(prescription);
//        }

//        // GET: Prescriptions/Create
//        //public IActionResult Create()
//        //{
//        //    ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");
//        //    return View();
//        //}


//        // GET: Prescriptions/Create
//        public IActionResult Create(int? recordId)
//        {
//            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");

//            // UPGRADE: Fetch the Patient's name so the dropdown shows "Record #1 - John Doe" instead of just "1"
//            var records = _context.MedicalRecords.Include(m => m.Patient).ToList();
//            var recordList = records.Select(r => new {
//                RecordId = r.RecordId,
//                Display = $"Record #{r.RecordId} - {r.Patient?.FirstName} {r.Patient?.LastName}"
//            });

//            if (recordId.HasValue)
//            {
//                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", recordId);
//            }
//            else
//            {
//                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display");
//            }

//            return View();
//        }

//        // POST: Prescriptions/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("PrescriptionId,RecordId,DoctorId,PrescribedDate,Notes")] Prescription prescription)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(prescription);
//                await _context.SaveChangesAsync();

//                // BUG FIX: Send the doctor straight back to the Patient's Medical Record!
//                return RedirectToAction("Details", "MedicalRecords", new { id = prescription.RecordId });
//            }

//            // If the form fails, we must reload the smart dropdowns
//            var records = _context.MedicalRecords.Include(m => m.Patient).ToList();
//            var recordList = records.Select(r => new {
//                RecordId = r.RecordId,
//                Display = $"Record #{r.RecordId} - {r.Patient?.FirstName} {r.Patient?.LastName}"
//            });

//            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
//            ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", prescription.RecordId);
//            return View(prescription);
//        }

//        // GET: Prescriptions/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var prescription = await _context.Prescriptions.FindAsync(id);
//            if (prescription == null)
//            {
//                return NotFound();
//            }
//            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
//            return View(prescription);
//        }

//        // POST: Prescriptions/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("PrescriptionId,RecordId,DoctorId,PrescribedDate,Notes")] Prescription prescription)
//        {
//            if (id != prescription.PrescriptionId)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(prescription);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!PrescriptionExists(prescription.PrescriptionId))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
//            return View(prescription);
//        }

//        // GET: Prescriptions/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var prescription = await _context.Prescriptions
//                .Include(p => p.Doctor)
//                .FirstOrDefaultAsync(m => m.PrescriptionId == id);
//            if (prescription == null)
//            {
//                return NotFound();
//            }

//            return View(prescription);
//        }

//        // POST: Prescriptions/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var prescription = await _context.Prescriptions.FindAsync(id);
//            if (prescription != null)
//            {
//                _context.Prescriptions.Remove(prescription);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool PrescriptionExists(int id)
//        {
//            return _context.Prescriptions.Any(e => e.PrescriptionId == id);
//        }
//    }
//}



using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.MedicalTeam + "," + HospitalRoles.Pharmacist)]
    public class PrescriptionsController : Controller
    {
        private readonly HospitalDbContext _context;

        public PrescriptionsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Prescriptions (With Deep Search)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // REAL-WORLD FIX: Include the Medical Record and Patient so pharmacists know whose RX this is!
            var rxQuery = _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                rxQuery = rxQuery.Where(p =>
                    p.Doctor.LastName.Contains(searchString) ||
                    p.MedicalRecord.Patient.FirstName.Contains(searchString) ||
                    p.MedicalRecord.Patient.LastName.Contains(searchString) ||
                    p.Notes.Contains(searchString));
            }

            return View(await rxQuery.OrderByDescending(p => p.PrescribedDate).AsNoTracking().ToListAsync());
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Patient)
                .Include(p => p.PrescriptionItems) // Pull the actual drugs for the view
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // GET: Prescriptions/Create
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
        public IActionResult Create(int? recordId)
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");

            var records = _context.MedicalRecords.Include(m => m.Patient).ToList();
            var recordList = records.Select(r => new {
                RecordId = r.RecordId,
                Display = $"Record #{r.RecordId} - {r.Patient?.FirstName} {r.Patient?.LastName}"
            });

            if (recordId.HasValue)
                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", recordId);
            else
                ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display");

            return View();
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
        public async Task<IActionResult> Create([Bind("PrescriptionId,RecordId,DoctorId,PrescribedDate,Notes")] Prescription prescription)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prescription);
                await _context.SaveChangesAsync();
                // Redirect back to Patient Record to add specific drugs
                return RedirectToAction("Details", "MedicalRecords", new { id = prescription.RecordId });
            }

            var records = _context.MedicalRecords.Include(m => m.Patient).ToList();
            var recordList = records.Select(r => new {
                RecordId = r.RecordId,
                Display = $"Record #{r.RecordId} - {r.Patient?.FirstName} {r.Patient?.LastName}"
            });

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
            ViewData["RecordId"] = new SelectList(recordList, "RecordId", "Display", prescription.RecordId);
            return View(prescription);
        }

        // GET: Prescriptions/Edit/5
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return NotFound();

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
            return View(prescription);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrDoctor)]
        public async Task<IActionResult> Edit(int id, [Bind("PrescriptionId,RecordId,DoctorId,PrescribedDate,Notes")] Prescription prescription)
        {
            if (id != prescription.PrescriptionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(prescription.PrescriptionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Details", "MedicalRecords", new { id = prescription.RecordId });
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", prescription.DoctorId);
            return View(prescription);
        }

        // GET: Prescriptions/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin can delete an RX
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Patient)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.PrescriptionId == id);
        }
    }
}