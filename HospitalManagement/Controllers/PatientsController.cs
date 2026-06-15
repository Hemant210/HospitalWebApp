using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.Admin + "," + HospitalRoles.Receptionist + "," + HospitalRoles.Doctor)]
    public class PatientsController : Controller
    {
        private readonly HospitalDbContext _context;
        private readonly UserManager<AppUser> _userManager; // 1. Add the private field

        // 2. Add UserManager to the constructor so ASP.NET can inject it
        public PatientsController(HospitalDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Assign it so your Create method can use it
        }

        // GET: Patients (With Search Filter)
        public async Task<IActionResult> Index(string searchString)
        // ... the rest of your controller stays exactly the same
        {
            ViewData["CurrentFilter"] = searchString;

            var patientsQuery = from p in _context.Patients select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                patientsQuery = patientsQuery.Where(p => p.FirstName.Contains(searchString)
                                                      || p.LastName.Contains(searchString)
                                                      || p.Phone.Contains(searchString));
            }

            return View(await patientsQuery.AsNoTracking().ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.PatientId == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public IActionResult Create() => View();

        // Make sure to inject UserManager<AppUser> at the top of your controller!

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Create([Bind("PatientId,FirstName,LastName,DOB,Gender,Phone,Email,Address,BloodGroup,InsuranceNo")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                // 1. Save the Medical Profile
                _context.Add(patient);
                await _context.SaveChangesAsync(); // This generates the new PatientId

                // 2. AUTOMATION: Auto-Generate the Patient's Login Portal Account!
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(patient.Email);
                    if (existingUser == null)
                    {
                        var newUser = new AppUser
                        {
                            UserName = patient.Email,
                            Email = patient.Email,
                            PhoneNumber = patient.Phone, // This becomes their password!
                            FullName = $"{patient.FirstName} {patient.LastName}",
                            Role = HospitalRoles.Patient,
                            LinkedId = patient.PatientId, // Links the login to the medical record
                            IsActive = true,
                            EmailConfirmed = true
                        };

                        // Create the user with a dummy secure password (since they log in via phone number anyway)
                        var result = await _userManager.CreateAsync(newUser, "PatientPortal@123!");
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(newUser, HospitalRoles.Patient);
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.AdminOrReceptionist)]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,FirstName,LastName,DOB,Gender,Phone,Email,Address,BloodGroup,InsuranceNo")] Patient patient)
        {
            if (id != patient.PatientId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.PatientId == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}