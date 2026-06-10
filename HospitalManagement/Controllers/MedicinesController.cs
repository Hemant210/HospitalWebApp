using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.AllStaff)] // Everyone logged in can see stock
    public class MedicinesController : Controller
    {
        private readonly HospitalDbContext _context;

        public MedicinesController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Medicines (With Search & Category Filter)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var medicinesQuery = from m in _context.Medicines select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                medicinesQuery = medicinesQuery.Where(m => m.Name.Contains(searchString)
                                                        || m.Category.Contains(searchString));
            }

            return View(await medicinesQuery.AsNoTracking().ToListAsync());
        }

        // GET: Medicines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == id);

            if (medicine == null) return NotFound();

            return View(medicine);
        }

        // GET: Medicines/Create
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public IActionResult Create() => View();

        // POST: Medicines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public async Task<IActionResult> Create(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        // GET: Medicines/Edit/5
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return NotFound();

            return View(medicine);
        }

        // POST: Medicines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public async Task<IActionResult> Edit(int id, Medicine medicine)
        {
            if (id != medicine.MedicineId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicineExists(medicine.MedicineId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        // GET: Medicines/Delete/5
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == id);

            if (medicine == null) return NotFound();

            return View(medicine);
        }

        // POST: Medicines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MedicineExists(int id)
        {
            return _context.Medicines.Any(e => e.MedicineId == id);
        }
    }
}