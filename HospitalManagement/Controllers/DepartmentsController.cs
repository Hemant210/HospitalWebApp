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

        public async Task<IActionResult> Index() => View(await _context.Departments.ToListAsync());

        public IActionResult Create() => View();

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

        // Edit, Details, and Delete can be scaffolded by Visual Studio!
    }
}