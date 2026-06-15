//using HospitalManagement.Auth;
//using HospitalManagement.Data;
//using HospitalManagement.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HospitalManagement.Controllers
//{
//    [Authorize(Roles = HospitalRoles.Admin)] // STRICT SECURITY: Admin Only!
//    public class DepartmentsController : Controller
//    {
//        private readonly HospitalDbContext _context;

//        public DepartmentsController(HospitalDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index() => View(await _context.Departments.ToListAsync());

//        public IActionResult Create() => View();

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Department department)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(department);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(department);
//        }

//        // Edit, Details, and Delete can be scaffolded by Visual Studio!
//    }
//}




using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.Admin)] // STRICT SECURITY: Admin Only!
    public class DepartmentsController : Controller
    {
        private readonly HospitalDbContext _context;

        public DepartmentsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Departments (with filtering logic)
        public async Task<IActionResult> Index(string searchString)
        {
            // Keep the search term visible inside the input box after filtering clears
            ViewData["CurrentFilter"] = searchString;

            var departments = from d in _context.Departments select d;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Filter gracefully by either Name or Location field matches
                departments = departments.Where(d => d.DeptName.Contains(searchString)
                                                  || d.Location.Contains(searchString));
            }

            return View(await departments.AsNoTracking().ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments
                .Include(d => d.Doctors) // Optional link inclusion for audit metrics
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null) return NotFound();

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create() => View();

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }
        // ---------------------------------------------------
        // 📱 iOS API ENDPOINTS (FULL CRUD)
        // ---------------------------------------------------

        public class ApiDepartmentDto
        {
            public int DepartmentId { get; set; }
            public string DeptName { get; set; } = string.Empty;
            public string? Location { get; set; }
        }

        [HttpGet("/api/departments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDepartmentsApi()
        {
            var departments = await _context.Departments
                .Select(d => new ApiDepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DeptName = d.DeptName,
                    Location = d.Location
                }).AsNoTracking().ToListAsync();
            return Ok(departments);
        }

        [HttpPost("/api/departments")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateDepartmentApi([FromBody] ApiDepartmentDto dto)
        {
            var dept = new Department { DeptName = dto.DeptName, Location = dto.Location };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("/api/departments/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateDepartmentApi(int id, [FromBody] ApiDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            dept.DeptName = dto.DeptName;
            dept.Location = dto.Location;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("/api/departments/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteDepartmentApi(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }

}