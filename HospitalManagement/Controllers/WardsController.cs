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
    [Authorize(Roles = HospitalRoles.Admin)]
    public class WardsController : Controller
    {
        private readonly HospitalDbContext _context;

        public WardsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Wards
        public async Task<IActionResult> Index()
        {
            return View(await _context.Wards.ToListAsync());
        }

        // GET: Wards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ward = await _context.Wards
                .FirstOrDefaultAsync(m => m.WardId == id);
            if (ward == null)
            {
                return NotFound();
            }

            return View(ward);
        }

        // GET: Wards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Wards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WardId,WardName,WardType,TotalBeds")] Ward ward)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ward);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }

        // GET: Wards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
            {
                return NotFound();
            }
            return View(ward);
        }

        // POST: Wards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WardId,WardName,WardType,TotalBeds")] Ward ward)
        {
            if (id != ward.WardId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ward);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WardExists(ward.WardId))
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
            return View(ward);
        }

        // GET: Wards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ward = await _context.Wards
                .FirstOrDefaultAsync(m => m.WardId == id);
            if (ward == null)
            {
                return NotFound();
            }

            return View(ward);
        }

        // POST: Wards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                _context.Wards.Remove(ward);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WardExists(int id)
        {
            return _context.Wards.Any(e => e.WardId == id);
        }

        // ---------------------------------------------------
        // 📱 iOS API ENDPOINTS (FULL CRUD)
        // ---------------------------------------------------

        public class ApiWardDto
        {
            public int WardId { get; set; }
            public string WardName { get; set; } = string.Empty;
            public string? WardType { get; set; }
            public int TotalBeds { get; set; }
        }

        [HttpGet("/api/wards")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWardsApi()
        {
            var wards = await _context.Wards
                .Select(w => new ApiWardDto
                {
                    WardId = w.WardId,
                    WardName = w.WardName,
                    WardType = w.WardType,
                    TotalBeds = w.TotalBeds
                }).AsNoTracking().ToListAsync();
            return Ok(wards);
        }

        [HttpPost("/api/wards")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateWardApi([FromBody] ApiWardDto dto)
        {
            var ward = new Ward
            {
                WardName = dto.WardName,
                WardType = dto.WardType,
                TotalBeds = dto.TotalBeds
            };
            _context.Wards.Add(ward);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("/api/wards/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateWardApi(int id, [FromBody] ApiWardDto dto)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null) return NotFound();

            ward.WardName = dto.WardName;
            ward.WardType = dto.WardType;
            ward.TotalBeds = dto.TotalBeds;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("/api/wards/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteWardApi(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null) return NotFound();

            _context.Wards.Remove(ward);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
