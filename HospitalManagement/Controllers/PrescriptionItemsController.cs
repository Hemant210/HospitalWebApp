using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.MedicalTeam)]
    public class PrescriptionItemsController : Controller
    {
        private readonly HospitalDbContext _context;

        public PrescriptionItemsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: PrescriptionItems
        public async Task<IActionResult> Index()
        {
            var hospitalDbContext = _context.PrescriptionItems.Include(p => p.Medicine).Include(p => p.Prescription);
            return View(await hospitalDbContext.ToListAsync());
        }

        // GET: PrescriptionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescriptionItem = await _context.PrescriptionItems
                .Include(p => p.Medicine)
                .Include(p => p.Prescription)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (prescriptionItem == null)
            {
                return NotFound();
            }

            return View(prescriptionItem);
        }

        // GET: PrescriptionItems/Create
        public IActionResult Create()
        {
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId");
            return View();
        }

        // POST: PrescriptionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,PrescriptionId,MedicineId,Dosage,DurationDays")] PrescriptionItem prescriptionItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prescriptionItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name", prescriptionItem.MedicineId);
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId", prescriptionItem.PrescriptionId);
            return View(prescriptionItem);
        }

        // GET: PrescriptionItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescriptionItem = await _context.PrescriptionItems.FindAsync(id);
            if (prescriptionItem == null)
            {
                return NotFound();
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name", prescriptionItem.MedicineId);
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId", prescriptionItem.PrescriptionId);
            return View(prescriptionItem);
        }

        // POST: PrescriptionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemId,PrescriptionId,MedicineId,Dosage,DurationDays")] PrescriptionItem prescriptionItem)
        {
            if (id != prescriptionItem.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prescriptionItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionItemExists(prescriptionItem.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name", prescriptionItem.MedicineId);
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId", prescriptionItem.PrescriptionId);
            return View(prescriptionItem);
        }

        // GET: PrescriptionItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescriptionItem = await _context.PrescriptionItems
                .Include(p => p.Medicine)
                .Include(p => p.Prescription)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (prescriptionItem == null)
            {
                return NotFound();
            }

            return View(prescriptionItem);
        }

        // POST: PrescriptionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescriptionItem = await _context.PrescriptionItems.FindAsync(id);
            if (prescriptionItem != null)
            {
                _context.PrescriptionItems.Remove(prescriptionItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionItemExists(int id)
        {
            return _context.PrescriptionItems.Any(e => e.ItemId == id);
        }
    }
}
