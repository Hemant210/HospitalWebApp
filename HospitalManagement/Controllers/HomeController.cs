using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HospitalManagement.Controllers
{
    [Authorize] // Requires login to view the dashboard
    public class HomeController : Controller
    {
        private readonly HospitalDbContext _context;

        // Inject the database context
        public HomeController(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Count Total Patients
            ViewBag.TotalPatients = await _context.Patients.CountAsync();

            // 2. Count Beds where Status is "Available"
            ViewBag.AvailableBeds = await _context.Beds.CountAsync(b => b.Status == BedStatus.Available);

            // 3. Count Appointments scheduled for Today
            ViewBag.TodaysAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == DateTime.Today.Date);

            // 4. Count Lab Tests that are still "Pending"
            ViewBag.PendingLabs = await _context.LabTests
                .CountAsync(l => l.Status == LabTestStatus.Pending);

            // 5. NEW: Fetch 5 Doctors for the "Available Specialists" widget
            ViewBag.AvailableDoctors = await _context.Doctors
                .Include(d => d.Department)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}