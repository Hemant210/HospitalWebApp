using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.AllStaff)]
    public class AppointmentsController : Controller
    {
        private readonly HospitalDbContext _context;

        public AppointmentsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            // Include both Patient and Doctor data so the table shows names, not just IDs
            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient);
            return View(await appointments.ToListAsync());
        }

        // GET: Appointments/Create
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)] // Receptionists handle booking
        public IActionResult Create()
        {
            // Creates dropdowns for selecting the Doctor and Patient
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName");
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
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FirstName", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "FirstName", appointment.PatientId);
            return View(appointment);
        }
    }
}