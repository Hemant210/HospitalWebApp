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
    public class DoctorsController : Controller
    {
        private readonly HospitalDbContext _context;

        public DoctorsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Doctors (with advanced search filtering)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Include Department data to avoid lazy-loading issues in the view
            var doctorsQuery = _context.Doctors.Include(d => d.Department).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                doctorsQuery = doctorsQuery.Where(d => d.FirstName.Contains(searchString)
                                                    || d.LastName.Contains(searchString)
                                                    || d.Specialization.Contains(searchString));
            }

            return View(await doctorsQuery.AsNoTracking().ToListAsync());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = HospitalRoles.Admin)]
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName");
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> Create([Bind("DoctorId,FirstName,LastName,Specialization,Phone,Email,DepartmentId")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName", doctor.DepartmentId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName", doctor.DepartmentId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorId,FirstName,LastName,Specialization,Phone,Email,DepartmentId")] Doctor doctor)
        {
            if (id != doctor.DoctorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName", doctor.DepartmentId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }

        // ---------------------------------------------------
        // 📱 iOS API ENDPOINTS (FULL CRUD)
        // ---------------------------------------------------

        public class ApiDoctorDto
        {
            public int DoctorId { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? Specialization { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public int DepartmentId { get; set; }
            public string? DepartmentName { get; set; } // Sent to iOS for display
        }

        [HttpGet("/api/doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorsApi()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Select(d => new ApiDoctorDto
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Specialization = d.Specialization,
                    Phone = d.Phone,
                    Email = d.Email,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department.DeptName
                }).AsNoTracking().ToListAsync();
            return Ok(doctors);
        }

        [HttpPost("/api/doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateDoctorApi([FromBody] ApiDoctorDto dto)
        {
            var doctor = new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Specialization = dto.Specialization,
                Phone = dto.Phone,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("/api/doctors/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateDoctorApi(int id, [FromBody] ApiDoctorDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.FirstName = dto.FirstName;
            doctor.LastName = dto.LastName;
            doctor.Specialization = dto.Specialization;
            doctor.Phone = dto.Phone;
            doctor.Email = dto.Email;
            doctor.DepartmentId = dto.DepartmentId;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("/api/doctors/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteDoctorApi(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}