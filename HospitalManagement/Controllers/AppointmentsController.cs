using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.AllStaff)] // All authenticated personnel can monitor scheduling queues
    public class AppointmentsController : Controller
    {
        private readonly HospitalDbContext _context;

        public AppointmentsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Appointments (With Deep Filter Searching)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var appointmentsQuery = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                // Deep filter query across transactional entities
                appointmentsQuery = appointmentsQuery.Where(a =>
                    a.Patient.FirstName.Contains(searchString) ||
                    a.Patient.LastName.Contains(searchString) ||
                    a.Doctor.FirstName.Contains(searchString) ||
                    a.Doctor.LastName.Contains(searchString) ||
                    a.Status.ToString().Contains(searchString));
            }

            // Order chronological queues with upcoming slots prioritized
            return View(await appointmentsQuery.OrderBy(a => a.AppointmentDate).AsNoTracking().ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public IActionResult Create()
        {
            PopulateDropdownsSelectList();
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdownsSelectList(appointment);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            PopulateDropdownsSelectList(appointment);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.AppointmentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdownsSelectList(appointment);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Reusable Helper to safely aggregate Full Names inside selection drops
        private void PopulateDropdownsSelectList(Appointment? appointment = null)
        {
            var doctors = _context.Doctors.ToList().Select(d => new {
                DoctorId = d.DoctorId,
                FullName = $"Dr. {d.FirstName} {d.LastName} ({d.Specialization})"
            });

            var patients = _context.Patients.ToList().Select(p => new {
                PatientId = p.PatientId,
                FullName = $"{p.FirstName} {p.LastName} (ID: #{p.PatientId})"
            });

            ViewData["DoctorId"] = appointment == null
                ? new SelectList(doctors, "DoctorId", "FullName")
                : new SelectList(doctors, "DoctorId", "FullName", appointment.DoctorId);

            ViewData["PatientId"] = appointment == null
                ? new SelectList(patients, "PatientId", "FullName")
                : new SelectList(patients, "PatientId", "FullName", appointment.PatientId);
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }
    }
}