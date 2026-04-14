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
    public class LabTestsController : Controller
    {
        private readonly HospitalDbContext _context;

        public LabTestsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: LabTests
        public async Task<IActionResult> Index()
        {
            var hospitalDbContext = _context.LabTests.Include(l => l.MedicalRecord).Include(l => l.Patient).Include(l => l.Technician);
            return View(await hospitalDbContext.ToListAsync());
        }

        // GET: LabTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _context.LabTests
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .Include(l => l.Technician)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (labTest == null)
            {
                return NotFound();
            }

            return View(labTest);
        }

        // GET: LabTests/Create
        public IActionResult Create()
        {
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName");
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName");
            return View();
        }

        // POST: LabTests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,RecordId,PatientId,TechnicianId,TestName,Cost,Result,TestDate,Status")] LabTest labTest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labTest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
            return View(labTest);
        }

        // GET: LabTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _context.LabTests.FindAsync(id);
            if (labTest == null)
            {
                return NotFound();
            }
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
            return View(labTest);
        }

        // POST: LabTests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TestId,RecordId,PatientId,TechnicianId,TestName,Cost,Result,TestDate,Status")] LabTest labTest)
        {
            if (id != labTest.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabTestExists(labTest.TestId))
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
            ViewData["RecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labTest.RecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", labTest.PatientId);
            ViewData["TechnicianId"] = new SelectList(_context.Staffs, "StaffId", "FirstName", labTest.TechnicianId);
            return View(labTest);
        }

        // GET: LabTests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labTest = await _context.LabTests
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .Include(l => l.Technician)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (labTest == null)
            {
                return NotFound();
            }

            return View(labTest);
        }

        // POST: LabTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labTest = await _context.LabTests.FindAsync(id);
            if (labTest != null)
            {
                _context.LabTests.Remove(labTest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LabTestExists(int id)
        {
            return _context.LabTests.Any(e => e.TestId == id);
        }
    }
}
