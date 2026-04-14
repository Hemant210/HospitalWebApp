using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    // Receptionists and Admins handle admitting patients
    [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
    public class AdmissionsController : Controller
    {
        private readonly HospitalDbContext _context;

        public AdmissionsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Admissions
        public async Task<IActionResult> Index()
        {
            // Fetch admissions along with Patient, Doctor, and Bed details
            var admissions = _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward); // Grab the Ward info tied to the bed

            return View(await admissions.ToListAsync());
        }

        // GET: Admissions/Create
        public IActionResult Create()
        {
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName");
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");

            // AUTOMATION LOGIC: Only show beds that are currently 'Available'
            var availableBeds = _context.Beds
                .Include(b => b.Ward)
                .Where(b => b.Status == BedStatus.Available)
                .Select(b => new {
                    BedId = b.BedId,
                    Display = $"{b.Ward.WardName} - Bed {b.BedNumber}"
                }).ToList();

            ViewData["BedId"] = new SelectList(availableBeds, "BedId", "Display");
            return View();
        }

        // POST: Admissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Admission admission)
        {
            if (ModelState.IsValid)
            {
                // 1. Save the admission
                _context.Add(admission);

                // 2. AUTOMATION LOGIC: Find the bed and mark it as Occupied
                var bed = await _context.Beds.FindAsync(admission.BedId);
                if (bed != null)
                {
                    bed.Status = BedStatus.Occupied;
                    _context.Update(bed);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we fail, reload the dropdowns
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", admission.PatientId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", admission.DoctorId);
            return View(admission);
        }

        // GET: Admissions/Edit/5 (This is used for Discharging the patient!)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var admission = await _context.Admissions.FindAsync(id);
            if (admission == null) return NotFound();

            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", admission.PatientId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", admission.DoctorId);

            // We fetch all beds here just in case they need to switch rooms
            var allBeds = _context.Beds.Include(b => b.Ward)
                .Select(b => new { BedId = b.BedId, Display = $"{b.Ward.WardName} - Bed {b.BedNumber}" }).ToList();
            ViewData["BedId"] = new SelectList(allBeds, "BedId", "Display", admission.BedId);

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

                    // AUTOMATION LOGIC: If a Discharge Date is entered, free up the bed!
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
            return View(admission);
        }

        private bool AdmissionExists(int id)
        {
            return _context.Admissions.Any(e => e.AdmissionId == id);
        }
    }
}