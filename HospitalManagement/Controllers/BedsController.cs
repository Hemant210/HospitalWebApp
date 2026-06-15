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
    public class BedsController : Controller
    {
        private readonly HospitalDbContext _context;

        public BedsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Beds
        public async Task<IActionResult> Index()
        {
            var hospitalDbContext = _context.Beds.Include(b => b.Ward);
            return View(await hospitalDbContext.ToListAsync());
        }

        // GET: Beds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bed = await _context.Beds
                .Include(b => b.Ward)
                .FirstOrDefaultAsync(m => m.BedId == id);
            if (bed == null)
            {
                return NotFound();
            }

            return View(bed);
        }

        // GET: Beds/Create
        public IActionResult Create()
        {
            ViewData["WardId"] = new SelectList(_context.Wards, "WardId", "WardName");
            return View();
        }

        // POST: Beds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BedId,WardId,BedNumber,PricePerDay,Status")] Bed bed)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WardId"] = new SelectList(_context.Wards, "WardId", "WardName", bed.WardId);
            return View(bed);
        }

        // GET: Beds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bed = await _context.Beds.FindAsync(id);
            if (bed == null)
            {
                return NotFound();
            }
            ViewData["WardId"] = new SelectList(_context.Wards, "WardId", "WardName", bed.WardId);
            return View(bed);
        }

        // POST: Beds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BedId,WardId,BedNumber,PricePerDay,Status")] Bed bed)
        {
            if (id != bed.BedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BedExists(bed.BedId))
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
            ViewData["WardId"] = new SelectList(_context.Wards, "WardId", "WardName", bed.WardId);
            return View(bed);
        }

        // GET: Beds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bed = await _context.Beds
                .Include(b => b.Ward)
                .FirstOrDefaultAsync(m => m.BedId == id);
            if (bed == null)
            {
                return NotFound();
            }

            return View(bed);
        }

        // POST: Beds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                _context.Beds.Remove(bed);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.BedId == id);
        }

        // ---------------------------------------------------
        // 📱 iOS API ENDPOINTS (FULL CRUD)
        // ---------------------------------------------------

        public class ApiBedDto
        {
            public int BedId { get; set; }
            public int WardId { get; set; }
            public string? WardName { get; set; } // Sent to iOS for display
            public string? BedNumber { get; set; }
            public decimal PricePerDay { get; set; }
            public int Status { get; set; } // 0=Available, 1=Occupied, 2=Maintenance
        }

        [HttpGet("/api/beds")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBedsApi()
        {
            var beds = await _context.Beds
                .Include(b => b.Ward)
                .Select(b => new ApiBedDto
                {
                    BedId = b.BedId,
                    WardId = b.WardId,
                    WardName = b.Ward.WardName,
                    BedNumber = b.BedNumber,
                    PricePerDay = b.PricePerDay,
                    Status = (int)b.Status
                }).AsNoTracking().ToListAsync();
            return Ok(beds);
        }

        [HttpPost("/api/beds")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateBedApi([FromBody] ApiBedDto dto)
        {
            var bed = new Bed
            {
                WardId = dto.WardId,
                BedNumber = dto.BedNumber,
                PricePerDay = dto.PricePerDay,
                Status = (BedStatus)dto.Status
            };
            _context.Beds.Add(bed);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("/api/beds/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateBedApi(int id, [FromBody] ApiBedDto dto)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();

            bed.WardId = dto.WardId;
            bed.BedNumber = dto.BedNumber;
            bed.PricePerDay = dto.PricePerDay;
            bed.Status = (BedStatus)dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("/api/beds/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteBedApi(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();

            _context.Beds.Remove(bed);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
