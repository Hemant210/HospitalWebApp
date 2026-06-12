using HospitalManagement.Auth;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api; // Add this at the top

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = HospitalRoles.AdminOrReceptionist)] // Strictly Finance/Front-desk
    public class BillsController : Controller
    {
        private readonly HospitalDbContext _context;

        public BillsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Bills (With Search & Status Filtering)
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var billsQuery = _context.Bills
                .Include(b => b.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                billsQuery = billsQuery.Where(b =>
                    b.Patient.FirstName.Contains(searchString) ||
                    b.Patient.LastName.Contains(searchString) ||
                    b.BillId.ToString() == searchString ||
                    b.PaymentStatus.ToString().Contains(searchString));
            }

            // Order by most recent bills first
            return View(await billsQuery.OrderByDescending(b => b.BillDate).AsNoTracking().ToListAsync());
        }

        // GET: Bills/Details/5 (The Invoice View)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.Admission)
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(m => m.BillId == id);

            if (bill == null) return NotFound();

            return View(bill);
        }

        // GET: Bills/GenerateFromAdmission/5
        public async Task<IActionResult> GenerateFromAdmission(int id)
        {
            // 1. 🚨 DOUBLE-BILLING FIREWALL: Check if an invoice ALREADY exists for this admission
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.AdmissionId == id);
            if (existingBill != null)
            {
                // If a bill exists (paid or unpaid), safely route them to the existing invoice details!
                return RedirectToAction(nameof(Details), new { id = existingBill.BillId });
            }

            // 2. Fetch the Admission and ALL related data
            var admission = await _context.Admissions
                .Include(a => a.Bed)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.MedicalRecords)
                        .ThenInclude(mr => mr.LabTests)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.MedicalRecords)
                        .ThenInclude(mr => mr.Prescriptions)
                            .ThenInclude(pr => pr.PrescriptionItems)
                                .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null) return NotFound();

            // 3. Safety Check: Patient must be discharged to generate a final bill
            if (!admission.DischargeDate.HasValue)
            {
                TempData["ErrorMessage"] = "Cannot generate bill. Patient must be officially discharged first.";
                return RedirectToAction("Index", "Admissions");
            }

            // 4. Calculate Room Charges (Minimum 1 day charge)
            int daysStayed = (admission.DischargeDate.Value.Date - admission.AdmitDate.Date).Days;
            if (daysStayed == 0) daysStayed = 1;
            decimal totalRoomCharges = daysStayed * (admission.Bed?.PricePerDay ?? 0);

            // 5. Filter Medical Records to ONLY those created during this specific hospital stay
            var recordsDuringStay = admission.Patient?.MedicalRecords
                .Where(r => r.VisitDate.Date >= admission.AdmitDate.Date &&
                            r.VisitDate.Date <= admission.DischargeDate.Value.Date).ToList();

            // 6. Calculate Lab Charges
            decimal totalLabCharges = recordsDuringStay?
                .SelectMany(r => r.LabTests).Sum(l => l.Cost) ?? 0;

            // 7. Calculate Medicine Charges (Assuming Quantity needed = DurationDays)
            decimal totalMedicineCharges = recordsDuringStay?
                .SelectMany(r => r.Prescriptions)
                .SelectMany(p => p.PrescriptionItems)
                .Sum(pi => pi.DurationDays * (pi.Medicine?.UnitPrice ?? 0)) ?? 0;

            // 8. Create the Bill object
            var bill = new Bill
            {
                PatientId = admission.PatientId,
                AdmissionId = admission.AdmissionId,
                DoctorFee = 500, // Standard fixed consultation fee
                RoomCharges = totalRoomCharges,
                LabCharges = totalLabCharges,
                MedicineCharges = totalMedicineCharges,
                TotalAmount = 500 + totalRoomCharges + totalLabCharges + totalMedicineCharges,
                BillDate = DateTime.Now,
                PaymentStatus = PaymentStatus.Unpaid
            };

            return View("Create", bill); // Send them to the Create screen to review the math
        }

        // Add this action inside your BillsController
        [HttpPost]
        public IActionResult CreateRazorpayOrder(decimal amount, string receiptId)
        {
            // WARNING: In production, put these keys in appsettings.json!
            string key = "rzp_test_T0qaSLRiXeKGgW";
            string secret = "LgoEWHaFnfoolyTSBm0deeuB";

            try
            {
                RazorpayClient client = new RazorpayClient(key, secret);

                // Razorpay expects the amount in PAISE (multiply by 100)
                decimal amountInPaise = amount * 100;

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amountInPaise);
                options.Add("receipt", receiptId);
                options.Add("currency", "INR");

                Order order = client.Order.Create(options);

                return Json(new { orderId = order["id"], key = key });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: Bills/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bill bill)
        {
            if (ModelState.IsValid)
            {
                bill.TotalAmount = (bill.DoctorFee + bill.RoomCharges + bill.LabCharges + bill.MedicineCharges) - bill.Discount;

                if (bill.PaidAmount >= bill.TotalAmount)
                    bill.PaymentStatus = PaymentStatus.Paid;
                else if (bill.PaidAmount > 0)
                    bill.PaymentStatus = PaymentStatus.Partial;
                else
                    bill.PaymentStatus = PaymentStatus.Unpaid;

                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = bill.BillId });
            }

            return View(bill);
        }

        // GET: Bills/Edit/5 (Used strictly for RECEIVING PAYMENTS)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills.Include(b => b.Patient).FirstOrDefaultAsync(b => b.BillId == id);
            if (bill == null) return NotFound();

            return View(bill);
        }

        // POST: Bills/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bill bill)
        {
            if (id != bill.BillId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBill = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(b => b.BillId == id);
                    if (existingBill == null) return NotFound();

                    // Security: Re-calculate the total to prevent HTML manipulation hacks
                    bill.TotalAmount = (existingBill.DoctorFee + existingBill.RoomCharges + existingBill.LabCharges + existingBill.MedicineCharges) - bill.Discount;

                    if (bill.PaidAmount >= bill.TotalAmount)
                        bill.PaymentStatus = PaymentStatus.Paid;
                    else if (bill.PaidAmount > 0)
                        bill.PaymentStatus = PaymentStatus.Partial;
                    else
                        bill.PaymentStatus = PaymentStatus.Unpaid;

                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(bill.BillId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id = bill.BillId });
            }
            return View(bill);
        }

        // GET: Bills/Delete/5
        [Authorize(Roles = HospitalRoles.Admin)] // ONLY Admin can void a financial record
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(m => m.BillId == id);

            if (bill == null) return NotFound();

            return View(bill);
        }

        // POST: Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = HospitalRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.BillId == id);
        }
    }
}