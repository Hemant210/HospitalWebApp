using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers
{
    // All staff can view the list, but we will restrict the Create/Edit methods below
    [Authorize(Roles = HospitalRoles.AllStaff)]
    public class MedicinesController : Controller
    {
        private readonly HospitalDbContext _context;

        public MedicinesController(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() => View(await _context.Medicines.ToListAsync());

        // STRICT SECURITY: Only Pharmacist or Admin can add new drugs
        [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Pharmacist)]
        public IActionResult Create() => View();

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
    }
}