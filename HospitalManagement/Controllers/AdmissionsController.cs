using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.AdminOrReceptionist)] // Guard rails for Front-Desk operations
    public class AdmissionsController : Controller
    {
        private readonly HospitalDbContext _context;

        public AdmissionsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Admissions (With Search Engine Filtering)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var admissionsQuery = _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                admissionsQuery = admissionsQuery.Where(a =>
                    a.Patient.FirstName.Contains(searchString) ||
                    a.Patient.LastName.Contains(searchString) ||
                    a.Doctor.FirstName.Contains(searchString) ||
                    a.Doctor.LastName.Contains(searchString) ||
                    a.Reason.Contains(searchString));
            }

            return View(await admissionsQuery.OrderByDescending(a => a.AdmitDate).AsNoTracking().ToListAsync());
        }

        // GET: Admissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .FirstOrDefaultAsync(m => m.AdmissionId == id);

            if (admission == null) return NotFound();

            return View(admission);
        }

        // GET: Admissions/Create
        public IActionResult Create()
        {
            PopulateDropdownsSelectList();
            return View();
        }

        // POST: Admissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Admission admission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admission);

                // AUTOMATION LOGIC: Auto-occupy the assigned bed hardware asset
                var bed = await _context.Beds.FindAsync(admission.BedId);
                if (bed != null)
                {
                    bed.Status = BedStatus.Occupied;
                    _context.Update(bed);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdownsSelectList(admission);
            return View(admission);
        }

        // GET: Admissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var admission = await _context.Admissions.FindAsync(id);
            if (admission == null) return NotFound();

            PopulateDropdownsSelectList(admission, includeAllBeds: true);
            return View(admission);
        }

        // POST: Admissions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Admission admission)
        {
            if (id != admission.AdmissionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(admission);

                    // AUTOMATION LOGIC: Free bed up immediately if discharge timestamp is stamped
                    if (admission.DischargeDate.HasValue)
                    {
                        var bed = await _context.Beds.FindAsync(admission.BedId);
                        if (bed != null)
                        {
                            bed.Status = BedStatus.Available;
                            _context.Update(bed);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdmissionExists(admission.AdmissionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdownsSelectList(admission, includeAllBeds: true);
            return View(admission);
        }

        // GET: Admissions/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // Legal records safeguard: Admin only!
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .FirstOrDefaultAsync(m => m.AdmissionId == id);

            if (admission == null) return NotFound();

            return View(admission);
        }

        // POST: Admissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admission = await _context.Admissions.FindAsync(id);
            if (admission != null)
            {
                // Safety logic: If deleting an active admission record, reset the bed to available
                var bed = await _context.Beds.FindAsync(admission.BedId);
                if (bed != null && !admission.DischargeDate.HasValue)
                {
                    bed.Status = BedStatus.Available;
                    _context.Update(bed);
                }

                _context.Admissions.Remove(admission);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Clean, structured helper method for form lookup selections
        private void PopulateDropdownsSelectList(Admission? admission = null, bool includeAllBeds = false)
        {
            var patients = _context.Patients.ToList().Select(p => new {
                PatientId = p.PatientId,
                FullName = $"{p.FirstName} {p.LastName} (ID: #{p.PatientId})"
            });

            var doctors = _context.Doctors.ToList().Select(d => new {
                DoctorId = d.DoctorId,
                FullName = $"Dr. {d.FirstName} {d.LastName} ({d.Specialization})"
            });

            var bedQuery = _context.Beds.Include(b => b.Ward).AsQueryable();
            if (!includeAllBeds)
            {
                bedQuery = bedQuery.Where(b => b.Status == BedStatus.Available);
            }

            var bedsList = bedQuery.ToList().Select(b => new {
                BedId = b.BedId,
                Display = $"{b.Ward?.WardName} - Bed {b.BedNumber} (₹{b.PricePerDay}/day)"
            });

            ViewData["PatientId"] = admission == null ? new SelectList(patients, "PatientId", "FullName") : new SelectList(patients, "PatientId", "FullName", admission.PatientId);
            ViewData["DoctorId"] = admission == null ? new SelectList(doctors, "DoctorId", "FullName") : new SelectList(doctors, "DoctorId", "FullName", admission.DoctorId);
            ViewData["BedId"] = admission == null ? new SelectList(bedsList, "BedId", "Display") : new SelectList(bedsList, "BedId", "Display", admission.BedId);
        }

        private bool AdmissionExists(int id)
        {
            return _context.Admissions.Any(e => e.AdmissionId == id);
        }
    }
}