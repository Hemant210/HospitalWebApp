using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.Admin)] // Admin handles HR
    public class StaffsController : Controller
    {
        private readonly HospitalDbContext _context;

        public StaffsController(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staff = _context.Staffs.Include(s => s.Department);
            return View(await staff.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DeptName", staff.DepartmentId);
            return View(staff);
        }
    }
}